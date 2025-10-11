using Unity.GraphToolkit.Editor;
using Unity.Mathematics;

namespace Misaki.TextureMaker
{
    internal class Split : CodeGenerationNode
    {
        private IPort _inputPort;
        private IPort _outputPortR;
        private IPort _outputPortG;
        private IPort _outputPortB;
        private IPort _outputPortA;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<float4>("Input").Build();

            _outputPortR = context.AddOutputPort<float>("R").Build();
            _outputPortG = context.AddOutputPort<float>("G").Build();
            _outputPortB = context.AddOutputPort<float>("B").Build();
            _outputPortA = context.AddOutputPort<float>("A").Build();
        }

        public override void GenerateCode(ICodeGenContext ctx)
        {
            var inputVar = CodeGenUtility.GetInputVariableName<float4>(_inputPort, ctx, color => new FunctionCallExpr("float4", new() 
            {
                new ConstantExpr(color.x.ToString("F")),
                new ConstantExpr(color.y.ToString("F")),
                new ConstantExpr(color.z.ToString("F")),
                new ConstantExpr(color.w.ToString("F")),
            }));

            var outputVarR = CodeGenUtility.GetUniqueVariableName(_outputPortR);
            ctx.AddInstruction(new Instruction
            {
                expression = new ConstantExpr($"{inputVar}.x"),
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.Float,
                    name = outputVarR
                }
            });

            var outputVarG = CodeGenUtility.GetUniqueVariableName(_outputPortG);
            ctx.AddInstruction(new Instruction
            {
                expression = new ConstantExpr($"{inputVar}.y"),
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.Float,
                    name = outputVarG
                }
            });

            var outputVarB = CodeGenUtility.GetUniqueVariableName(_outputPortB);
            ctx.AddInstruction(new Instruction
            {
                expression = new ConstantExpr($"{inputVar}.z"),
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.Float,
                    name = outputVarB
                }
            });

            var outputVarA = CodeGenUtility.GetUniqueVariableName(_outputPortA);
            ctx.AddInstruction(new Instruction
            {
                expression = new ConstantExpr($"{inputVar}.w"),
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.Float,
                    name = outputVarA
                }
            });
        }
    }
}