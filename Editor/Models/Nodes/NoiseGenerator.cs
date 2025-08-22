using Unity.Mathematics;
using UnityEngine;

namespace Misaki.TextureMaker
{
    public enum NoiseType
    {
        Perlin,
        SimplexNoise,
        FBM,
        Voronoi,
        WhiteNoise
    }

    internal unsafe class NoiseGenerator : TextureExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<float>("Output").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<NoiseType>("Noise Type").Build();
            context.AddOption<float>("Scale").WithDefaultValue(10.0f).Build();
            context.AddOption<uint>("Octaves").WithDefaultValue(4).Build();
            context.AddOption<float>("Persistence").WithDefaultValue(0.5f).Build();
            context.AddOption<float>("Lacunarity").WithDefaultValue(2.0f).Build();
            context.AddOption<uint>("Seed").WithDefaultValue(0).Build();
            context.AddOption<float>("Offset X").WithDefaultValue(0.0f).Build();
            context.AddOption<float>("Offset Y").WithDefaultValue(0.0f).Build();
        }

        public override unsafe void Execute(Vector2 uv)
        {
            var noiseType = GetOptionValue<NoiseType>("Noise Type");
            var scale = GetOptionValue<float>("Scale");
            var octaves = GetOptionValue<uint>("Octaves");
            var persistence = GetOptionValue<float>("Persistence");
            var lacunarity = GetOptionValue<float>("Lacunarity");
            var seed = GetOptionValue<uint>("Seed");
            var offsetX = GetOptionValue<float>("Offset X");
            var offsetY = GetOptionValue<float>("Offset Y");

            // Initialize random state
            var a = uv.x + uv.y;
            var index = *(uint*)&a;
            var random = Unity.Mathematics.Random.CreateFromIndex(index + seed);

            var noiseValue = noiseType switch
            {
                NoiseType.Perlin => GeneratePerlinNoise(uv.x, uv.y),
                NoiseType.SimplexNoise => GenerateSimplexNoise(uv.x, uv.y),
                NoiseType.FBM => GenerateFBM(uv.x, uv.y, octaves, persistence, lacunarity),
                NoiseType.Voronoi => GenerateVoronoi(uv.x, uv.y, ref random),
                NoiseType.WhiteNoise => GenerateWhiteNoise(uv.x, uv.y, ref random),
                _ => GeneratePerlinNoise(uv.x, uv.y)
            };

            SetPortValue("Output", Mathf.Clamp01(noiseValue));
        }

        private static float GeneratePerlinNoise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }

        private static float GenerateSimplexNoise(float x, float y)
        {
            return noise.snoise(new float2(x, y));
        }

        private static float GenerateFBM(float x, float y, uint octaves, float persistence, float lacunarity)
        {
            var value = 0.0f;
            var amplitude = 1.0f;
            var frequency = 1.0f;
            var maxValue = 0.0f;

            for (var i = 0; i < octaves; i++)
            {
                value += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return value / maxValue;
        }

        private static float GenerateVoronoi(float x, float y, ref Unity.Mathematics.Random random)
        {
            var cellX = Mathf.FloorToInt(x);
            var cellY = Mathf.FloorToInt(y);
            var minDist = float.MaxValue;

            // Check surrounding cells
            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    var neighborX = cellX + dx;
                    var neighborY = cellY + dy;

                    // Generate random point in cell
                    random.InitState((uint)(neighborX * 374761393 + neighborY * 668265263));
                    var pointX = neighborX + random.NextFloat();
                    var pointY = neighborY + random.NextFloat();

                    var dist = Vector2.Distance(new Vector2(x, y), new Vector2(pointX, pointY));
                    minDist = Mathf.Min(minDist, dist);
                }
            }

            return Mathf.Clamp01(minDist);
        }

        private static float GenerateWhiteNoise(float x, float y, ref Unity.Mathematics.Random random)
        {
            var seed = (uint)(Mathf.FloorToInt(x * 12.9898f + y * 78.233f) * 43758.5453f);
            random.InitState(seed);
            return random.NextFloat();
        }
    }
}