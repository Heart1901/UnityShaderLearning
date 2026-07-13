# Week 02 - 风格化 Toon 渲染

第 1 周我们解决了顶点、法线、UV、Lambert、色阶光照和简单成品。

第 2 周开始进入更接近商业项目的风格化渲染：Ramp、描边、Rim Light、卡通高光、MatCap、阴影纹理，最后组合成一个可展示的 Toon 材质。

## 最终场景

场景目标：

`Assets/Scenes/Learning/Week02_StylizedToon_Demo.unity`

最终 Shader：

`Assets/Shaders/Learning/Week02_StylizedToon/W02D07_AdvancedToonProduct.shader`

## 第 1 天：Ramp 光照

文件：`W02D01_RampDiffuse.shader`

第 1 周的 Toon 光照是用 `smoothstep` 把明暗切成两档。第 2 周换成 Ramp 贴图。

核心逻辑：

```hlsl
ndotl = saturate(dot(normalWS, lightDir));
ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(ndotl, 0.5));
```

Ramp 的优势是：美术可以通过一张小图控制明暗层次、颜色倾向和过渡风格。

## 第 2 天：描边 Pass

文件：`W02D02_OutlinePass.shader`

核心思路：

1. 多画一个 Pass。
2. `Cull Front`，只显示模型背面。
3. 顶点沿法线方向外扩一点。
4. 输出纯色描边。

这是一种经典的法线外扩描边，适合角色和风格化物体。

## 第 3 天：Rim Light

文件：`W02D03_RimLight.shader`

Rim Light 的本质是视线方向和法线方向的关系。

```hlsl
rim = pow(1 - dot(normalWS, viewDirWS), _RimPower);
```

物体边缘处，法线和视线接近垂直，所以 rim 会变强。

## 第 4 天：卡通高光

文件：`W02D04_ToonSpecular.shader`

写实高光是连续的，卡通高光通常是一个形状明确的亮斑。

做法：

1. 用 Blinn-Phong 得到连续高光。
2. 用 `smoothstep` 把它压成色块。

```hlsl
specRaw = pow(dot(normalWS, halfDirWS), _SpecPower);
specBand = smoothstep(_SpecThreshold, _SpecThreshold + _SpecSoftness, specRaw);
```

## 第 5 天：MatCap

文件：`W02D05_MatCap.shader`

MatCap 使用视空间法线去采样一张材质球贴图。

```hlsl
normalVS = mul((float3x3)GetWorldToViewMatrix(), normalWS);
matcapUV = normalVS.xy * 0.5 + 0.5;
```

它很适合快速增加“材质感”，比如金属、陶瓷、塑料、宝石、皮革。

## 第 6 天：阴影纹理

文件：`W02D06_ShadowPattern.shader`

风格化阴影不一定是纯色，可以叠一层纹理：

- 漫画网点
- 手绘噪声
- 斜线阴影
- 笔触纹理

这一课重点是：只在暗部叠加纹理，而不是全身乱铺。

## 第 7 天：高级 Toon 成品

文件：`W02D07_AdvancedToonProduct.shader`

组合内容：

- Ramp 光照
- 法线外扩描边
- Rim Light
- 卡通高光
- MatCap 补光
- 暗部纹理

这份 Shader 就是第 2 周的学习核心。你看代码时建议按这个顺序：

1. 看 `Properties`，理解材质参数。
2. 看 `Outline` Pass，理解描边。
3. 看 `AdvancedToon` Pass 的 `Attributes` 和 `Varyings`。
4. 看 `frag` 里每一块效果怎么累加。

## 面试表达

第 2 周完成后，你应该能这样介绍：

“我实现了一套基于 URP 的风格化 Toon 材质，包含 Ramp 阶梯光照、法线外扩描边、视角相关 Rim Light、卡通化 Blinn-Phong 高光、MatCap 材质补光以及暗部纹理控制。材质参数可调，适合角色和物体风格化表现。”

这个表达就比“我会写 Toon Shader”高级很多。
