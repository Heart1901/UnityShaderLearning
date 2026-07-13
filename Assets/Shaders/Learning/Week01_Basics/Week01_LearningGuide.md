# Week 01 - Shader 基础到风格化入门成品

这一周的目标不是背概念，而是建立一条完整链路：模型顶点进入 Shader，经过坐标变换、法线计算、光照、UV 采样，最后组合成一个小型风格化效果。

## 最终场景

打开这个场景：

`Assets/Scenes/Learning/Week01_ShaderBasics_Demo.unity`

场景中有 7 个展示物体，对应 7 天内容。最后一个大物体使用 `W01D07_StylizedFinal.shader`，它把前 6 天的知识合成了一个简单但完整的风格化材质：贴图、色阶光照、边缘光、描边。

## 第 1 天：最小 Shader

文件：`W01D01_UnlitColor.shader`

重点：

- `Properties` 是材质面板暴露出来的参数。
- `vertex shader` 把模型顶点从 object space 转到 clip space。
- `fragment shader` 决定屏幕上每个片元输出什么颜色。

你要看懂这一句：

```hlsl
output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
```

它代表：模型自己的顶点坐标，最终要变成 GPU 能画到屏幕上的裁剪空间坐标。

## 第 2 天：顶点数据传给片元

文件：`W01D02_ObjectSpaceGradient.shader`

重点：

- 顶点阶段可以计算数据。
- 片元阶段可以接收顶点阶段传下来的插值结果。
- 同一个模型，不同高度可以显示不同颜色。

这一天要理解 `v2f` / `Varyings`：它是 vertex 到 fragment 的桥。

## 第 3 天：法线可视化

文件：`W01D03_NormalVisualizer.shader`

重点：

- 法线是方向向量，不是颜色。
- 为了显示到屏幕上，我们把 `-1..1` 的方向映射到 `0..1` 的颜色。

核心公式：

```hlsl
normalColor = normalWS * 0.5 + 0.5;
```

这能帮助你直观看到模型表面每个位置的法线方向。

## 第 4 天：Lambert 漫反射

文件：`W01D04_LambertDiffuse.shader`

重点：

- 光照最核心的一步是 `dot(normal, lightDir)`。
- 点积越大，说明表面越朝向光源，颜色越亮。
- 点积越小，说明表面背光，颜色越暗。

核心公式：

```hlsl
ndotl = saturate(dot(normalWS, lightDirWS));
```

`saturate` 会把值限制在 `0..1`。

## 第 5 天：色阶光照

文件：`W01D05_ToonBandLighting.shader`

重点：

- 写实光照是连续渐变。
- 风格化光照经常把连续渐变压成明显的亮暗块面。
- `smoothstep` 可以控制明暗分界线的软硬。

你要调两个参数：

- `_BandCenter`：明暗分界位置。
- `_BandSoftness`：边界软硬程度。

## 第 6 天：UV 与贴图采样

文件：`W01D06_TextureUV.shader`

重点：

- UV 是模型顶点携带的二维坐标。
- 贴图采样就是用 UV 去图片上取颜色。
- `_BaseMap_ST` 支持材质面板里的 Tiling 和 Offset。

核心代码：

```hlsl
uv = TRANSFORM_TEX(input.uv, _BaseMap);
color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
```

## 第 7 天：第一周成品 Shader

文件：`W01D07_StylizedFinal.shader`

它组合了：

- 贴图采样：来自第 6 天。
- 法线方向：来自第 3 天。
- 光照点积：来自第 4 天。
- 色阶明暗：来自第 5 天。
- 边缘光：使用 `1 - dot(normal, viewDir)`。
- 描边：额外 Pass，把模型沿法线方向放大一点，再只画背面。

你学习这个文件时，建议按顺序看：

1. `Properties`
2. `Outline` Pass
3. `Forward` Pass 的 `Attributes`
4. `Forward` Pass 的 `Varyings`
5. `vert`
6. `frag`

## 第一周你需要真正掌握的 8 个词

- `POSITION`
- `NORMAL`
- `TEXCOORD0`
- `SV_POSITION`
- object space
- world space
- clip space
- dot product

## 学习方式

第一遍只看场景效果和材质参数，不急着改代码。

第二遍打开 Shader 文件，只看数据怎么传：

`Attributes -> Varyings -> frag`

第三遍开始改参数：

- 改颜色。
- 改 `_BandCenter`。
- 改 `_BandSoftness`。
- 改 `_RimPower`。
- 改 `_OutlineWidth`。

第四遍再改代码：

- 把色阶从 2 档改成 3 档。
- 把 rim light 颜色换成只在阴影处出现。
- 尝试让描边宽度随视角变化。

这周结束时，你不需要记住所有 API，但要能讲清楚：顶点数据如何进入 Shader，法线如何参与光照，UV 如何采样贴图，以及为什么风格化光照可以从 Lambert 改出来。
