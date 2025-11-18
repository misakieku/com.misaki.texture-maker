using System;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal class Abs : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("abs", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    [Serializable]
    internal class Exp : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("exp", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    [Serializable]
    internal class Length : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("length", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    [Serializable]
    internal class Log : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("log", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    [Serializable]
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

    [Serializable]
    internal class Negate : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new BinaryExpr(ConstantExpr.Null, "-", new VariableExpr(inputs[0]));
        }
    }

    [Serializable]
    internal class Normalize : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("normalize", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    [Serializable]
    internal class Posterize : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
            new PortDeclaration { displayName = "Steps", valueType = ShaderVariableType.Float },
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

    [Serializable]
    internal class Reciprocal : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
        };

        protected override Expression BuildExpression(ReadOnlySpan<string> inputs)
        {
            return new FunctionCallExpr("rcp", new()
            {
                new VariableExpr(inputs[0])
            });
        }
    }

    [Serializable]
    internal class ReciprocalSquareRoot : MathOperatorNode
    {
        protected override PortDeclaration[] InputDeclarations => new[]
        {
            new PortDeclaration { displayName = "Value", valueType = ReturnType },
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