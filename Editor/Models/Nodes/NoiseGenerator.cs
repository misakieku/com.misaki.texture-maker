using Misaki.TextureMaker.CodeGen;
using Unity.Mathematics;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    public enum NoiseType
    {
        Perlin,
        SimplexNoise,
        FBM,
        Voronoi,
        WhiteNoise
    }

    internal class NoiseGenerator : TextureExecutableNode
    {
        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _outputPort = context.AddOutputPort<float>("Output").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<NoiseType>("Noise Type").WithDefaultValue(NoiseType.Perlin).Build();
            context.AddOption<float>("Scale").WithDefaultValue(10.0f).Build();
            context.AddOption<uint>("Octaves").WithDefaultValue(4).Build();
            context.AddOption<float>("Persistence").WithDefaultValue(0.5f).Build();
            context.AddOption<float>("Lacunarity").WithDefaultValue(2.0f).Build();
            context.AddOption<uint>("Seed").WithDefaultValue(0).Build();
            context.AddOption<float>("Offset X").WithDefaultValue(0.0f).Build();
            context.AddOption<float>("Offset Y").WithDefaultValue(0.0f).Build();
        }

        public override void GenerateCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("Unity.Mathematics");
            context.AddUsing("UnityEngine");
            
            var outputVar = context.GetOutputVariable(_outputPort);
            var noiseTypeField = context.GetDataFieldName(nodeId, "noiseType");
            var scaleField = context.GetDataFieldName(nodeId, "scale");
            var octavesField = context.GetDataFieldName(nodeId, "octaves");
            var persistenceField = context.GetDataFieldName(nodeId, "persistence");
            var lacunarityField = context.GetDataFieldName(nodeId, "lacunarity");
            var seedField = context.GetDataFieldName(nodeId, "seed");
            var offsetXField = context.GetDataFieldName(nodeId, "offsetX");
            var offsetYField = context.GetDataFieldName(nodeId, "offsetY");
            
            context.AddLine($"// NoiseGenerator Node {nodeId}");
            context.AddLine($"var scaledU_{nodeId} = (uv.x + data.{offsetXField}) * data.{scaleField};");
            context.AddLine($"var scaledV_{nodeId} = (uv.y + data.{offsetYField}) * data.{scaleField};");
            
            // Generate random state
            context.AddLine($"var randomIndex_{nodeId} = Unity.Mathematics.math.asuint(uv.x + uv.y) + data.{seedField};");
            context.AddLine($"var random_{nodeId} = Unity.Mathematics.Random.CreateFromIndex(randomIndex_{nodeId});");
            
            context.AddLine($"float noiseValue_{nodeId};");
            context.AddLine($"switch (data.{noiseTypeField})");
            context.AddLine("{");
            context.AddLine("    case NoiseType.Perlin:");
            context.AddLine($"        noiseValue_{nodeId} = GeneratedCodeHelpers.GeneratePerlinNoise(scaledU_{nodeId}, scaledV_{nodeId}); break;");
            context.AddLine("    case NoiseType.SimplexNoise:");
            context.AddLine($"        noiseValue_{nodeId} = GeneratedCodeHelpers.GenerateSimplexNoise(scaledU_{nodeId}, scaledV_{nodeId}); break;");
            context.AddLine("    case NoiseType.FBM:");
            context.AddLine($"        noiseValue_{nodeId} = GeneratedCodeHelpers.GenerateFBM(scaledU_{nodeId}, scaledV_{nodeId}, data.{octavesField}, data.{persistenceField}, data.{lacunarityField}); break;");
            context.AddLine("    case NoiseType.Voronoi:");
            context.AddLine($"        noiseValue_{nodeId} = GeneratedCodeHelpers.GenerateVoronoi(scaledU_{nodeId}, scaledV_{nodeId}, ref random_{nodeId}); break;");
            context.AddLine("    case NoiseType.WhiteNoise:");
            context.AddLine($"        noiseValue_{nodeId} = GeneratedCodeHelpers.GenerateWhiteNoise(scaledU_{nodeId}, scaledV_{nodeId}, ref random_{nodeId}); break;");
            context.AddLine("    default:");
            context.AddLine($"        noiseValue_{nodeId} = GeneratedCodeHelpers.GeneratePerlinNoise(scaledU_{nodeId}, scaledV_{nodeId}); break;");
            context.AddLine("}");
            
            context.AddLine($"var {outputVar} = Unity.Mathematics.math.clamp(noiseValue_{nodeId}, 0f, 1f);");
            
            // Register the output variable
            context.RegisterOutputVariable(_outputPort, outputVar);
        }

        public override void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            context.AddDataField("NoiseType", context.GetDataFieldName(nodeId, "noiseType"), $"Noise type for {nodeId}");
            context.AddDataField("float", context.GetDataFieldName(nodeId, "scale"), $"Scale for {nodeId}");
            context.AddDataField("uint", context.GetDataFieldName(nodeId, "octaves"), $"Octaves for {nodeId}");
            context.AddDataField("float", context.GetDataFieldName(nodeId, "persistence"), $"Persistence for {nodeId}");
            context.AddDataField("float", context.GetDataFieldName(nodeId, "lacunarity"), $"Lacunarity for {nodeId}");
            context.AddDataField("uint", context.GetDataFieldName(nodeId, "seed"), $"Seed for {nodeId}");
            context.AddDataField("float", context.GetDataFieldName(nodeId, "offsetX"), $"Offset X for {nodeId}");
            context.AddDataField("float", context.GetDataFieldName(nodeId, "offsetY"), $"Offset Y for {nodeId}");
        }

        public override void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            var noiseType = GetOptionValue<NoiseType>("Noise Type");
            var scale = GetOptionValue<float>("Scale");
            var octaves = GetOptionValue<uint>("Octaves");
            var persistence = GetOptionValue<float>("Persistence");
            var lacunarity = GetOptionValue<float>("Lacunarity");
            var seed = GetOptionValue<uint>("Seed");
            var offsetX = GetOptionValue<float>("Offset X");
            var offsetY = GetOptionValue<float>("Offset Y");
            
            context.AddInitializationLine($"data.{context.GetDataFieldName(nodeId, "noiseType")} = NoiseType.{noiseType};");
            context.AddInitializationLine($"data.{context.GetDataFieldName(nodeId, "scale")} = {scale}f;");
            context.AddInitializationLine($"data.{context.GetDataFieldName(nodeId, "octaves")} = {octaves};");
            context.AddInitializationLine($"data.{context.GetDataFieldName(nodeId, "persistence")} = {persistence}f;");
            context.AddInitializationLine($"data.{context.GetDataFieldName(nodeId, "lacunarity")} = {lacunarity}f;");
            context.AddInitializationLine($"data.{context.GetDataFieldName(nodeId, "seed")} = {seed};");
            context.AddInitializationLine($"data.{context.GetDataFieldName(nodeId, "offsetX")} = {offsetX}f;");
            context.AddInitializationLine($"data.{context.GetDataFieldName(nodeId, "offsetY")} = {offsetY}f;");
        }
    }
}