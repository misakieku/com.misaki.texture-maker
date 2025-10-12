using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;

namespace Misaki.TextureMaker
{
    internal class InstructionCompiler
    {
        private const string _CS_KERNEL = "CSMain";

        public readonly static int3 threadGroupSize = new(8, 8, 1);

        private readonly IShaderLibrary _shaderLibrary;
        private readonly List<ICodeGenContext> _codeGenContexts;

        public InstructionCompiler(IShaderLibrary shaderLibrary)
        {
            _shaderLibrary = shaderLibrary;
            _codeGenContexts = new();
        }

        private void GenerateDefaultVariables(StringBuilder sb)
        {
            sb.AppendLine(@"
float4 textureSize; // width, height, 1/width, 1/height");
        }

        private void GenerateBuiltInVariables(StringBuilder sb)
        {
            var enumValues = Enum.GetValues(typeof(BuiltInVariable));
            foreach (var enumValue in enumValues)
            {
                var ev = (BuiltInVariable)enumValue;

                switch (ev)
                {
                    case BuiltInVariable.PixelCoordinate:
                        sb.Append(@"
    uint2 pixelCoordinate = dispatchThreadID.xy;");
                        break;
                    case BuiltInVariable.UV:
                        sb.Append(@"
    float2 uv = (pixelCoordinate + 0.5f) * textureSize.zw;");
                        break;
                    default:
                        // Handled by HLSL semantics
                        break;
                }
            }

            sb.AppendLine();
        }

        private IEnumerable<Instruction> InlineInstructions(IReadOnlyList<Instruction> instructions)
        {
            // 1) Collect inline-candidates: constant results that can be substituted.
            var inlineCandidates = new Dictionary<string, Expression>(StringComparer.Ordinal);
            foreach (var instr in instructions)
            {
                if (instr.result.IsValid && instr.expression is ConstantExpr)
                {
                    inlineCandidates[instr.result.name] = instr.expression;
                }
            }

            // 2) Build new instruction list, skipping the definitions of inline candidates
            //    and replacing references inside other expressions.
            //var outList = new List<Instruction>(instructions.Count);
            foreach (var instr in instructions)
            {
                // If this instruction defines a value that we decided to inline, skip it entirely.
                if (instr.result.IsValid && inlineCandidates.ContainsKey(instr.result.name))
                {
                    continue;
                }

                // Otherwise, produce a copied instruction with an inlined expression.
                var newInstr = instr;
                newInstr.expression = instr.expression.Inline(inlineCandidates);

                if (instr.expression is InlineableExpr inlineable)
                {
                    inlineCandidates[instr.result.name] = inlineable.innerExpression;
                    // We can skip this instruction as well, since it is just a wrapper for inlining.
                    continue;
                }
                else
                {
                    yield return newInstr;
                }
            }
        }

        private static void GenerateFunctionCode(StringBuilder sb, FunctionDeclaration function)
        {
            var canInline = function.flags.HasFlag(FunctionFlag.Inlineable);
            var inlineStr = canInline ? "inline " : string.Empty;

            var parameters = string.Empty;
            var paramArray = default(string[]);
            var realCode = function.code;

            if (function.signature != null && function.signature.Count > 0)
            {
                paramArray = new string[function.signature.Count];
                for (int i = 0; i < function.signature.Count; i++)
                {
                    var param = function.signature[i];
                    var modifier = string.Empty;
                    if (param.modifier.HasFlag(ParameterModifier.In)
                        && param.modifier.HasFlag(ParameterModifier.Out))
                    {
                        modifier = "inout ";
                    }
                    else if (param.modifier.HasFlag(ParameterModifier.In))
                    {
                        modifier = "in ";
                    }
                    else if (param.modifier.HasFlag(ParameterModifier.Out))
                    {
                        modifier = "out ";
                    }

                    var typeStr = param.type.ToHLSLString();
                    if (string.IsNullOrEmpty(typeStr))
                    {
                        typeStr = "void"; // Fallback to void, though this should not happen
                    }

                    paramArray[i] = $"{modifier}{typeStr} {param.name}";
                }

                parameters = string.Join(", ", paramArray);
                realCode = string.Format(function.code, function.signature.Select(p => p.name).ToArray());
            }

            sb.Append($@"
{inlineStr}{function.returnType.ToHLSLString()} {function.name} ({parameters})
{{
    {realCode}
}}");
            sb.AppendLine();
        }

        public void AddContext(ICodeGenContext ctx, int kernelIndex)
        {
            var newSize = Math.Max(_codeGenContexts.Capacity, kernelIndex + 1);
            if (newSize > _codeGenContexts.Count)
            {
                _codeGenContexts.Capacity = newSize;
                for (var i = _codeGenContexts.Count; i < newSize; i++)
                {
                    _codeGenContexts.Add(null);
                }
            }

            _codeGenContexts[kernelIndex] = ctx;
        }

        public ICodeGenContext GetContext(int kernelIndex)
        {
            return _codeGenContexts[kernelIndex];
        }

        public string Compile()
        {
            var sb = new StringBuilder();

            sb.AppendLine("// Auto-generated shader code");

            // CS Kernel
            for (var kernelIndex = 0; kernelIndex < _codeGenContexts.Count; kernelIndex++)
            {
                var context = _codeGenContexts[kernelIndex];
                sb.AppendLine("#pragma kernel " + _CS_KERNEL + kernelIndex);
            }

            // Includes
            if (_shaderLibrary.Includes.Count > 0)
            {
                sb.AppendLine();
                foreach (var include in _shaderLibrary.Includes)
                {
                    sb.AppendLine($"#include \"{include}\"");
                }
            }

            // Definitions
            if (_shaderLibrary.Definitions.Count > 0)
            {
                sb.AppendLine();
                foreach (var definition in _shaderLibrary.Definitions)
                {
                    sb.AppendLine($"#define {definition}");
                }
            }

            // Shader Variables
            if (_shaderLibrary.Variables.Count > 0)
            {
                sb.AppendLine();
                GenerateDefaultVariables(sb);
                sb.AppendLine();
                foreach (var variable in _shaderLibrary.Variables)
                {
                    sb.AppendLine($"{variable.declaration.ToShaderCode()};");
                }
            }

            // Functions
            if (_shaderLibrary.Functions.Count > 0)
            {
                sb.AppendLine();
                foreach (var function in _shaderLibrary.Functions)
                {
                    GenerateFunctionCode(sb, function);
                }
            }

            // Main
            for (var kernelIndex = 0; kernelIndex < _codeGenContexts.Count; kernelIndex++)
            {
                var context = _codeGenContexts[kernelIndex];
                sb.Append(@$"
[numthreads({threadGroupSize.x},{threadGroupSize.y},{threadGroupSize.z})]
void {_CS_KERNEL + kernelIndex} (uint3 dispatchThreadID : SV_DispatchThreadID, uint3 groupId : SV_GroupID, uint groupIndex : SV_GroupIndex, uint3 groupThreadId : SV_GroupThreadID)
{{");
                GenerateBuiltInVariables(sb);
                sb.AppendLine();

#if DISABLE_INSTR_INLINE
                foreach (var instr in context.InstructionSet)
#else
                foreach (var instr in InlineInstructions(context.InstructionSet))
#endif
                {
                    if (instr.result.IsValid)
                    {
                        sb.AppendLine(@$"{instr.result.ToShaderCode()} = {instr.expression.Emit(0)};".Indent(1));
                    }
                    else
                    {
                        sb.AppendLine(@$"{instr.expression.Emit(1)};");
                    }
                }

                sb.AppendLine(@"
}");
            }

            return sb.ToString();
        }
    }
}