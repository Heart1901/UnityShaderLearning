# ShaderToy Cases

这个目录专门用于学习 ShaderToy 官网案例。

它不属于项目里的风格化渲染学习计划，而是一个独立的每日训练区：目标是每天从 ShaderToy 官网选择一个 shader，理解它的核心思路，并在 Unity 中实现或迁移一个可运行版本。

建议每个案例单独建一个子目录，按日期或序号命名：

```text
Assets/ShaderToyCases/
  2026-07-15_RaymarchingSphere/
    README.md
    Shaders/
    Materials/
    Textures/
    Scenes/
```

每个案例的 README 建议记录：

- ShaderToy 原始链接
- 原作者和 shader 名称
- 今日目标：完整迁移、局部复现，或只学习某个函数
- 核心数学概念：例如 UV、噪声、SDF、raymarching、调色、后处理
- Unity 迁移要点：iTime、iResolution、iMouse、iChannel 如何替换
- 遇到的问题和修正方式
- 今日总结：这个 shader 最值得记住的 1-3 个点

每日流程建议：

1. 在 ShaderToy 官网选择一个案例
2. 先运行和观察效果，不急着改代码
3. 标出入口函数 `mainImage`
4. 找出 1-2 个核心函数或公式
5. 在 Unity 中实现最小可运行版本
6. 截图并写下今日总结

最低完成标准：

- 有一个 Unity shader 文件
- 有一个材质或展示场景
- README 中保留原始 ShaderToy 链接
- 能用自己的话解释今天学到的核心技巧
