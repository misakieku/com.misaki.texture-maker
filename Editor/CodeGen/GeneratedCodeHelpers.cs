using Unity.Mathematics;
using UnityEngine;

namespace Misaki.TextureMaker
{
    /// <summary>
    /// Helper methods for generated code - these will be available in the generated execution code
    /// </summary>
    public static class GeneratedCodeHelpers
    {
        // Math functions using Unity.Mathematics for better Burst compatibility
        public static float4 Abs(float4 a)
        {
            return math.abs(a);
        }

        public static float4 Clamp01(float4 a)
        {
            return math.clamp(a, 0f, 1f);
        }

        public static float4 Fract(float4 a)
        {
            return a - math.floor(a);
        }

        public static float4 Power(float4 a, float4 b)
        {
            return math.pow(a, b);
        }

        public static float4 Sin(float4 a)
        {
            return math.sin(a);
        }

        public static float4 Cos(float4 a)
        {
            return math.cos(a);
        }

        public static float4 Lerp(float4 a, float4 b, float t)
        {
            return math.lerp(a, b, t);
        }

        public static float4 Smoothstep(float4 edge0, float4 edge1, float4 x)
        {
            var t = math.clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        // Noise generation functions
        public static float GeneratePerlinNoise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }

        public static float GenerateSimplexNoise(float x, float y)
        {
            return noise.snoise(new float2(x, y));
        }

        public static float GenerateFBM(float x, float y, uint octaves, float persistence, float lacunarity)
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

        public static float GenerateVoronoi(float x, float y, ref Unity.Mathematics.Random random)
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

                    var dist = math.distance(new float2(x, y), new float2(pointX, pointY));
                    minDist = math.min(minDist, dist);
                }
            }

            return math.clamp(minDist, 0f, 1f);
        }

        public static float GenerateWhiteNoise(float x, float y, ref Unity.Mathematics.Random random)
        {
            var seed = (uint)(math.floor(x * 12.9898f + y * 78.233f) * 43758.5453f);
            random.InitState(seed);
            return random.NextFloat();
        }

        // Channel helper for shuffle operations
        public static float GetChannelValue(float4 color, int channel)
        {
            return channel switch
            {
                0 => color.x, // R
                1 => color.y, // G
                2 => color.z, // B
                3 => color.w, // A
                _ => 0f
            };
        }
    }
}