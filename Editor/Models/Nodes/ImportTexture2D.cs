using UnityEngine;
using UnityEditor;
using Misaki.TextureMaker.CodeGen;
using Unity.GraphToolkit.Editor;

namespace Misaki.TextureMaker
{
    internal class ImportTexture2D : TextureExecutableNode
    {
        private IPort _inputPort;
        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<Texture2D>("Input").Build();
            _outputPort = context.AddOutputPort<Color>("Output").Build();
        }

        public override void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            context.AddDataField("Unity.Collections.NativeArray<UnityEngine.Color>", $"textureData_{nodeId}", $"Texture data for {nodeId}");
            context.AddDataField("int", $"textureWidth_{nodeId}", $"Texture width for {nodeId}");
            context.AddDataField("int", $"textureHeight_{nodeId}", $"Texture height for {nodeId}");
            context.AddDataField("string", $"texturePath_{nodeId}", $"Texture path for {nodeId}");
        }

        public override void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            // Get the texture path from the input port (will be set during preparation)
            context.AddInitializationLine($"data.texturePath_{nodeId} = \"\"; // Will be set in Prepare");
            context.AddInitializationLine($"data.textureWidth_{nodeId} = 0;");
            context.AddInitializationLine($"data.textureHeight_{nodeId} = 0;");
        }

        public override void GeneratePrepareCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("UnityEditor");
            context.AddUsing("Unity.Collections");

            context.AddPrepareLine($"// Prepare ImportTexture2D {nodeId}");
            context.AddPrepareLine($"// TODO: Get texture from input port connection");
            context.AddPrepareLine($"var texture_{nodeId} = AssetDatabase.LoadAssetAtPath<Texture2D>(data.texturePath_{nodeId});");
            context.AddPrepareLine($"if (texture_{nodeId} != null && texture_{nodeId}.isReadable)");
            context.AddPrepareLine($"{{");
            context.AddPrepareLine($"    data.textureWidth_{nodeId} = texture_{nodeId}.width;");
            context.AddPrepareLine($"    data.textureHeight_{nodeId} = texture_{nodeId}.height;");
            context.AddPrepareLine($"    var pixels = texture_{nodeId}.GetPixels();");
            context.AddPrepareLine($"    data.textureData_{nodeId} = new Unity.Collections.NativeArray<UnityEngine.Color>(pixels, Unity.Collections.Allocator.Persistent);");
            context.AddPrepareLine($"}}");
            context.AddPrepareLine($"else");
            context.AddPrepareLine($"{{");
            context.AddPrepareLine($"    UnityEngine.Debug.LogWarning($\"Texture at {{data.texturePath_{nodeId}}} is null or not readable\");");
            context.AddPrepareLine($"    data.textureData_{nodeId} = new Unity.Collections.NativeArray<UnityEngine.Color>(1, Unity.Collections.Allocator.Persistent);");
            context.AddPrepareLine($"    data.textureData_{nodeId}[0] = UnityEngine.Color.magenta; // Error color");
            context.AddPrepareLine($"    data.textureWidth_{nodeId} = 1;");
            context.AddPrepareLine($"    data.textureHeight_{nodeId} = 1;");
            context.AddPrepareLine($"}}");
        }

        public override void GenerateFinalizeCode(ICodeGenContext context, string nodeId)
        {
            context.AddFinalizeLine($"// Finalize ImportTexture2D {nodeId}");
            context.AddFinalizeLine($"if (data.textureData_{nodeId}.IsCreated)");
            context.AddFinalizeLine($"    data.textureData_{nodeId}.Dispose();");
        }

        public override void GenerateCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("Unity.Mathematics");
            context.AddUsing("UnityEngine");

            var outputVar = context.GetOutputVariable(_outputPort);

            context.AddLine($"// ImportTexture2D Node {nodeId}");
            context.AddLine($"var texCoordX_{nodeId} = Unity.Mathematics.math.clamp((int)(uv.x * (data.textureWidth_{nodeId} - 1)), 0, data.textureWidth_{nodeId} - 1);");
            context.AddLine($"var texCoordY_{nodeId} = Unity.Mathematics.math.clamp((int)(uv.y * (data.textureHeight_{nodeId} - 1)), 0, data.textureHeight_{nodeId} - 1);");
            context.AddLine($"var texIndex_{nodeId} = texCoordY_{nodeId} * data.textureWidth_{nodeId} + texCoordX_{nodeId};");
            context.AddLine($"var {outputVar} = data.textureData_{nodeId}[texIndex_{nodeId}];");

            // Register the output variable
            context.RegisterOutputVariable(_outputPort, outputVar);
        }
    }
}