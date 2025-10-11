using System;

namespace Misaki.TextureMaker
{
    internal class Add : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "+", new VariableExpr(inputs[1]));
        }
    }

    internal class Subtract : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "-", new VariableExpr(inputs[1]));
        }
    }

    internal class Multiply : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "*", new VariableExpr(inputs[1]));
        }
    }

    internal class Divide : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(new VariableExpr(inputs[0]), "/", new VariableExpr(inputs[1]));
        }
    }

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

    internal class Sqrt : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
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