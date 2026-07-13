Shader "Custom/Day9_FBMTest" {
    SubShader {
        Cull Off ZWrite Off ZTest Always
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            float hash(float2 p) {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            // 平滑插值的噪声函数 (每日任务：尝试替换这里的噪声实现)
            float noise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // 核心 fBm 函数：叠加多层噪声，增加细节和复杂度
            float fbm(float2 p, int octaves, float persistence, float lacunarity) {
                float value = 0.0;
                float amplitude = 0.5;   // 初始振幅
                float frequency = 4.0;    // 初始频率，提升基础细节
                for (int i = 0; i < octaves; i++) {
                    value += amplitude * noise(p * frequency);
                    amplitude *= persistence;     // 每层振幅衰减
                    frequency *= lacunarity;      // 每层频率倍增
                }
                return value;
            }

            float4 frag(v2f_img i) : SV_Target {
                float2 uv = i.uv * 3.0;  // 扩大显示范围
                float col = fbm(uv, 4, 0.5, 2.0);
                return float4(col, col, col, 1);
            }
            ENDCG
        }
    }
}