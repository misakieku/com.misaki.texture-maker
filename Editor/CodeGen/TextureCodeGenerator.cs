using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker.CodeGen
{
    internal class TextureCodeGenerator
    {
        public string GenerateExecutionCode(List<ITextureExecutable> executionOrder, string outputDirectory = "Assets/Generated")
        {
            var context = new CodeGenContext();

            // Add common usings
            context.AddUsing("UnityEngine");
            context.AddUsing("Unity.Mathematics");
            context.AddUsing("Unity.Jobs");
            context.AddUsing("Unity.Collections");
            context.AddUsing("Unity.Burst");
            context.AddUsing("UnityEditor");
            context.AddUsing("Misaki.TextureMaker");

            // Generate data structure and initialization for all nodes
            GenerateDataStructures(context, executionOrder);

            // Generate prepare and finalize code
            GeneratePrepareAndFinalizeCode(context, executionOrder);

            // Generate execution code for each node
            GenerateExecutionLogic(context, executionOrder);

            // Generate dedicated job structs and helper classes for each output node
            GenerateJobStructsAndHelpers(context, executionOrder);

            // Generate the full code
            var generatedCode = context.GenerateFullCode();

            // Write to file for debugging
            WriteCodeToFile(generatedCode, outputDirectory);

            return generatedCode;
        }

        private void GenerateDataStructures(CodeGenContext context, List<ITextureExecutable> executionOrder)
        {
            for (var i = 0; i < executionOrder.Count; i++)
            {
                var node = executionOrder[i];
                var nodeId = $"node_{i}";

                if (node is ICodeGeneratable codeGen)
                {
                    codeGen.GenerateDataFields(context, nodeId);
                    codeGen.GenerateDataInitialization(context, nodeId);
                }
            }
        }

        private void GeneratePrepareAndFinalizeCode(CodeGenContext context, List<ITextureExecutable> executionOrder)
        {
            for (var i = 0; i < executionOrder.Count; i++)
            {
                var node = executionOrder[i];
                var nodeId = $"node_{i}";

                if (node is ICodeGeneratable codeGen)
                {
                    codeGen.GeneratePrepareCode(context, nodeId);
                    codeGen.GenerateFinalizeCode(context, nodeId);
                }
            }
        }

        private void GenerateExecutionLogic(CodeGenContext context, List<ITextureExecutable> executionOrder)
        {
            for (var i = 0; i < executionOrder.Count; i++)
            {
                var node = executionOrder[i];
                var nodeId = $"node_{i}";

                // Generate execution code
                if (node is ICodeGeneratable codeGen)
                {
                    codeGen.GenerateCode(context, nodeId);
                }
                else
                {
                    // Fallback for nodes without code generation
                    GenerateFallbackCode(context, node, nodeId);
                }
            }

            // Return the final output - for output nodes we don't return a value
            var finalNode = executionOrder.LastOrDefault();
            if (finalNode is OutputNode)
            {
                context.AddLine("return Color.black; // Output nodes write to their output arrays");
            }
            else if (finalNode != null)
            {
                // Try to find the main output port
                var mainOutputPort = finalNode.GetOutputPorts().FirstOrDefault(p => p.name == "Output") 
                                   ?? finalNode.GetOutputPorts().FirstOrDefault();
                
                if (mainOutputPort != null)
                {
                    var finalOutput = context.GetOutputVariable(mainOutputPort);
                    context.AddLine($"return {finalOutput};");
                }
                else
                {
                    context.AddLine("return Color.black;");
                }
            }
            else
            {
                context.AddLine("return Color.black;");
            }
        }

        private void GenerateJobStructsAndHelpers(CodeGenContext context, List<ITextureExecutable> executionOrder)
        {
            // Find all output nodes
            var outputNodes = executionOrder.Where((node, index) => node is OutputNode)
                                          .Select((node, index) => new { Node = node, Index = executionOrder.IndexOf(node) })
                                          .ToList();

            foreach (var outputNode in outputNodes)
            {
                var nodeId = $"node_{outputNode.Index}";
                var className = $"TextureExecution_{nodeId}";
                
                context.SetOutputNodeInfo(nodeId, className);

                // Generate dedicated job struct for this output node
                var jobStruct = GenerateJobStruct(nodeId, className);
                context.AddJobStruct(nodeId, jobStruct);

                // Generate helper class with Run method
                GenerateHelperClass(context, nodeId, className);
            }
        }

        private string GenerateJobStruct(string nodeId, string className)
        {
            return $@"        [Unity.Burst.BurstCompile]
        public struct {className}Job : Unity.Jobs.IJobParallelFor
        {{
            public TextureExecutionData data;
            public Unity.Collections.NativeArray<UnityEngine.Color> output;
            public int width;
            public int height;

            public void Execute(int index)
            {{
                var y = index / width;
                var x = index % width;
                var uv = new UnityEngine.Vector2((float)x / (width - 1), (float)y / (height - 1));
                output[index] = ExecutePixel(uv, ref data);
            }}
        }}";
        }

        private void GenerateHelperClass(CodeGenContext context, string nodeId, string className)
        {
            context.AddHelperLine($"public static class {className}Helper");
            context.AddHelperLine("{");
            context.AddHelperLine($"    public static Unity.Jobs.JobHandle Run(int width, int height, Unity.Collections.NativeArray<UnityEngine.Color> output, Unity.Jobs.JobHandle dependsOn = default)");
            context.AddHelperLine("    {");
            context.AddHelperLine("        var data = GeneratedTextureExecution.Initialize();");
            context.AddHelperLine("        GeneratedTextureExecution.Prepare(ref data);");
            context.AddHelperLine("");
            context.AddHelperLine($"        var job = new GeneratedTextureExecution.{className}Job");
            context.AddHelperLine("        {");
            context.AddHelperLine("            data = data,");
            context.AddHelperLine("            output = output,");
            context.AddHelperLine("            width = width,");
            context.AddHelperLine("            height = height");
            context.AddHelperLine("        };");
            context.AddHelperLine("");
            context.AddHelperLine("        var handle = job.Schedule(width * height, 64, dependsOn);");
            context.AddHelperLine("        handle.Complete();");
            context.AddHelperLine("");
            context.AddHelperLine("        GeneratedTextureExecution.Finalize(ref data);");
            context.AddHelperLine("        return handle;");
            context.AddHelperLine("    }");
            context.AddHelperLine("}");
            context.AddHelperLine("");
        }

        private void GenerateFallbackCode(CodeGenContext context, ITextureExecutable node, string nodeId)
        {
            context.AddLine($"// Fallback execution for {node.GetType().Name} {nodeId}");
            context.AddLine($"// TODO: Implement code generation for {node.GetType().Name}");

            // Generate default output variables
            foreach (var port in node.GetOutputPorts())
            {
                var outputVar = context.GetOutputVariable(port);
                var defaultValue = GetDefaultOutputValue(port.dataType);
                context.DeclareVariable(GetPortTypeName(port.dataType), outputVar, defaultValue);
                context.RegisterOutputVariable(port, outputVar);
            }
        }

        private string GetDefaultOutputValue(Type portType)
        {
            if (portType == typeof(Color))
                return "Color.black";
            if (portType == typeof(float))
                return "0f";
            if (portType == typeof(Vector2))
                return "Vector2.zero";
            if (portType == typeof(int))
                return "0";
            return "default";
        }

        private string GetPortTypeName(Type portType)
        {
            if (portType == typeof(Color))
                return "Color";
            if (portType == typeof(float))
                return "float";
            if (portType == typeof(Vector2))
                return "Vector2";
            if (portType == typeof(int))
                return "int";
            if (portType == typeof(TextureData))
                return "TextureData";
            return portType.Name;
        }

        private void WriteCodeToFile(string generatedCode, string outputDirectory)
        {
            try
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                var fileName = $"GeneratedTextureExecution_{DateTime.Now:yyyyMMdd_HHmmss}.cs";
                var filePath = Path.Combine(outputDirectory, fileName);

                File.WriteAllText(filePath, generatedCode);

                UnityEngine.Debug.Log($"Generated texture execution code written to: {filePath}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to write generated code to file: {ex.Message}");
            }
        }
    }
}