# 2026-07-16 - Journey Tribute

Original reference: https://www.shadertoy.com/view/ldlcRf

- Author: Shakemayster
- Title: Tribute - Journey!
- Unity shader: `Shaders/JourneyTribute.shader`
- Unity material: `Materials/JourneyTribute.mat`
- Demo scene: `Scenes/JourneyTribute_Demo.unity`
- Preview image: `Preview/JourneyTribute_Preview.png`

![Static preview](Preview/JourneyTribute_Preview.png)

## Daily Goal

Rebuild the visual idea of ShaderToy `ldlcRf` as a Unity URP full-screen shader: a Journey-inspired golden desert scene with a central glowing pyramid, layered dunes, foreground traveler, cloth flyers, distant ruins, light beams, sand ridges, and drifting spark particles.

## Core Ideas

- Use layered height curves and perspective sand ridges to fake a broad desert floor.
- Build atmospheric depth with sky gradients, sun disk/glow, volumetric-looking beams, cloud noise, and horizon haze.
- Draw the pyramid, ruins, foreground traveler, hood, scarf, and gold trim as procedural masks instead of meshes.
- Add cloth flyers and sand sparkle using repeated SDF-like masks.
- Keep the result as a single full-screen shader so it is easy to inspect and modify.

## Unity Migration Notes

- This is a reference recreation, not a byte-for-byte port of the original ShaderToy source.
- ShaderToy `iTime` maps to Unity `_Time.y * _TimeScale`.
- ShaderToy `iResolution` is replaced by mesh UV plus `_ScreenParams.x / _ScreenParams.y`.
- The shader is an URP `UniversalForward` unlit pass.
- The demo script draws both a generated full-screen quad and an `OnGUI` material fallback.

## How To Use

1. Open `Scenes/JourneyTribute_Demo.unity`.
2. Enter Play Mode.
3. Select `Journey Tribute Demo Camera`.
4. Tune the material properties: `_TimeScale`, `_SunGlow`, `_DuneContrast`, `_CloudAmount`, `_ParticleAmount`, `_TravelerScale`, and the palette colors.

## What To Remember

This case is useful for learning how a complex scenic ShaderToy can be approximated as smaller procedural layers: sky first, terrain bands next, then foreground character and atmospheric detail.
