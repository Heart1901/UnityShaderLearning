# 2026-07-16 - Fractal Pyramid

Original case: https://www.shadertoy.com/view/tsXBzS

- Author: bradjamesgrant
- Title: Fractal Pyramid
- Public code reference: https://api.arcade.academy/en/2.6.8/tutorials/shader_toy/fractal_pyramid.html
- Public port reference: https://godotshaders.com/shader/fractal-pyramid/
- Unity shader: `Shaders/FractalPyramid.shader`
- Unity material: `Materials/FractalPyramid.mat`
- Demo scene: `Scenes/FractalPyramid_Demo.unity`
- Preview image: `Preview/FractalPyramid_Preview.png`

![Static preview](Preview/FractalPyramid_Preview.png)

## Daily Goal

Rebuild ShaderToy `tsXBzS` as a Unity URP full-screen raymarching case that can be opened directly as a demo scene.

## Core Ideas

- A camera ray is built from `ro`, `cf`, `cs`, and `cu`, then marched through a procedural distance field.
- `MapFractal` repeats eight folds: rotate `xz`, rotate `xy`, mirror `xz` with `abs`, then subtract an offset.
- The marcher accumulates emissive color along both hit and near-miss rays with `palette(length(p) * 0.1) / d`.
- The shader uses a small built-in exposure/tone-map step so the glow is visible even without Unity Bloom post-processing.
- The final look depends on glow intensity, a dark background, and the repeated folding of 3D space.

## Unity Migration Notes

- ShaderToy `iTime` maps to Unity `_Time.y * _TimeScale`.
- ShaderToy `iResolution` is replaced by mesh UV plus `_ScreenParams.x / _ScreenParams.y`.
- GLSL `vec*`, `mat2`, and `mix` become HLSL `float*`, a custom `Rotate2D`, and `lerp`.
- The original fragment shader is implemented as an URP `UniversalForward` unlit pass.
- The demo script draws both a generated full-screen quad and an `OnGUI` material fallback, so the Game view still shows the shader if the quad path is not visible.

## How To Use

1. Open `Scenes/FractalPyramid_Demo.unity`.
2. Enter Play Mode.
3. Select `Fractal Pyramid Demo Camera` to tune the display script if needed.
4. Tune the material properties: `_TimeScale`, `_Zoom`, `_GlowIntensity`, `_Exposure`, `_ColorA`, `_ColorB`, `_BackgroundColor`, `_FoldOffset`, `_HitThreshold`, and `_MaxDistance`.

## What To Remember

This case is the next step after 2D ShaderToy ports: it introduces ray construction, distance-field marching, iterative folding, and emissive accumulation. It is still compact enough to study line by line.
