using System.Collections.Generic;
using System.Text;

namespace Misaki.TextureMaker
{
    internal enum ShaderVariableType
    {
        None = 0,
        Void,
        Float,
        Float2,
        Float3,
        Float4,
        Int,
        Int2,
        Int3,
        Int4,
        UInt,
        UInt2,
        UInt3,
        UInt4,
        Bool,
        Texture2D,
        RWTexture2D,
        SamplerState
    }

    internal struct VariableDeclaration
    {
        public ShaderVariableType type;
        public string name;

        public readonly bool IsValid => type != ShaderVariableType.None && !string.IsNullOrEmpty(name);

        public readonly string ToShaderCode()
        {
            if (!IsValid)
            {
                return string.Empty;
            }

            var hlslString = type.ToHLSLString();
            if (string.IsNullOrEmpty(hlslString))
            {
                return name;
            }

            return $"{hlslString} {name}";
        }
    }

    internal abstract record Expression
    {
        public abstract string Emit(int indentLevel);
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
    }

    internal record VariableExpr : Expression
    {
        public string name;

        public static readonly VariableExpr Zero = new ("0");
        public static readonly VariableExpr One = new ("1");
        public static readonly VariableExpr True = new ("true");
        public static readonly VariableExpr False = new ("false");
        public static readonly VariableExpr Null = new (string.Empty);

        public VariableExpr(string name)
        {
            this.name = name;
        }

        public override string Emit(int indentLevel)
        {
            return name.Indent(indentLevel);
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
    }

    internal record FunctionCallExpr : Expression
    {
        public string functionName;
        public List<Expression> inArguments;
        public List<VariableDeclaration> outArguments;

        public FunctionCallExpr(string functionName, List<Expression> inArguments, List<VariableDeclaration> outArguments = null)
        {
            this.functionName = functionName;
            this.inArguments = inArguments;
            this.outArguments = outArguments;
        }

        public override string Emit(int indentLevel)
        {
            var sb = new StringBuilder();
            if (outArguments != null)
            {
                foreach (var argDecl in outArguments)
                {
                    sb.AppendLine($"{argDecl.ToShaderCode()};".Indent(indentLevel));
                }
            }

            var inArgs = inArguments != null ? string.Join(", ", inArguments.ConvertAll(arg => arg.Emit(0))) : string.Empty;
            var outArgs = outArguments != null ? string.Join(", ", outArguments.ConvertAll(arg => arg.name)) : string.Empty;
            var allArgs = string.Join(", ", new List<string> { inArgs, outArgs }.FindAll(s => !string.IsNullOrEmpty(s)));

            sb.Append($"{functionName}({allArgs})".Indent(indentLevel));

            return sb.ToString();
        }
    }

    internal struct Instruction
    {
        public VariableDeclaration result;
        public Expression expression;
    }

    internal static class ShaderVariableTypeExtensions
    {
        public static string ToHLSLString(this ShaderVariableType type)
        {
            return type switch
            {
                ShaderVariableType.Float => "float",
                ShaderVariableType.Float2 => "float2",
                ShaderVariableType.Float3 => "float3",
                ShaderVariableType.Float4 => "float4",
                ShaderVariableType.Int => "int",
                ShaderVariableType.Int2 => "int2",
                ShaderVariableType.Int3 => "int3",
                ShaderVariableType.Int4 => "int4",
                ShaderVariableType.UInt => "uint",
                ShaderVariableType.UInt2 => "uint2",
                ShaderVariableType.UInt3 => "uint3",
                ShaderVariableType.UInt4 => "uint4",
                ShaderVariableType.Bool => "bool",
                ShaderVariableType.Texture2D => "Texture2D",
                ShaderVariableType.RWTexture2D => "RWTexture2D<float4>",
                ShaderVariableType.SamplerState => "SamplerState",
                _ => string.Empty
            };
        }
    }
}
