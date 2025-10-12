using System;
using System.Collections.Generic;
using System.Text;

namespace Misaki.TextureMaker
{
    internal abstract record Expression
    {
        public abstract string Emit(int indentLevel);
        public abstract Expression Inline(IReadOnlyDictionary<string, Expression> table);
    }

    internal static class ExpressionExtensions
    {
        public static InlineableExpr AsInlineable(this Expression expr)
        {
            return new InlineableExpr(expr);
        }
    }

    internal record InlineableExpr : Expression
    {
        public Expression innerExpression;

        public InlineableExpr(Expression innerExpression)
        {
            this.innerExpression = innerExpression;
        }

        public override string Emit(int indentLevel)
        {
            return innerExpression.Emit(indentLevel);
        }

        public override Expression Inline(IReadOnlyDictionary<string, Expression> table)
        {
            return innerExpression.Inline(table);
        }
    }

    internal record SequenceExpr : Expression
    {
        public Expression[] expressions;

        public SequenceExpr(params Expression[] expressions)
        {
            this.expressions = expressions;
        }

        public override string Emit(int indentLevel)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < expressions.Length; i++)
            {
                var expr = expressions[i];
                sb.Append(expr.Emit(i == 0 ? indentLevel : 0));
            }
            return sb.ToString();
        }

        public override Expression Inline(IReadOnlyDictionary<string, Expression> table)
        {
            if (expressions.Length == 0)
            {
                return VariableExpr.Null;
            }
            else if (expressions.Length == 1)
            {
                return expressions[0].Inline(table);
            }
            else
            {
                var inlinedExprs = new Expression[expressions.Length];
                for (var i = 0; i < expressions.Length; i++)
                {
                    inlinedExprs[i] = expressions[i].Inline(table);
                }

                return new SequenceExpr(inlinedExprs);
            }
        }
    }

    internal record OperatorExpr : Expression
    {
        public string op;

        public OperatorExpr(string op)
        {
            this.op = op;
        }

        public override string Emit(int indentLevel)
        {
            return op.Indent(indentLevel);
        }

        public override Expression Inline(IReadOnlyDictionary<string, Expression> table)
        {
            return this;
        }
    }

    internal record VariableExpr : Expression
    {
        public string name;

        public static readonly VariableExpr Zero = new("0");
        public static readonly VariableExpr One = new("1");
        public static readonly VariableExpr True = new("true");
        public static readonly VariableExpr False = new("false");
        public static readonly VariableExpr Null = new(string.Empty);

        public VariableExpr(string name)
        {
            this.name = name;
        }

        public override string Emit(int indentLevel)
        {
            return name.Indent(indentLevel);
        }

        public override Expression Inline(IReadOnlyDictionary<string, Expression> table)
        {
            if (table.TryGetValue(name, out var inlined))
            {
                return inlined.Inline(table);
            }

            return this;
        }
    }

    internal record ConstantExpr : Expression
    {
        public string name;

        public static readonly ConstantExpr Zero = new("0");
        public static readonly ConstantExpr One = new("1");
        public static readonly ConstantExpr True = new("true");
        public static readonly ConstantExpr False = new("false");
        public static readonly ConstantExpr Null = new(string.Empty);

        public ConstantExpr(string name)
        {
            this.name = name;
        }

        public override string Emit(int indentLevel)
        {
            return name.Indent(indentLevel);
        }

        public override Expression Inline(IReadOnlyDictionary<string, Expression> table)
        {
            return this;
        }
    }

    internal record BinaryExpr : Expression
    {
        public Expression left;
        public Expression right;
        public string op;

        public BinaryExpr(Expression left, string op, Expression right)
        {
            this.left = left;
            this.right = right;
            this.op = op;
        }

        public override string Emit(int indentLevel)
        {
            return $"({left.Emit(0)} {op} {right.Emit(0)})".Indent(indentLevel);
        }

        public override Expression Inline(IReadOnlyDictionary<string, Expression> table)
        {
            var l = left.Inline(table);
            var r = right.Inline(table);
            // We don't inline expresion if both sides are constants, because that would require evaluating the expression here, which dxc can handle better.
            return new BinaryExpr(l, op, r);
        }
    }

    internal record FunctionCallExpr : Expression
    {
        public string functionName;
        public List<Expression> inputParams;
        public List<VariableDeclaration> outputParameters;

        public FunctionCallExpr(string functionName, List<Expression> inputParams, List<VariableDeclaration> outputParameters = null)
        {
            this.functionName = functionName;
            this.inputParams = inputParams;
            this.outputParameters = outputParameters;
        }

        public override string Emit(int indentLevel)
        {
            var sb = new StringBuilder();
            if (outputParameters != null)
            {
                foreach (var argDecl in outputParameters)
                {
                    sb.AppendLine($"{argDecl.ToShaderCode()};".Indent(indentLevel));
                }
            }

            var inArgs = inputParams != null ? string.Join(", ", inputParams.ConvertAll(arg => arg.Emit(0))) : string.Empty;
            var outArgs = outputParameters != null ? string.Join(", ", outputParameters.ConvertAll(arg => arg.name)) : string.Empty;
            var allArgs = string.Join(", ", new List<string> { inArgs, outArgs }.FindAll(s => !string.IsNullOrEmpty(s)));

            sb.Append($"{functionName}({allArgs})".Indent(indentLevel));

            return sb.ToString();
        }

        public override Expression Inline(IReadOnlyDictionary<string, Expression> table)
        {
            // Inline inside arguments. Preserve OutArgs (they are variable declarations).
            var newArgs = new List<Expression>(inputParams.Count);
            foreach (var expr in inputParams)
            {
                newArgs.Add(expr.Inline(table));
            }

            var newCall = new FunctionCallExpr(functionName, newArgs);
            // copy OutArgs if present (they are VariableDeclaration objects, not expressions)
            if (outputParameters != null && outputParameters.Count > 0)
            {
                newCall.outputParameters = new List<VariableDeclaration>(outputParameters);
            }

            return newCall;
        }
    }
}