using System;
using Unity.GraphToolkit.Editor;
using Unity.Mathematics;

namespace Misaki.TextureMaker
{
    internal struct NodePortDeclaration
    {
        public string displayName;
        public ShaderVariableType valueType;
    }

    internal enum PortValueType
    {
        Float,
        Float2,
        Float3,
        Float4
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

        protected virtual ShaderVariableType ReturnType => ValueType;
        protected ShaderVariableType ValueType => ToShaderVariableType(GetOptionValue<PortValueType>(VALUE_TYPE_OPTION_NAME));

        protected sealed override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPorts = new IPort[InputDeclarations.Length];
            for (var i = 0; i < InputDeclarations.Length; i++)
            {
                var decl = InputDeclarations[i];
                _inputPorts[i] = context.AddInputPort(decl.displayName).WithDataType(decl.valueType.ToType()).Build();
            }

            _outputPort = context.AddOutputPort("Result").WithDataType(ReturnType.ToType()).Build();
        }

        protected sealed override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PortValueType>(VALUE_TYPE_OPTION_NAME).WithDefaultValue(PortValueType.Float4).ShowInInspectorOnly().Build();
            OnDefineNodeOptions(context);
        }

        protected virtual void OnDefineNodeOptions(IOptionDefinitionContext context)
        {
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
            var inputVars = new string[_inputPorts.Length];
            for (var i = 0; i < _inputPorts.Length; i++)
            {
                var sharpType = InputDeclarations[i].valueType;
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
                    type = ReturnType,
                    name = CodeGenUtility.GetUniqueVariableName(_outputPort)
                },
            });
        }

        protected abstract Expression BuildExpression(ReadOnlySpan<string> inputs);
    }
}