
using System.Runtime.CompilerServices;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

namespace Misaki.TextureMaker
{
    internal static unsafe class V128Utility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static v128 ToV128(this Color color)
        {
            return new v128(color.r, color.g, color.b, color.a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color ToColor(this v128 v)
        {
            return new Color(v.Float0, v.Float1, v.Float2, v.Float3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static v128 Lerp(v128 a, v128 b, float t)
        {
            var vT = Sse.set1_ps(t);
            var diff = Sse.sub_ps(b, a);

            return Sse.add_ps(a, Sse.mul_ps(diff, vT));
        }

        public static v128 Select(v128 screen, v128 multiply, v128 mask)
        {
            var result = default(v128);
            result.Float0 = mask.Float0 != 0 ? multiply.Float0 : screen.Float0;
            result.Float1 = mask.Float1 != 0 ? multiply.Float1 : screen.Float1;
            result.Float2 = mask.Float2 != 0 ? multiply.Float2 : screen.Float2;
            result.Float3 = mask.Float3 != 0 ? multiply.Float3 : screen.Float3;
            return result;
        }
    }
}