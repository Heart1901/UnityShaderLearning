# 2026-07-15 - Shader Art Coding Intro

Original case: https://www.shadertoy.com/view/mtyGWy

- Author: kishimisu
- Title: An introduction to Shader Art Coding
- Public reference port: https://godotshaders.com/shader/port-of-an-introduction-to-shader-art-coding-by-kishimisu/
- Unity shader: `Shaders/ShaderArtCodingIntro.shader`
- Unity material: `Materials/ShaderArtCodingIntro.mat`
- Demo scene: `Scenes/ShaderArtCodingIntro_Demo.unity`
- Preview image: `Preview/ShaderArtCodingIntro_Preview.png`

![Static preview](Preview/ShaderArtCodingIntro_Preview.png)

## Daily Goal

Rebuild the classic ShaderToy fragment-only effect in Unity as an Unlit shader that can run on a quad or full-screen mesh.

## Core Ideas

- `palette(t)` uses cosine waves to produce a looping color palette.
- `uv = frac(uv * 1.5) - 0.5` repeatedly folds UVs into smaller centered cells.
- `length(uv) * exp(-length(uv0))` creates a distance field that fades away from the original center.
- `sin`, `abs`, and `pow` turn the distance value into animated glowing rings/lines.

## Unity Migration Notes

- ShaderToy `mainImage(out vec4 fragColor, in vec2 fragCoord)` becomes Unity's fragment function `frag(v2f i)`.
- ShaderToy `iTime` maps to Unity `_Time.y`.
- ShaderToy `iResolution.xy` is replaced by mesh UV plus `_ScreenParams.x / _ScreenParams.y` for aspect correction.
- GLSL `vec2`, `vec3`, `vec4` become HLSL `float2`, `float3`, `float4`.
- GLSL `fract` becomes HLSL `frac`.

## How To Use

1. Open `Scenes/ShaderArtCodingIntro_Demo.unity`.
2. Select `ShaderToy Demo Camera`.
3. Enter Play Mode to see the animated palette and glowing folded rings.
4. Tune the material properties: `_Intensity`, `_TimeScale`, `_TileScale`, `_LineScale`, `_GlowWidth`, and `_GlowPower`.

## What To Remember

This shader is a good first ShaderToy migration because it has no textures, no lighting model, and no raymarching. The important lesson is how much visual richness can come from UV folding, a small palette function, and a distance-to-glow remap.
