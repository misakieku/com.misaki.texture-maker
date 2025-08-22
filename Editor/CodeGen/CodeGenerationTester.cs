using UnityEngine;
using UnityEditor;
using Misaki.TextureMaker.CodeGen;

namespace Misaki.TextureMaker.Editor
{
    /// <summary>
    /// Editor utility for testing and debugging the code generation system
    /// </summary>
    public class CodeGenerationTester : EditorWindow
    {
        [MenuItem("Tools/Texture Maker/Code Generation Tester")]
        public static void ShowWindow()
        {
            GetWindow<CodeGenerationTester>("Code Gen Tester");
        }

        private void OnGUI()
        {
            GUILayout.Label("Texture Maker Code Generation Tester", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Generate Sample Code"))
            {
                GenerateSampleCode();
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox(
                "This will generate a sample texture execution code with a few basic nodes. " +
                "The generated .cs file will be saved to Assets/Generated/ for inspection.",
                MessageType.Info);
        }

        private void GenerateSampleCode()
        {
            var context = new CodeGenContext();
            
            // Add common usings
            context.AddUsing("UnityEngine");
            context.AddUsing("Unity.Burst.Intrinsics");
            context.AddUsing("static Unity.Burst.Intrinsics.X86");
            context.AddUsing("Unity.Mathematics");
            context.AddUsing("Misaki.TextureMaker");
            
            // Simulate a simple pipeline: NoiseGenerator -> Brightness -> Shuffle -> Output
            GenerateSamplePipeline(context);
            
            var generatedCode = context.GenerateFullCode("SampleTextureExecution");
            
            // Write to file
            var outputPath = "Assets/Generated";
            if (!System.IO.Directory.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }
            
            var fileName = $"SampleTextureExecution_{System.DateTime.Now:yyyyMMdd_HHmmss}.cs";
            var filePath = System.IO.Path.Combine(outputPath, fileName);
            
            System.IO.File.WriteAllText(filePath, generatedCode);
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Sample code generated and saved to: {filePath}");
            EditorUtility.DisplayDialog("Code Generated", $"Sample code saved to:\n{filePath}", "OK");
        }

        private void GenerateSamplePipeline(CodeGenContext context)
        {
            // Simulate data fields for sample nodes
            context.AddDataField("NoiseType", "node_0_noiseType", "Noise type for node_0");
            context.AddDataField("float", "node_0_scale", "Scale for node_0");
            context.AddDataField("uint", "node_0_seed", "Seed for node_0");
            context.AddDataField("int", "node_2_shuffleMask", "Pre-computed shuffle mask for node_2");
            
            // Simulate initialization
            context.AddInitializationLine("data.node_0_noiseType = NoiseType.Perlin;");
            context.AddInitializationLine("data.node_0_scale = 10.0f;");
            context.AddInitializationLine("data.node_0_seed = 12345;");
            context.AddInitializationLine("data.node_2_shuffleMask = 0x1B; // R:B, G:G, B:R, A:A");
            
            // Generate execution code
            context.AddLine("// Sample Texture Generation Pipeline");
            context.AddLine();
            
            // Node 0: Noise Generator
            context.AddLine("// NoiseGenerator Node node_0");
            context.AddLine("var scaledU_node_0 = uv.x * data.node_0_scale;");
            context.AddLine("var scaledV_node_0 = uv.y * data.node_0_scale;");
            context.AddLine("var randomIndex_node_0 = Unity.Mathematics.math.asuint(uv.x + uv.y) + data.node_0_seed;");
            context.AddLine("var random_node_0 = Unity.Mathematics.Random.CreateFromIndex(randomIndex_node_0);");
            context.AddLine("float noiseValue_node_0;");
            context.AddLine("switch (data.node_0_noiseType)");
            context.AddLine("{");
            context.AddLine("    case NoiseType.Perlin:");
            context.AddLine("        noiseValue_node_0 = GeneratedCodeHelpers.GeneratePerlinNoise(scaledU_node_0, scaledV_node_0); break;");
            context.AddLine("    default:");
            context.AddLine("        noiseValue_node_0 = GeneratedCodeHelpers.GeneratePerlinNoise(scaledU_node_0, scaledV_node_0); break;");
            context.AddLine("}");
            context.AddLine("var output_node_0_Output = Unity.Mathematics.math.clamp(noiseValue_node_0, 0f, 1f);");
            context.AddLine();
            
            // Node 1: Brightness (convert float to color and apply brightness)
            context.AddLine("// Brightness Node node_1");
            context.AddLine("var input_node_1_Input = new Color(output_node_0_Output, output_node_0_Output, output_node_0_Output, 1f);");
            context.AddLine("var input_node_1_Brightness = 1.5f; // Example brightness value");
            context.AddLine("var vInput_node_1 = input_node_1_Input.ToV128();");
            context.AddLine("var vBrightness_node_1 = Sse.set1_ps(input_node_1_Brightness);");
            context.AddLine("var output_node_1_Output = Sse.mul_ps(vInput_node_1, vBrightness_node_1).ToColor();");
            context.AddLine();
            
            // Node 2: Shuffle
            context.AddLine("// Shuffle Node node_2");
            context.AddLine("var input_node_2_Input = output_node_1_Output;");
            context.AddLine("var vInput_node_2 = input_node_2_Input.ToV128();");
            context.AddLine("var output_node_2_Output = Sse.shuffle_ps(vInput_node_2, vInput_node_2, data.node_2_shuffleMask).ToColor();");
            context.AddLine();
            
            // Return final result
            context.AddLine("return output_node_2_Output;");
        }
    }
}