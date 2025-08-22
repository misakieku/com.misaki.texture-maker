using Unity.Burst.Intrinsics;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

namespace Misaki.TextureMaker
{
    internal enum PatternType
    {
        Checkerboard,
        Stripes,
        Dots,
        Grid,
        Brick,
        Hexagon,
        Spiral,
        Wave
    }

    internal unsafe class PatternGenerator : TextureExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<TextureData>("Output").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PatternType>("Pattern Type").WithDefaultValue(PatternType.Checkerboard).Build();
            context.AddOption<Color>("Color A").WithDefaultValue(Color.black).Build();
            context.AddOption<Color>("Color B").WithDefaultValue(Color.white).Build();
            context.AddOption<float>("Scale X").WithDefaultValue(8.0f).Build();
            context.AddOption<float>("Scale Y").WithDefaultValue(8.0f).Build();
            context.AddOption<float>("Thickness").WithDefaultValue(0.1f).Build();
            context.AddOption<float>("Offset X").Build();
            context.AddOption<float>("Offset Y").Build();
            context.AddOption<float>("Rotation").Build();
        }

        public override void Execute(Vector2 uv)
        {
            var patternType = GetOptionValue<PatternType>("Pattern Type");
            var colorA = GetOptionValue<Color>("Color A");
            var colorB = GetOptionValue<Color>("Color B");
            var scaleX = GetOptionValue<float>("Scale X");
            var scaleY = GetOptionValue<float>("Scale Y");
            var thickness = GetOptionValue<float>("Thickness");
            var offsetX = GetOptionValue<float>("Offset X");
            var offsetY = GetOptionValue<float>("Offset Y");
            var rotation = GetOptionValue<float>("Rotation");

            var vColorA = new v128(colorA.r, colorA.g, colorA.b, colorA.a);
            var vColorB = new v128(colorB.r, colorB.g, colorB.b, colorB.a);
            var vColorDiff = Sse.sub_ps(vColorB, vColorA);

            var rotRad = rotation * Mathf.Deg2Rad;
            var cosRot = Mathf.Cos(rotRad);
            var sinRot = Mathf.Sin(rotRad);

            // Apply rotation
            var rotU = uv.x * cosRot - uv.y * sinRot;
            var rotV = uv.x * sinRot + uv.y * cosRot;

            // Scale
            var scaledU = rotU * scaleX;
            var scaledV = rotV * scaleY;

            var pattern = patternType switch
            {
                PatternType.Checkerboard => GenerateCheckerboard(scaledU, scaledV),
                PatternType.Stripes => GenerateStripes(scaledU, thickness),
                PatternType.Dots => GenerateDots(scaledU, scaledV, thickness),
                PatternType.Grid => GenerateGrid(scaledU, scaledV, thickness),
                PatternType.Brick => GenerateBrick(scaledU, scaledV, thickness),
                PatternType.Hexagon => GenerateHexagon(scaledU, scaledV, thickness),
                PatternType.Spiral => GenerateSpiral(scaledU, scaledV, scaleX),
                PatternType.Wave => GenerateWave(scaledU, scaledV, thickness),
                _ => 0.0f
            };

            // Interpolate colors
            var vPattern = Sse.set1_ps(pattern);
            var vFinal = Sse.add_ps(vColorA, Sse.mul_ps(vColorDiff, vPattern));

            SetPortValue("Output", vFinal.ToColor());
        }

        private static float GenerateCheckerboard(float u, float v)
        {
            var checkU = Mathf.FloorToInt(u) % 2;
            var checkV = Mathf.FloorToInt(v) % 2;
            return (checkU ^ checkV) == 0 ? 0.0f : 1.0f;
        }

        private static float GenerateStripes(float u, float thickness)
        {
            var fract = u - Mathf.Floor(u);
            return fract < thickness ? 1.0f : 0.0f;
        }

        private static float GenerateDots(float u, float v, float radius)
        {
            var cellU = u - Mathf.Floor(u) - 0.5f;
            var cellV = v - Mathf.Floor(v) - 0.5f;
            var dist = Mathf.Sqrt(cellU * cellU + cellV * cellV);
            return dist < radius ? 1.0f : 0.0f;
        }

        private static float GenerateGrid(float u, float v, float thickness)
        {
            var fractU = u - Mathf.Floor(u);
            var fractV = v - Mathf.Floor(v);
            return (fractU < thickness || fractV < thickness) ? 1.0f : 0.0f;
        }

        private static float GenerateBrick(float u, float v, float mortarThickness)
        {
            var row = Mathf.FloorToInt(v);
            var offsetU = (row % 2) * 0.5f;
            var brickU = u + offsetU;

            var fractU = brickU - Mathf.Floor(brickU);
            var fractV = v - Mathf.Floor(v);

            return (fractU < mortarThickness || fractV < mortarThickness) ? 0.0f : 1.0f;
        }

        private static float GenerateHexagon(float u, float v, float thickness)
        {
            // Simplified hexagon pattern - approximate with triangular grid
            var sqrt3 = 1.732050808f;
            var hexU = u;
            var hexV = v * sqrt3;

            var skewedU = hexU + hexV * 0.5f;
            var skewedV = hexV;

            var cellU = skewedU - Mathf.Floor(skewedU);
            var cellV = skewedV - Mathf.Floor(skewedV);

            return (cellU + cellV < 1.0f && cellU > thickness && cellV > thickness) ? 1.0f : 0.0f;
        }

        private static float GenerateSpiral(float u, float v, float frequency)
        {
            var centerU = u - 0.5f;
            var centerV = v - 0.5f;
            var angle = Mathf.Atan2(centerV, centerU);
            var radius = Mathf.Sqrt(centerU * centerU + centerV * centerV);

            var spiral = Mathf.Sin(angle * frequency + radius * 20.0f);
            return (spiral + 1.0f) * 0.5f;
        }

        private static float GenerateWave(float u, float v, float amplitude)
        {
            var wave = Mathf.Sin(u * Mathf.PI * 2.0f) * amplitude + Mathf.Sin(v * Mathf.PI * 2.0f) * amplitude;
            return (wave + 1.0f) * 0.5f;
        }
    }
}