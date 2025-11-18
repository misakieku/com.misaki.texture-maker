using System;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal class Split : MultiDimensionNode
    {
        protected override PortDeclaration[] InputDeclarations => new PortDeclaration[]
        {
            new PortDeclaration {displayName = "Input", valueType = ValueType, targetDimension = TargetDimension.Default},
        };

        protected override PortDeclaration[] OutputDeclarations => new PortDeclaration[]
        {
            new PortDeclaration {displayName = "R", valueType = ShaderVariableType.Float, targetDimension = TargetDimension.Default},
            new PortDeclaration {displayName = "G", valueType = ShaderVariableType.Float, targetDimension = TargetDimension.Float2 | TargetDimension.Float3 | TargetDimension.Float4 },
            new PortDeclaration {displayName = "B", valueType = ShaderVariableType.Float, targetDimension = TargetDimension.Float3 | TargetDimension.Float4},
            new PortDeclaration {displayName = "A", valueType = ShaderVariableType.Float, targetDimension = TargetDimension.Float4},
        };

        public override void GenerateCode(ICodeGenContext ctx)
        {
            var inputVar = ctx.GetInputVariableName(InputPorts[0], ValueType, data =>
            {
                return CodeGenUtility.ToConstantExpr(data, ValueType);
            });

            for (var i = 0; i < ComponentCount; i++)
            {
                if (i >= OutputPorts.Length)
                {
                    break;
                }

                var outputVar = ctx.GetOutputVariableName(OutputPorts[i]);
                ctx.AddInstruction(new Instruction
                {
                    expression = new ConstantExpr($"{inputVar}[{i}]"),
                    result = new VariableDeclaration
                    {
                        type = ShaderVariableType.Float,
                        name = outputVar
                    }
                });
            }
        }
    }
}