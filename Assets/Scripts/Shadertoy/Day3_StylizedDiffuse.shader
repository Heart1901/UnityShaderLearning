Shader "Cartoon/Day3_StylizedDiffuse" {
    Properties {
        _LightColor ("亮部颜色", Color) = (1, 0.8, 0.8, 1) // 球体受光面颜色
        _DarkColor ("暗部颜色", Color) = (0.6, 0.2, 0.2, 1) // 球体背光面颜色
        _BgColor ("背景颜色", Color) = (0.9, 0.9, 0.9, 1)
        _OutlineColor ("描边颜色", Color) = (0, 0, 0, 1)
        _OutlineWidth ("描边宽度", Range(0.01, 0.1)) = 0.03
        _LightDir ("光源方向", Vector) = (1, 1, 0, 0) // 世界空间光源方向（x,y,z）
        _LightThreshold ("光影阈值", Range(-1, 1)) = 0.2 // 划分亮暗的临界值
    }

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _LightColor;
            fixed4 _DarkColor;
            fixed4 _BgColor;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float3 _LightDir;
            float _LightThreshold;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 screenUV : TEXCOORD0; // 中心原点，校正宽高比
                float3 normal : TEXCOORD1; // 球体表面法线（模拟3D球体的法线）
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // 计算屏幕UV（同前，中心原点）
                o.screenUV = (v.uv * 2 - 1);
                o.screenUV.x *= _ScreenParams.x / _ScreenParams.y;

                // 模拟球体表面法线：屏幕UV即球体局部坐标（中心为原点，半径1）
                // 球体法线方向 = 表面点坐标的归一化（从球心指向表面）
                o.normal = normalize(float3(o.screenUV, sqrt(1 - dot(o.screenUV, o.screenUV))));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 1. 计算球体距离场（判断是否在球内）
                float sphereDist = length(i.screenUV) - 0.5; // 球体半径0.5

                // 2. 描边判断（复用Day2逻辑）
                bool isOutline = (sphereDist >= 0) && (sphereDist < _OutlineWidth);

                // 3. 风格化漫反射计算
                // 归一化光源方向（世界空间）
                float3 lightDir = normalize(_LightDir.xyz);
                // 计算法线与光源方向的点积（漫反射强度，范围[-1,1]）
                float dotNL = dot(i.normal, lightDir);

                // 用step函数离散化：>阈值为亮部，否则为暗部（无中间过渡）
                bool isLight = step(_LightThreshold, dotNL);

                // 4. 内部色块判断（是否在球内）
                bool inSphere = sphereDist < 0;

                // 5. 颜色输出优先级：描边 > 球体亮部/暗部 > 背景
                if (isOutline) {
                    return _OutlineColor;
                } else if (inSphere) {
                    return isLight ? _LightColor : _DarkColor; // 亮暗分块
                } else {
                    return _BgColor;
                }
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}