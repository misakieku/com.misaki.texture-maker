using System;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal abstract class MathOperatorNode : MultiDimensionNode
    {
        protected virtual ShaderVariableType ReturnType => ValueType;

        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "A", valueType = ValueType, targetDimension = TargetDimension.Default },
            new PortDeclaration { displayName = "B", valueType = ValueType, targetDimension = TargetDimension.Default },
        };

        protected sealed override PortDeclaration[] OutputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Result", valueType = ReturnType },
        };

        public sealed override void GenerateCode(ICodeGenContext ctx)
        {
            var inputVars = new string[InputPorts.Length];
            for (var i = 0; i < InputPorts.Length; i++)
            {
                var sharpType = InputDeclarations[i].valueType;
                inputVars[i] = ctx.GetInputVariableName(InputPorts[i], sharpType, data =>
                {
                    return CodeGenUtility.ToConstantExpr(data, InputDeclarations[i].valueType);
                });
            }

            ctx.AddInstruction(new Instruction
            {
                expression = BuildExpression(inputVars),
                result = new VariableDeclaration
                {
                    type = ValueType,
                    name = ctx.GetOutputVariableName(OutputPorts[0])
                },
            });
        }

        protected abstract Expression BuildExpression(ReadOnlySpan<string> inputs);
    }
}