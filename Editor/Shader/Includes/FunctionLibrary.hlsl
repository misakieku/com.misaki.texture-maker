#ifndef TEXTUREMAKER_FUNCTION_LIBRARY_HLSL
#define TEXTUREMAKER_FUNCTION_LIBRARY_HLSL

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

inline void SampleTexture2D(Texture2D tex, SamplerState texSampler, float2 uv, out float4 result)
{
    result = tex.SampleLevel(texSampler, uv, 0);
    result.rgb = pow(abs(result.rgb), 0.45454545f); // Linear to sRGB since SampleLevel doesn't do that automatically
}

inline void WriteTexture2D(RWTexture2D<float4> tex, uint2 uv, float4 value)
{
    tex[uv] = value;
}

#endif