Shader "Custom/BumpySphere" {
    Properties {
        _SphereRadius ("球体半径", Float) = 5.0
        _NoiseStrength ("凹凸强度", Float) = 0.8
        _RotateSpeed ("旋转速度", Float) = 0.3
        _Iterations ("射线步进次数", Int) = 60
        _FBMOctaves ("噪声 octaves", Int) = 5
    }

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties 变量声明
            float _SphereRadius;
            float _NoiseStrength;
            float _RotateSpeed;
            int _Iterations;
            int _FBMOctaves;

            // 旋转矩阵（2D）
            float2x2 rotate(float a) {
                float s = sin(a);
                float c = cos(a);
                return float2x2(c, -s, s, c);
            }

            // 分形布朗运动（FBM）噪声
            float fbm(float3 p, float3 seed, float T) {
                float res = 0.0;
                float i = 1.0;
                for(int oct = 0; oct < _FBMOctaves; oct++) {
                    res += abs(dot(cos(p * i - T), seed)) / i;
                    i *= 1.42; // 频率倍增系数
                }
                return res;
            }

            // 顶点着色器（传递UV）
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord; // 传递纹理坐标
                return o;
            }

            // 片元着色器（核心逻辑）
            fixed4 frag(v2f i) : SV_Target {
                // 1. 初始化变量
                float2 R = _ScreenParams.xy; // 屏幕分辨率
                float T = _Time.y * _RotateSpeed; // 时间（控制旋转速度）
                float aspect = R.x / R.y; // 宽高比（避免拉伸）

                // 2. 计算标准化UV（适配屏幕比例）
                float2 uv = (i.uv * 2.0 - 1.0); // [-1,1]范围
                uv.x *= aspect; // 修正宽高比

                // 3. 射线参数
                float3 ro = float3(0, 0, -20); // 射线原点（相机位置）
                float3 rd = normalize(float3(uv, 1.0)); // 射线方向

                // 4. 光线步进（射线与球体相交检测）
                float z = 0.0; // 射线行进距离
                float3 color = float3(0, 0, 0); // 输出颜色
                float hit = 0.0; // 是否命中球体

                for(int step = 0; step < _Iterations; step++) {
                    float3 p = ro + rd * z; // 当前射线位置

                    // 对球体表面点进行旋转（随时间动画）
                    p.xy = mul(rotate(T), p.xy); // XY平面旋转
                    p.xz = mul(rotate(T * 0.7), p.xz); // XZ平面旋转（不同速度增强动态）

                    // 计算FBM噪声（凹凸细节）
                    float3 seed = float3(1.2, 3.4, 5.6); // 噪声种子
                    float noise = fbm(p, seed, T) * _NoiseStrength;

                    // 球体距离函数（基础球体半径 + 噪声扰动）
                    float d = length(p) - (_SphereRadius + noise);

                    // 命中判断（距离小于阈值视为相交）
                    if(d < 0.01) {
                        hit = 1.0;
                        // 5. 计算颜色（基于噪声和法向量的简单光照）
                        float3 normal = normalize(p); // 近似法向量
                        float3 lightDir = normalize(float3(1, 1, -1)); // 光源方向
                        float diff = max(dot(normal, lightDir), 0.2); // 漫反射
                        color = float3(0.8, 0.6, 0.9) * diff; // 基础颜色 + 漫反射
                        break; // 命中后退出循环
                    }

                    // 未命中则继续行进（步进距离为当前距离的1/3，加速收敛）
                    z += d * 0.3;

                    // 超出最大距离则退出（避免无限循环）
                    if(z > 100) break;
                }

                // 6. 输出最终颜色（只保留命中的球体颜色）
                return float4(color * hit, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}