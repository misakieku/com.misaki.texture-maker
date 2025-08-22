using Unity.Burst.Intrinsics;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

namespace Misaki.TextureMaker
{
    internal enum BlendMode
    {
        Normal,
        Multiply,
        Screen,
        Overlay,
        SoftLight,
        HardLight,
        ColorDodge,
        ColorBurn,
        Darken,
        Lighten,
        Difference,
        Exclusion,
        Add,
        Subtract
    }

    internal unsafe class Blend : TextureExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<Color>("Base").Build();
            context.AddInputPort<Color>("Overlay").Build();
            context.AddInputPort<Color>("Mask").Build();

            context.AddOutputPort<Color>("Output").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<BlendMode>("Blend Mode").WithDefaultValue(BlendMode.Normal).Build();
        }

        public override void Execute(Vector2 uv)
        {
            var baseColor = GetInputPortValue<Color>("Base");
            var overlayColor = GetInputPortValue<Color>("Overlay");
            var maskColor = GetInputPortValue<Color>("Mask");

            var blendMode = GetOptionValue<BlendMode>("Blend Mode");

            var vBase = baseColor.ToV128();
            var vOverlay = overlayColor.ToV128();
            var vMask = maskColor.ToV128();

            var blended = BlendColor(vBase, vOverlay, blendMode);
            var output = new v128(
                blended.Float0 * vMask.Float0,
                blended.Float1 * vMask.Float1,
                blended.Float2 * vMask.Float2,
                blended.Float3 * vMask.Float3
            );

            SetPortValue("Output", *(Color*)&output);
        }

        private static v128 BlendColor(v128 baseColor, v128 overlayColor, BlendMode mode)
        {
            return mode switch
            {
                BlendMode.Normal => overlayColor,
                BlendMode.Multiply => Sse.mul_ps(baseColor, overlayColor),
                BlendMode.Screen => ScreenBlend(baseColor, overlayColor),
                BlendMode.Add => Sse.add_ps(baseColor, overlayColor),
                BlendMode.Subtract => Sse.sub_ps(baseColor, overlayColor),
                BlendMode.Overlay => OverlayBlend(baseColor, overlayColor),
                BlendMode.SoftLight => SoftLightBlend(baseColor, overlayColor),
                BlendMode.HardLight => throw new System.NotImplementedException(),
                BlendMode.ColorDodge => throw new System.NotImplementedException(),
                BlendMode.ColorBurn => throw new System.NotImplementedException(),
                BlendMode.Darken => throw new System.NotImplementedException(),
                BlendMode.Lighten => throw new System.NotImplementedException(),
                BlendMode.Difference => throw new System.NotImplementedException(),
                BlendMode.Exclusion => throw new System.NotImplementedException(),
                _ => overlayColor
            };
        }

        private static v128 ScreenBlend(v128 baseColor, v128 overlayColor)
        {
            var ones = Sse.set1_ps(1.0f);
            var invBase = Sse.sub_ps(ones, baseColor);
            var invOverlay = Sse.sub_ps(ones, overlayColor);
            var result = Sse.mul_ps(invBase, invOverlay);
            return Sse.sub_ps(ones, result);
        }

        private static v128 OverlayBlend(v128 baseColor, v128 overlayColor)
        {
            var half = Sse.set1_ps(0.5f);
            var ones = Sse.set1_ps(1.0f);
            var two = Sse.set1_ps(2.0f);

            // If base < 0.5: 2 * base * overlay
            var multiply = Sse.mul_ps(two, Sse.mul_ps(baseColor, overlayColor));

            // Else: 1 - 2 * (1 - base) * (1 - overlay)
            var invBase = Sse.sub_ps(ones, baseColor);
            var invOverlay = Sse.sub_ps(ones, overlayColor);
            var screen = Sse.sub_ps(ones, Sse.mul_ps(two, Sse.mul_ps(invBase, invOverlay)));

            // Blend based on base < 0.5
            var mask = Sse.cmplt_ps(baseColor, half);
            return V128Utility.Select(screen, multiply, mask);
        }

        private static v128 SoftLightBlend(v128 baseColor, v128 overlayColor)
        {
            var half = Sse.set1_ps(0.5f);
            var ones = Sse.set1_ps(1.0f);
            var two = Sse.set1_ps(2.0f);

            var a1 = Sse.mul_ps(Sse.mul_ps(two, baseColor), overlayColor);
            var b1 = Sse.mul_ps(baseColor, baseColor);
            var c1 = Sse.sub_ps(ones, Sse.mul_ps(two, overlayColor));
            var result1 = Sse.add_ps(a1, Sse.mul_ps(b1, c1));

            var a2 = Sse.sqrt_ps(baseColor);
            var b2 = Sse.sub_ps(Sse.mul_ps(two, overlayColor), ones);
            var c2 = Sse.mul_ps(two, Sse.mul_ps(Sse.sub_ps(ones, overlayColor), baseColor));
            var result2 = Sse.add_ps(Sse.mul_ps(a2, b2), c2);

            var zeroOrOne = Sse.cmplt_ps(baseColor, half);
            zeroOrOne = Sse.and_ps(zeroOrOne, ones); // Convert to 0 or 1

            return Sse.add_ps(Sse.mul_ps(result2, zeroOrOne), Sse.mul_ps(result1, Sse.sub_ps(ones, zeroOrOne)));
        }

        private static v128 LerpPixels(v128 a, v128 b, float t)
        {
            var vT = Sse.set1_ps(t);
            var oneMinusT = Sse.set1_ps(1.0f - t);
            return Sse.add_ps(Sse.mul_ps(a, oneMinusT), Sse.mul_ps(b, vT));
        }
    }
}