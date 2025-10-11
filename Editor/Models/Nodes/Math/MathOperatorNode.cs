using System;
using Unity.GraphToolkit.Editor;
using Unity.Mathematics;

namespace Misaki.TextureMaker
{
    internal struct NodePortDeclaration
    {
        public string displayName;
        public PortValueType valueType;
    }

    internal abstract class MathOperatorNode : CodeGenerationNode
    {
        protected const string VALUE_TYPE_OPTION_NAME = "Value Type";

        private IPort _outputPort;
        private IPort[] _inputPorts;

        protected virtual NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "A", valueType = ValueType },
            new NodePortDeclaration { displayName = "B", valueType = ValueType },
        };

        protected PortValueType ValueType => GetOptionValue<PortValueType>(VALUE_TYPE_OPTION_NAME);

        private static Type GetDataType(PortValueType valueType) => valueType switch
        {
            PortValueType.Float => typeof(float),
            PortValueType.Float2 => typeof(float2),
            PortValueType.Float3 => typeof(float3),
            PortValueType.Float4 => typeof(float4),
            _ => typeof(float4)
        };

        protected sealed override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPorts = new IPort[InputDeclarations.Length];
            for (var i = 0; i < InputDeclarations.Length; i++)
            {
                var decl = InputDeclarations[i];
                _inputPorts[i] = context.AddInputPort(decl.displayName).WithDataType(GetDataType(decl.valueType)).Build();
            }

            _outputPort = context.AddOutputPort("Result").WithDataType(GetDataType(ValueType)).Build();
        }

        protected sealed override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PortValueType>(VALUE_TYPE_OPTION_NAME).WithDefaultValue(PortValueType.Float4).ShowInInspectorOnly().Build();
        }

        private static Expression ToConstantExpr(object data, PortValueType dataType)
        {
            return data switch
            {
                float f when dataType == PortValueType.Float => new ConstantExpr(f.ToString("F")),
                float2 v2 when dataType == PortValueType.Float2 => new ConstantExpr($"float2({v2.x}, {v2.y})"),
                float3 v3 when dataType == PortValueType.Float3 => new ConstantExpr($"float3({v3.x}, {v3.y}, {v3.z})"),
                float4 v4 when dataType == PortValueType.Float4 => new ConstantExpr($"float4({v4.x}, {v4.y}, {v4.z}, {v4.w})"),
                _ => throw new InvalidOperationException($"Invalid data type {data.GetType()} with PortValueType {dataType}"),
            };
        }

        private static ShaderVariableType ToShaderVariableType(PortValueType dataType)
        {
            return dataType switch
            {
                PortValueType.Float => ShaderVariableType.Float,
                PortValueType.Float2 => ShaderVariableType.Float2,
                PortValueType.Float3 => ShaderVariableType.Float3,
                PortValueType.Float4 => ShaderVariableType.Float4,
                _ => ShaderVariableType.Float4
            };
        }

        public sealed override void GenerateCode(ICodeGenContext ctx)
        {
            var dataType = GetOptionValue<PortValueType>(VALUE_TYPE_OPTION_NAME);

            var inputVars = new string[_inputPorts.Length];
            for (var i = 0; i < _inputPorts.Length; i++)
            {
                var sharpType = ToShaderVariableType(InputDeclarations[i].valueType);
                inputVars[i] = CodeGenUtility.GetInputVariableName(_inputPorts[i], sharpType, ctx, data =>
                {
                    return ToConstantExpr(data, InputDeclarations[i].valueType);
                });
            }

            ctx.AddInstruction(new Instruction
            {
                expression = BuildExpression(inputVars),
                result = new VariableDeclaration
                {
                    type = ToShaderVariableType(dataType),
                    name = CodeGenUtility.GetUniqueVariableName(_outputPort)
                },
            });
        }

        protected abstract Expression BuildExpression(ReadOnlySpan<string> inputs);
    }
}