using Unity.GraphToolkit.Editor;
using Unity.Mathematics;

namespace Misaki.TextureMaker
{
    internal class Combine : CodeGenerationNode
    {
        private IPort _inputPortR;
        private IPort _inputPortG;
        private IPort _inputPortB;
        private IPort _inputPortA;

        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPortR = context.AddInputPort<float>("R").WithDefaultValue(0f).Build();
            _inputPortG = context.AddInputPort<float>("G").WithDefaultValue(0f).Build();
            _inputPortB = context.AddInputPort<float>("B").WithDefaultValue(0f).Build();
            _inputPortA = context.AddInputPort<float>("A").WithDefaultValue(1f).Build();

            _outputPort = context.AddOutputPort<float4>("Result").Build();
        }

        public override void GenerateCode(ICodeGenContext ctx)
        {
            var inputVarR = CodeGenUtility.GetInputVariableName<float>(_inputPortR, ctx, v => new ConstantExpr(v.ToString("F")));
            var inputVarG = CodeGenUtility.GetInputVariableName<float>(_inputPortG, ctx, v => new ConstantExpr(v.ToString("F")));
            var inputVarB = CodeGenUtility.GetInputVariableName<float>(_inputPortB, ctx, v => new ConstantExpr(v.ToString("F")));
            var inputVarA = CodeGenUtility.GetInputVariableName<float>(_inputPortA, ctx, v => new ConstantExpr(v.ToString("F")));

            var outputVar = CodeGenUtility.GetUniqueVariableName(_outputPort);

            ctx.AddInstruction(new Instruction
            {
                expression = new FunctionCallExpr("float4", new()
                {
                    new VariableExpr(inputVarR),
                    new VariableExpr(inputVarG),
                    new VariableExpr(inputVarB),
                    new VariableExpr(inputVarA),
                }),
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.Float4,
                    name = outputVar
                }
            });
        }
    }
}