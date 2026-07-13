Shader "Custom/BasicToonShader" {
    Properties {
        _BaseColor ("基础颜色", Color) = (0.9, 0.5, 0.2, 1) // 卡通物体主色
        _OutlineColor ("轮廓线颜色", Color) = (0, 0, 0, 1)   // 轮廓线颜色（默认黑色）
        _OutlineWidth ("轮廓线宽度", Range(0.001, 0.05)) = 0.01 // 轮廓线粗细
        _LightStep1 ("光照等级1阈值", Range(0, 1)) = 0.7     // 高光区阈值
        _LightStep2 ("光照等级2阈值", Range(0, 1)) = 0.2     // 阴影区阈值
        _RimLightIntensity ("边缘光强度", Range(0, 2)) = 0.8  // 边缘光亮度
    }

    SubShader {
        // 第一个Pass：渲染轮廓线
        Pass {
            Tags { "LightMode" = "Always" }
            Cull Front // 只渲染背面（利用背面扩张实现轮廓）
            ZWrite On  // 写入深度缓冲，避免轮廓线被物体遮挡

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float _OutlineWidth;

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL; // 模型空间法线
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
            };

            // 顶点着色器：将背面顶点沿法线方向扩张，形成轮廓
            Varyings vert(Attributes input) {
                Varyings output;
                
                // 将法线从模型空间转换到视图空间
                float3 normalVS = UnityObjectToViewPos(input.normalOS);
                // 沿法线方向扩张顶点（视图空间下）
                input.positionOS.xyz += input.normalOS * _OutlineWidth;
                // 转换到裁剪空间
                output.positionHCS = UnityObjectToClipPos(input.positionOS);
                
                return output;
            }

            // 片元着色器：输出轮廓线颜色
            half4 frag(Varyings input) : SV_Target {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // 第二个Pass：渲染卡通主体（带光照）
        Pass {
            Tags { "LightMode" = "ForwardBase" }
            Cull Back // 只渲染正面

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc" // 包含Unity光照相关函数

            float4 _BaseColor;
            float _LightStep1;
            float _LightStep2;
            float _RimLightIntensity;

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0; // 世界空间法线
                float3 viewDirWS : TEXCOORD1; // 世界空间视线方向
            };

            // 顶点着色器：计算世界空间法线和视线方向
            Varyings vert(Attributes input) {
                Varyings output;
                // 转换顶点位置到裁剪空间
                output.positionHCS = UnityObjectToClipPos(input.positionOS);
                // 转换法线到世界空间
                output.normalWS = UnityObjectToWorldNormal(input.normalOS);
                // 计算世界空间视线方向（相机位置 - 顶点世界位置）
                float3 positionWS = mul(unity_ObjectToWorld, input.positionOS).xyz;
                output.viewDirWS = normalize(_WorldSpaceCameraPos.xyz - positionWS);
                
                return output;
            }

            // 片元着色器：实现卡通光照效果
            half4 frag(Varyings input) : SV_Target {
                // 1. 标准化向量
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);
                half3 lightDirWS = normalize(_WorldSpaceLightPos0.xyz); // 主光源方向

                // 2. 卡通阶梯式光照（核心）
                half diff = dot(normalWS, lightDirWS); // 法线与光线夹角的余弦值
                half toonLight = 0;
                
                // 划分光照等级：高光 > 中间调 > 阴影
                if (diff > _LightStep1) {
                    toonLight = 1.0; // 高光区
                } else if (diff > _LightStep2) {
                    toonLight = 0.6; // 中间调
                } else {
                    toonLight = 0.2; // 阴影区
                }

                // 3. 边缘光效果（增强卡通感）
                half rim = 1.0 - dot(normalWS, viewDirWS); // 法线与视线的夹角
                half rimLight = smoothstep(0.7, 0.9, rim) * _RimLightIntensity;

                // 4. 组合最终颜色
                half3 finalColor = _BaseColor.rgb * toonLight + rimLight;
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
    