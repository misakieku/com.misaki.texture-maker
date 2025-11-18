using System;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal class Add : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "+", new VariableExpr(inputs[1]));
        }
    }

    [Serializable]
    internal class Subtract : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "-", new VariableExpr(inputs[1]));
        }
    }

    [Serializable]
    internal class Multiply : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "*", new VariableExpr(inputs[1]));
        }
    }

    [Serializable]
    internal class Divide : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "/", new VariableExpr(inputs[1]));
        }
    }

    [Serializable]
    internal class Power : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("pow", new()
            {
                new VariableExpr(inputs[0]),
                new VariableExpr(inputs[1])
            });
        }
    }

    [Serializable]
    internal class Sqrt : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("sqrt", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }
}