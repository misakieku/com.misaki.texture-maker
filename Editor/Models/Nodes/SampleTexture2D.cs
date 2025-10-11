using Unity.GraphToolkit.Editor;
using Unity.Mathematics;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class SampleTexture2D : CodeGenerationNode
    {
        private IPort _inputPort;
        private IPort _uvPort;
        private IPort _outputPort;

        private string _textureVarName;
        private string _samplerVarName;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<Texture2D>("Input").Build();
            _uvPort = context.AddInputPort<float2>("UV").WithDefaultValue(new Vector2(0.5f, 0.5f)).Build();

            _outputPort = context.AddOutputPort<float4>("Output").Build();
        }

        public override void Initialize(IShaderLibrary shaderLibrary)
        {
            _textureVarName = shaderLibrary.AddVariable(ShaderVariableType.Texture2D, _inputPort.name, (shader, index, name) =>
            {
                var texture = GetInputPortValue<Texture2D>(_inputPort.name);
                shader.SetTexture(index, name, texture);
            });

            _samplerVarName = shaderLibrary.AddVariableExactName(ShaderVariableType.SamplerState, $"sampler_{_textureVarName}", null);
        }

        public override void GenerateCode(ICodeGenContext ctx)
        {
            var uvVar = CodeGenUtility.GetInputVariableName<float2>(_uvPort, ctx, uv => new ConstantExpr($"float2({uv.x}, {uv.y})"));
            var outputVar = CodeGenUtility.GetUniqueVariableName(_outputPort);

            ctx.AddInstruction(new Instruction
            {
                expression = new FunctionCallExpr($"{_textureVarName}.SampleLevel", new()
                {
                    new VariableExpr(_samplerVarName),
                    new VariableExpr(uvVar),
                    new ConstantExpr("0")
                }),
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.Float4,
                    name = outputVar
                },
            });
        }
    }
}