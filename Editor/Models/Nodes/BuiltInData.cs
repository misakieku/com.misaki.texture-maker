using Unity.GraphToolkit.Editor;
using Unity.Mathematics;

namespace Misaki.TextureMaker
{
    internal class BuiltInData : CodeGenerationNode
    {
        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var option = GetOptionValue<BuiltInVariable>("Variable");
            var dataType = option switch
            {
                BuiltInVariable.DispatchThreadID => typeof(uint3),
                BuiltInVariable.GroupID => typeof(uint3),
                BuiltInVariable.GroupThreadID => typeof(uint3),
                BuiltInVariable.GroupIndex => typeof(uint),
                BuiltInVariable.PixelCoordinate => typeof(uint2),
                BuiltInVariable.UV => typeof(float2),
                _ => typeof(float)
            };

            _outputPort = context.AddOutputPort("Output").WithDataType(dataType).WithDisplayName(option.ToString()).Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<BuiltInVariable>("Variable").Build();
        }

        public override void GenerateCode(ICodeGenContext ctx)
        {
            var variable = GetOptionValue<BuiltInVariable>("Variable");

            var variableType = variable switch
            {
                BuiltInVariable.DispatchThreadID => ShaderVariableType.UInt3,
                BuiltInVariable.GroupID => ShaderVariableType.UInt3,
                BuiltInVariable.GroupIndex => ShaderVariableType.UInt,
                BuiltInVariable.GroupThreadID => ShaderVariableType.UInt3,
                BuiltInVariable.PixelCoordinate => ShaderVariableType.UInt2,
                BuiltInVariable.UV => ShaderVariableType.Float2,
                _ => ShaderVariableType.Float
            };

            ctx.AddInstruction(new Instruction
            {
                result = new VariableDeclaration
                {
                    type = variableType,
                    name = CodeGenUtility.GetUniqueVariableName(_outputPort)
                },
                expression = new ConstantExpr(ctx.GetBuiltInVariableName(variable))
            });
        }
    }
}