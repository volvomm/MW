#ifndef SPRITE_OUTLINE_FUNCTION_INCLUDED
#define SPRITE_OUTLINE_FUNCTION_INCLUDED

void SpriteOutlineMask_float(
    float2 UV,
    float OutlineThickness,
    float AlphaThreshold,
    UnityTexture2D SpriteTex,
    out float OutlineMask,
    out float BaseAlpha
)
{
    float4 baseSample = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV);
    BaseAlpha = baseSample.a;

    float2 texelSize = SpriteTex.texelSize.xy * OutlineThickness;

    float a1 = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV + float2( texelSize.x, 0)).a;
    float a2 = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV + float2(-texelSize.x, 0)).a;
    float a3 = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV + float2(0,  texelSize.y)).a;
    float a4 = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV + float2(0, -texelSize.y)).a;

    float a5 = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV + float2( texelSize.x,  texelSize.y)).a;
    float a6 = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV + float2(-texelSize.x,  texelSize.y)).a;
    float a7 = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV + float2( texelSize.x, -texelSize.y)).a;
    float a8 = SAMPLE_TEXTURE2D(SpriteTex.tex, SpriteTex.samplerstate, UV + float2(-texelSize.x, -texelSize.y)).a;

    float neighborAlpha = max(max(a1, a2), max(a3, a4));
    neighborAlpha = max(neighborAlpha, max(max(a5, a6), max(a7, a8)));

    float isBaseEmpty = BaseAlpha <= AlphaThreshold ? 1.0 : 0.0;
    float hasNeighbor = neighborAlpha > AlphaThreshold ? 1.0 : 0.0;

    OutlineMask = isBaseEmpty * hasNeighbor;
}

#endif