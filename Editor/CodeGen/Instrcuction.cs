using System;
using Unity.Mathematics;
using UnityEngine;

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

        public static Type ToType(this ShaderVariableType type)
        {
            return type switch
            {
                ShaderVariableType.Float => typeof(float),
                ShaderVariableType.Float2 => typeof(float2),
                ShaderVariableType.Float3 => typeof(float3),
                ShaderVariableType.Float4 => typeof(float4),
                ShaderVariableType.Int => typeof(int),
                ShaderVariableType.Int2 => typeof(int2),
                ShaderVariableType.Int3 => typeof(int3),
                ShaderVariableType.Int4 => typeof(int4),
                ShaderVariableType.UInt => typeof(uint),
                ShaderVariableType.UInt2 => typeof(uint2),
                ShaderVariableType.UInt3 => typeof(uint3),
                ShaderVariableType.UInt4 => typeof(uint4),
                ShaderVariableType.Bool => typeof(bool),
                ShaderVariableType.Texture2D => typeof(Texture2D),
                ShaderVariableType.RWTexture2D => typeof(RenderTexture),
                ShaderVariableType.SamplerState => typeof(object), // No direct mapping
                _ => typeof(void)
            };
        }
    }

    internal struct Instruction
    {
        public VariableDeclaration result;
        public Expression expression;
    }
}
