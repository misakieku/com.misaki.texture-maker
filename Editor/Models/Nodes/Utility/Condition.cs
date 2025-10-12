using System;

namespace Misaki.TextureMaker
{
    internal class Condition : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration {displayName = "Predicate", valueType = ShaderVariableType.Bool},
            new NodePortDeclaration {displayName = "True", valueType = ValueType},
            new NodePortDeclaration {displayName = "False", valueType = ValueType}
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new SequenceExpr(
                new VariableExpr(inputs[0]),
                new OperatorExpr(" ? "),
                new VariableExpr(inputs[1]),
                new OperatorExpr(" : "),
                new VariableExpr(inputs[2]));
        }
    }

    internal class All : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("all", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    internal class Any : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("any", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    internal class Not : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(ConstantExpr.Null, "!", new VariableExpr(inputs[0]));
        }
    }

    internal class And : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "&&", new VariableExpr(inputs[1]));
        }
    }

    internal class Or : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "||", new VariableExpr(inputs[1]));
        }
    }

    internal class Compare : MathOperatorNode
    {
        public enum ComparisonType
        {
            Equal,
            NotEqual,
            Greater,
            GreaterEqual,
            Less,
            LessEqual,
        }

        protected override ShaderVariableType ReturnType => ShaderVariableType.Bool;

        protected override void OnDefineNodeOptions(IOptionDefinitionContext context)
        {
            context.AddOption<ComparisonType>("Comparison Type").Build();
        }

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            var comparisonType = GetOptionValue<ComparisonType>("Comparison Type");
            var op = comparisonType switch
            {
                ComparisonType.Equal => "==",
                ComparisonType.NotEqual => "!=",
                ComparisonType.Greater => ">",
                ComparisonType.GreaterEqual => ">=",
                ComparisonType.Less => "<",
                ComparisonType.LessEqual => "<=",
                _ => throw new ArgumentOutOfRangeException()
            };

            return new BinaryExpr(new VariableExpr(inputs[0]), op, new VariableExpr(inputs[1]));
        }
    }
}