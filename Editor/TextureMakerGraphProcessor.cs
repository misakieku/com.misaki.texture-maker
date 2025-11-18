using Misaki.GraphProcessor.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class TextureMakerGraphProcessor : IGraphProcessor
    {
        public struct BuildOption : IBuildOption
        {
            public string outputName;
        }

        private readonly Graph _graph;
        private readonly ISorter _sorter;
        private readonly Dictionary<OutputNode, List<INode>> _processed;
        private readonly HashSet<INode> _visitedNodes;

        private BuildOption _option;

        public TextureMakerGraphProcessor(Graph graph)
        {
            _graph = graph;
            _sorter = new TopologicalSorter();
            _processed = new();
            _visitedNodes = new();
        }

        public void BuildGraph<T>(in T buildOption)
            where T : IBuildOption
        {
            if (buildOption is not BuildOption option)
            {
                throw new ArgumentException($"Invalid build option type: {typeof(T)}");
            }

            _option = option;

            var nodes = _graph.GetNodes().OfType<OutputNode>();

            _processed.Clear();
            _visitedNodes.Clear();

            foreach (var node in nodes)
            {
                var processedNodes = _sorter.Sort(node);
                _processed[node] = processedNodes;

                foreach (var n in processedNodes)
                {
                    _visitedNodes.Add(n);
                }
            }
        }

        public void ExecuteGraph()
        {
            ValidateExecutionState();

            var library = new ShaderLibrary();
            var compiler = new InstructionCompiler(library);
            library.AddInclude("Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl");

            InitializeNodes(library);
            PopulateInstructions(compiler);

            var code = compiler.CompileToShader();
            var computeShader = CreateComputeShader(code);

            DispatchShader(library, computeShader);
            CleanupNodes(computeShader);
        }

        private void ValidateExecutionState()
        {
            if (string.IsNullOrEmpty(_option.outputName))
            {
                throw new InvalidOperationException("Output path is not specified.");
            }

            if (_processed.Count == 0 || _visitedNodes.Count == 0)
            {
                throw new InvalidOperationException("Graph is not built. Please call BuildGraph() before ExecuteGraph().");
            }
        }

        private static void GenerateSubgraphFunction(IShaderLibrary library, ISubgraphNode subgraphNode)
        {
            var funcName = CodeGenUtility.GetSubGraphFunctionName(subgraphNode);
            var funcDecl = new FunctionDeclaration
            {
                name = funcName,
                signature = new(),
                returnType = ShaderVariableType.Void,
            };

            var inputs = new List<string>();
            foreach (var inputPort in subgraphNode.GetInputPorts())
            {
                var name = CodeGenUtility.DisplayNameToCodeFriendlyName(inputPort.DisplayName);
                var paramType = inputPort.DataType.ToShaderVariableType();

                funcDecl.signature.Add(new ParameterDeclaration
                {
                    name = CodeGenUtility.DisplayNameToCodeFriendlyName(inputPort.DisplayName),
                    type = paramType,
                    modifier = ParameterModifier.In
                });

                inputs.Add(name);
            }

            var outputs = new List<string>();
            foreach (var outputPort in subgraphNode.GetOutputPorts())
            {
                var name = CodeGenUtility.DisplayNameToCodeFriendlyName(outputPort.DisplayName);
                var paramType = outputPort.DataType.ToShaderVariableType();

                funcDecl.signature.Add(new ParameterDeclaration
                {
                    name = name,
                    type = paramType,
                    modifier = ParameterModifier.Out
                });

                outputs.Add(name);
            }

            var inputArgsLookup = new Dictionary<IVariable, string>();
            var outputArgsLookup = new Dictionary<IVariable, string>();
            var variables = subgraphNode.GetSubgraph().GetVariables();

            var inputIndex = 0;
            var outputIndex = 0;
            foreach (var variable in variables)
            {
                switch (variable.VariableKind)
                {
                    case VariableKind.Input:
                        inputArgsLookup[variable] = inputs[inputIndex++];
                        break;
                    case VariableKind.Output:
                        outputArgsLookup[variable] = outputs[outputIndex++];
                        break;
                    default:
                        break;
                }
            }

            var nodes = subgraphNode.GetSubgraph().GetNodes();
            var ctx = new SubGraphCodeGenContext(inputArgsLookup, outputArgsLookup);

            foreach (var node in nodes)
            {
                switch (node)
                {
                    case ICodeGenerationNode codeGenNode:
                        codeGenNode.GenerateCode(ctx);
                        break;
                    case ISubgraphNode sgNode:
                        GenerateSubgraphFunction(library, sgNode);
                        GenerateSubgraphCall(ctx, sgNode);
                        break;
                    default:
                        break;
                }
            }

            foreach (var outNode in nodes.OfType<IVariableNode>())
            {
                if (outNode.Variable.VariableKind == VariableKind.Output)
                {
                    var valueName = ctx.GetOutputVariableName(outNode.GetInputPort(0).FirstConnectedPort);
                    ctx.AddInstruction(new Instruction
                    {
                        expression = new VariableExpr(valueName),
                        result = new VariableDeclaration
                        {
                            name = outputArgsLookup[outNode.Variable]
                        }
                    });
                }
            }

            funcDecl.code = InstructionCompiler.CompileInstructions(ctx.InstructionSet);
            library.AddFunction(funcDecl);
        }

        private void InitializeNodes(ShaderLibrary library)
        {
            foreach (var node in _visitedNodes)
            {
                switch (node)
                {
                    case ICodeGenerationNode codeGenNode:
                        codeGenNode.Initialize(library);
                        break;
                    case ISubgraphNode subgraphNode:
                        GenerateSubgraphFunction(library, subgraphNode);
                        break;
                    default:
                        break;
                }
            }
        }

        private void PopulateInstructions(InstructionCompiler compiler)
        {
            var index = 0;
            foreach (var kvp in _processed)
            {
                var ctx = new CodeGenContext();

                foreach (var node in kvp.Value)
                {
                    switch (node)
                    {
                        case ICodeGenerationNode codeGenNode:
                            codeGenNode.GenerateCode(ctx);
                            break;
                        case ISubgraphNode subgraphNode:
                            GenerateSubgraphCall(ctx, subgraphNode);
                            break;
                        default:
                            break;
                    }
                }

                compiler.AddContext(ctx, index++);
            }
        }

        private static void GenerateSubgraphCall(ICodeGenContext ctx, ISubgraphNode subgraphNode)
        {
            var inputs = new List<Expression>();
            foreach (var inputPort in subgraphNode.GetInputPorts())
            {
                var inputVar = ctx.GetInputVariableName(inputPort, inputPort.DataType.ToShaderVariableType(), data =>
                {
                    return CodeGenUtility.ToConstantExpr(data, inputPort.DataType.ToShaderVariableType());
                });
                inputs.Add(new VariableExpr(inputVar));
            }

            var outputs = new List<VariableDeclaration>();
            foreach (var outputPort in subgraphNode.GetOutputPorts())
            {
                var outputVarName = ctx.GetOutputVariableName(outputPort);
                outputs.Add(new VariableDeclaration
                {
                    type = outputPort.DataType.ToShaderVariableType(),
                    name = outputVarName
                });
            }

            ctx.AddInstruction(new Instruction
            {
                expression = new FunctionCallExpr(CodeGenUtility.GetSubGraphFunctionName(subgraphNode), inputs, outputs),
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.None,
                    name = string.Empty
                }
            });
        }

        private void CleanupNodes(ComputeShader computeShader)
        {
            foreach (var node in _visitedNodes)
            {
                if (node is ICodeGenerationNode codeGenNode)
                {
                    codeGenNode.Cleanup(computeShader);
                }
            }
        }

        private ComputeShader CreateComputeShader(string code)
        {
            const string outputDir = "Assets/Editor/TextureMaker/Generated/";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var hlslPath = Path.Combine(outputDir, _option.outputName + ".compute");
            File.WriteAllText(hlslPath, code);
            AssetDatabase.ImportAsset(hlslPath);
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<ComputeShader>(hlslPath);
        }

        private void DispatchShader(ShaderLibrary library, ComputeShader computeShader)
        {
            var kernelIndex = 0;
            foreach (var kvp in _processed)
            {
                var width = kvp.Key.Width;
                var height = kvp.Key.Height;

                computeShader.SetVector("textureSize", new Vector4(width, height, 1.0f / width, 1.0f / height));

                foreach (var variable in library.Variables)
                {
                    variable.bindingCallback?.Invoke(computeShader, kernelIndex, variable.declaration.name);
                }

                // Dispatch the compute shader
                var threadGroupsX = (width + InstructionCompiler.threadGroupSize.x - 1) / InstructionCompiler.threadGroupSize.x;
                var threadGroupsY = (height + InstructionCompiler.threadGroupSize.y - 1) / InstructionCompiler.threadGroupSize.y;
                computeShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, InstructionCompiler.threadGroupSize.z);

                kernelIndex++;
            }
        }
    }
}