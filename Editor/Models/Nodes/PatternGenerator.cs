using UnityEngine;

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