using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Misaki.TextureMaker.CodeGen;
using Unity.GraphToolkit.Editor;

namespace Misaki.TextureMaker
{
    internal class ExportTexture2d : OutputNode
    {
        private IPort _inputPort;

        protected override void OnDefineInputPorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<Color>("Input").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<SupportedTextureFormat>("Format").WithDefaultValue(SupportedTextureFormat.RGBA32).Build();
            context.AddOption<string>("Output Path").WithDefaultValue("Assets/Output.png").ShowInInspectorOnly().Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidPath(string path)
        {
            return !string.IsNullOrEmpty(path) && (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg"));
        }

        public override void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            context.AddDataField("Unity.Collections.NativeArray<UnityEngine.Color>", $"outputData_{nodeId}", $"Output texture data for {nodeId}");
            context.AddDataField("string", $"outputPath_{nodeId}", $"Output path for {nodeId}");
            context.AddDataField("SupportedTextureFormat", $"outputFormat_{nodeId}", $"Output format for {nodeId}");
            context.AddDataField("int", $"outputWidth_{nodeId}", $"Output width for {nodeId}");
            context.AddDataField("int", $"outputHeight_{nodeId}", $"Output height for {nodeId}");
        }

        public override void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            var outputPath = GetOptionValue<string>("Output Path");
            var format = GetOptionValue<SupportedTextureFormat>("Format");

            context.AddInitializationLine($"data.outputPath_{nodeId} = \"{outputPath}\";");
            context.AddInitializationLine($"data.outputFormat_{nodeId} = SupportedTextureFormat.{format};");
            context.AddInitializationLine($"// Width and height will be set from input ports");
        }

        public override void GeneratePrepareCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("Unity.Collections");

            context.AddPrepareLine($"// Prepare ExportTexture2d {nodeId}");
            context.AddPrepareLine($"// TODO: Get width and height from input ports");
            context.AddPrepareLine($"data.outputWidth_{nodeId} = 512; // Default, should come from input");
            context.AddPrepareLine($"data.outputHeight_{nodeId} = 512; // Default, should come from input");
            context.AddPrepareLine($"var totalPixels_{nodeId} = data.outputWidth_{nodeId} * data.outputHeight_{nodeId};");
            context.AddPrepareLine($"data.outputData_{nodeId} = new Unity.Collections.NativeArray<UnityEngine.Color>(totalPixels_{nodeId}, Unity.Collections.Allocator.Persistent);");
        }

        public override void GenerateFinalizeCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("UnityEditor");
            context.AddUsing("System.IO");

            context.AddFinalizeLine($"// Finalize ExportTexture2d {nodeId}");
            context.AddFinalizeLine($"if (data.outputData_{nodeId}.IsCreated)");
            context.AddFinalizeLine($"{{");
            context.AddFinalizeLine($"    var texture_{nodeId} = new Texture2D(data.outputWidth_{nodeId}, data.outputHeight_{nodeId}, (TextureFormat)data.outputFormat_{nodeId}, false);");
            context.AddFinalizeLine($"    texture_{nodeId}.SetPixels(data.outputData_{nodeId}.ToArray());");
            context.AddFinalizeLine($"    texture_{nodeId}.Apply();");
            context.AddFinalizeLine($"");
            context.AddFinalizeLine($"    var extension_{nodeId} = System.IO.Path.GetExtension(data.outputPath_{nodeId}).ToLowerInvariant();");
            context.AddFinalizeLine($"    switch (extension_{nodeId})");
            context.AddFinalizeLine($"    {{");
            context.AddFinalizeLine($"        case \".png\":");
            context.AddFinalizeLine($"            System.IO.File.WriteAllBytes(data.outputPath_{nodeId}, texture_{nodeId}.EncodeToPNG());");
            context.AddFinalizeLine($"            break;");
            context.AddFinalizeLine($"        case \".jpg\":");
            context.AddFinalizeLine($"        case \".jpeg\":");
            context.AddFinalizeLine($"            System.IO.File.WriteAllBytes(data.outputPath_{nodeId}, texture_{nodeId}.EncodeToJPG());");
            context.AddFinalizeLine($"            break;");
            context.AddFinalizeLine($"        default:");
            context.AddFinalizeLine($"            UnityEngine.Debug.LogWarning($\"Unsupported output format '{{extension_{nodeId}}}'. Using PNG.\");");
            context.AddFinalizeLine($"            System.IO.File.WriteAllBytes(data.outputPath_{nodeId}, texture_{nodeId}.EncodeToPNG());");
            context.AddFinalizeLine($"            break;");
            context.AddFinalizeLine($"    }}");
            context.AddFinalizeLine($"");
            context.AddFinalizeLine($"    AssetDatabase.ImportAsset(data.outputPath_{nodeId});");
            context.AddFinalizeLine($"    AssetDatabase.Refresh();");
            context.AddFinalizeLine($"    UnityEngine.Object.DestroyImmediate(texture_{nodeId});");
            context.AddFinalizeLine($"    data.outputData_{nodeId}.Dispose();");
            context.AddFinalizeLine($"}}");
        }

        public override void GenerateCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("Unity.Mathematics");

            var inputVar = context.GetInputVariable(_inputPort);

            context.AddLine($"// ExportTexture2d Node {nodeId}");
            
            // Declare input variable if not connected
            if (!_inputPort.isConnected)
            {
                var inputType = GetPortTypeName(_inputPort.dataType);
                context.DeclareVariable(inputType, inputVar, "Color.black");
            }
            
            context.AddLine($"var pixelX_{nodeId} = Unity.Mathematics.math.clamp((int)(uv.x * (data.outputWidth_{nodeId} - 1)), 0, data.outputWidth_{nodeId} - 1);");
            context.AddLine($"var pixelY_{nodeId} = Unity.Mathematics.math.clamp((int)(uv.y * (data.outputHeight_{nodeId} - 1)), 0, data.outputHeight_{nodeId} - 1);");
            context.AddLine($"var pixelIndex_{nodeId} = pixelY_{nodeId} * data.outputWidth_{nodeId} + pixelX_{nodeId};");
            context.AddLine($"data.outputData_{nodeId}[pixelIndex_{nodeId}] = {inputVar};");
        }

        // Remove old methods - we don't need them anymore
        // Initialize(), Execute(), Complete() are replaced by the new system
    }
}