using Unity.Burst.Intrinsics;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

namespace Misaki.TextureMaker
{
    internal enum MathOperationType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        Min,
        Max,
        Abs,
        Floor,
        Ceil,
        Round,
        Fract,
        Sin,
        Cos,
        Sqrt,
        Clamp01
    }

    internal unsafe class MathOperation : TextureExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<Color>("A").Build();
            context.AddInputPort<Color>("B").Build();

            context.AddOutputPort<Color>("Output").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<MathOperationType>("Operation").WithDefaultValue(MathOperationType.Add).Build();
        }

        public override void Execute(Vector2 uv)
        {
            var a = GetInputPortValue<Color>("A");
            var b = GetInputPortValue<Color>("B");

            var operation = GetOptionValue<MathOperationType>("Operation");

            var vA = a.ToV128();
            var vB = b.ToV128();

            var result = operation switch
            {
                MathOperationType.Add => Sse.add_ps(vA, vB),
                MathOperationType.Subtract => Sse.sub_ps(vA, vB),
                MathOperationType.Multiply => Sse.mul_ps(vA, vB),
                MathOperationType.Divide => Sse.div_ps(vA, vB),
                MathOperationType.Min => Sse.min_ps(vA, vB),
                MathOperationType.Max => Sse.max_ps(vA, vB),
                MathOperationType.Power => PowerSIMD(vA, vB),
                MathOperationType.Abs => AbsSIMD(vA),
                MathOperationType.Floor => Sse4_1.floor_ps(vA),
                MathOperationType.Ceil => Sse4_1.ceil_ps(vA),
                MathOperationType.Round => Sse4_1.round_ps(vA, 0x00 | 0x08),
                MathOperationType.Fract => FractSIMD(vA),
                MathOperationType.Sin => SinSIMD(vA),
                MathOperationType.Cos => CosSIMD(vA),
                MathOperationType.Sqrt => Sse.sqrt_ps(vA),
                MathOperationType.Clamp01 => Clamp01SIMD(vA),
                _ => vA
            };

            SetPortValue("Output", result.ToColor());
        }

        // SIMD math functions
        private static v128 PowerSIMD(v128 a, v128 b)
        {
            return new v128(
                Mathf.Pow(a.Float0, b.Float0),
                Mathf.Pow(a.Float1, b.Float1),
                Mathf.Pow(a.Float2, b.Float2),
                a.Float3 // Preserve alpha
            );
        }

        private static v128 AbsSIMD(v128 a)
        {
            var signMask = Sse.set1_ps(-0.0f);
            return Sse.andnot_ps(signMask, a);
        }

        private static v128 FractSIMD(v128 a)
        {
            return Sse.sub_ps(a, Sse4_1.floor_ps(a));
        }

        private static v128 SinSIMD(v128 a)
        {
            return new v128(
                Mathf.Sin(a.Float0),
                Mathf.Sin(a.Float1),
                Mathf.Sin(a.Float2),
                a.Float3
            );
        }

        private static v128 CosSIMD(v128 a)
        {
            return new v128(
                Mathf.Cos(a.Float0),
                Mathf.Cos(a.Float1),
                Mathf.Cos(a.Float2),
                a.Float3
            );
        }

        private static v128 Clamp01SIMD(v128 a)
        {
            var zero = Sse.setzero_ps();
            var one = Sse.set1_ps(1.0f);
            return Sse.min_ps(Sse.max_ps(a, zero), one);
        }
    }
}