using System;

namespace Misaki.TextureMaker
{
    internal class Abs : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("abs", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    internal class Exp : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("exp", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    internal class Length : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("length", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    internal class Log : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("log", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    internal class Modulo : MathOperatorNode
    {
        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("fmod", new()
            {
                new VariableExpr(inputs[0]),
                new VariableExpr(inputs[1])
            });
        }
    }

    internal class Negate : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(ConstantExpr.Null, "-", new VariableExpr(inputs[0]));
        }
    }

    internal class Normalize : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("normalize", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    internal class Posterize : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
            new NodePortDeclaration { displayName = "Steps", valueType = ShaderVariableType.Float },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(
                new FunctionCallExpr("floor", new()
                {
                    new BinaryExpr(new VariableExpr(inputs[0]), "*", new VariableExpr(inputs[1]))
                }),
                "/",
                new VariableExpr(inputs[1])
            );
        }
    }

    internal class Reciprocal : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("rcp", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    internal class ReciprocalSquareRoot : MathOperatorNode
    {
        protected override NodePortDeclaration[] InputDeclarations => new[]
        {
            new NodePortDeclaration { displayName = "Value", valueType = ValueType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("rsqrt", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }
}