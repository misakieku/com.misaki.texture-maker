using System;
using System.IO;
using Unity.GraphToolkit.Editor;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal class WriteTexture2D : OutputNode
    {
        private IPort _inputPort;
        private INodeOption _outputPathOption;

        private RenderTexture _outputTexture;
        private string _textureVarName;

        internal string OutputPath => GetOptionValue<string>(_outputPathOption.Name);

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<float4>("Color").Build();
        }

        protected override void OnDefineNodeOptions(IOptionDefinitionContext context)
        {
            _outputPathOption = context.AddOption<string>("Output Path").ShowInInspectorOnly().Build();
        }

        public override void Initialize(IShaderLibrary shaderLibrary)
        {
            _outputTexture = new RenderTexture(Width, Height, 0, GraphicsFormat.R8G8B8A8_UNorm)
            {
                enableRandomWrite = true,
            };

            _outputTexture.Create();

            _textureVarName = shaderLibrary.AddVariable(ShaderVariableType.RWTexture2D, "output", (shader, index, name) =>
            {
                shader.SetTexture(index, name, _outputTexture);
            });
        }

        public override void GenerateCode(ICodeGenContext ctx)
        {
            var inputVar = ctx.GetInputVariableName<float4>(_inputPort, color =>
            {
                return new ConstantExpr($"float4({color.x}, {color.y}, {color.z}, {color.w})");
            });

            ctx.AddInstruction(new Instruction
            {
                expression = new VariableExpr(inputVar),
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.None,
                    name = $"{_textureVarName}[{ctx.GetBuiltInVariableName(BuiltInVariable.PixelCoordinate)}]"
                },
            });
        }

        public override void Cleanup(ComputeShader shader)
        {
            if (_outputTexture == null)
            {
                return;
            }

            var tex2d = new Texture2D(_outputTexture.width, _outputTexture.height, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);

            var currentRT = RenderTexture.active;
            RenderTexture.active = _outputTexture;

            tex2d.ReadPixels(new Rect(0, 0, _outputTexture.width, _outputTexture.height), 0, 0);
            tex2d.Apply();

            RenderTexture.active = currentRT;

            var bytes = tex2d.EncodeToPNG();
            // For HDR: byte[] bytes = tex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);

            File.WriteAllBytes(OutputPath, bytes);
            AssetDatabase.ImportAsset(OutputPath);
            AssetDatabase.Refresh();

            _outputTexture.Release();

            UnityEngine.Object.DestroyImmediate(_outputTexture);
            UnityEngine.Object.DestroyImmediate(tex2d);

            _outputTexture = null;
        }
    }
}
