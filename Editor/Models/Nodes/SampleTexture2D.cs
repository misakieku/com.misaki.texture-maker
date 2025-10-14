using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Windows;

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

            _outputPort = context.AddOutputPort<float4>("Result").Build();
        }

        public override void Initialize(IShaderLibrary shaderLibrary)
        {
            _textureVarName = shaderLibrary.AddVariable(ShaderVariableType.Texture2D, _inputPort.name, (shader, index, name) =>
            {
                var texture = GraphUtility.GetPortValue<Texture2D>(_inputPort);
                shader.SetTexture(index, name, texture);
            });

            _samplerVarName = shaderLibrary.AddVariableExactName(ShaderVariableType.SamplerState, $"sampler_{_textureVarName}", null);
        }

        private static Expression ToConstantExpr(object data, ShaderVariableType dataType)
        {
            var expr = data switch
            {
                float f when dataType == ShaderVariableType.Float => new ConstantExpr(f.ToString("F")),
                float2 v2 when dataType == ShaderVariableType.Float2 => new ConstantExpr($"float2({v2.x}, {v2.y})"),
                float3 v3 when dataType == ShaderVariableType.Float3 => new ConstantExpr($"float3({v3.x}, {v3.y}, {v3.z})"),
                float4 v4 when dataType == ShaderVariableType.Float4 => new ConstantExpr($"float4({v4.x}, {v4.y}, {v4.z}, {v4.w})"),
                bool b when dataType == ShaderVariableType.Bool => new ConstantExpr(b ? "true" : "false"),
                _ => throw new InvalidOperationException($"Invalid data type {data.GetType()} with PortValueType {dataType}"),
            };

            return expr;
        }

        public override void GenerateCode(ICodeGenContext ctx)
        {
            var uvVar = ctx.GetInputVariableName<float2>(_uvPort, uv => new ConstantExpr($"float2({uv.x}, {uv.y})"));
            var outputVar = ctx.GetOutputVariableName(_outputPort);

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