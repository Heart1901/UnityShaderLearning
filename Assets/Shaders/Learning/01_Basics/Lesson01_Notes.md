# Lesson 01 Notes

Goal:

- Understand the minimum ShaderLab structure
- Learn the input/output flow between vertex and fragment stages
- See how a material property reaches the shader

Key points:

- `Properties` exposes editor parameters
- `SubShader` groups one implementation
- `Pass` is one render pass
- `vert` moves vertices to clip space
- `frag` writes the final pixel color

Practice:

1. Change `_Color` in the material.
2. Remove the normal visualisation line and observe the output.
3. Explain why `v.normal * 0.5 + 0.5` makes a visible color.
