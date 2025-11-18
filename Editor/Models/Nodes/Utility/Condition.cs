using System;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal class Condition : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration {displayName = "Predicate", valueType = ShaderVariableType.Bool},
            new PortDeclaration {displayName = "True", valueType = ReturnType},
            new PortDeclaration {displayName = "False", valueType = ReturnType}
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

    [Serializable]
    internal class All : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("all", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    [Serializable]
    internal class Any : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("any", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    [Serializable]
    internal class Not : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(ConstantExpr.Null, "!", new VariableExpr(inputs[0]));
        }
    }

    [Serializable]
    internal class And : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "&&", new VariableExpr(inputs[1]));
        }
    }

    [Serializable]
    internal class Or : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "||", new VariableExpr(inputs[1]));
        }
    }

    [Serializable]
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