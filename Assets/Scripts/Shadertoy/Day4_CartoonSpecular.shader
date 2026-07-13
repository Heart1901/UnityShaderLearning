Shader "Cartoon/Day4_CartoonSpecular" {
    Properties {
        _LightColor ("亮部颜色", Color) = (1, 0.8, 0.8, 1)   // 受光面颜色（浅红）
        _DarkColor ("暗部颜色", Color) = (0.6, 0.2, 0.2, 1)   // 背光面颜色（深红）
        _SpecColor ("高光颜色", Color) = (1, 1, 0.8, 1)      // 高光色（偏黄白）
        _BgColor ("背景颜色", Color) = (0.9, 0.9, 0.9, 1)     // 背景（浅灰）
        _OutlineColor ("描边颜色", Color) = (0, 0, 0, 1)      // 描边（黑色）
        _OutlineWidth ("描边宽度", Range(0.01, 0.1)) = 0.03   // 描边粗细
        _LightDir ("光源方向", Vector) = (1, 1, 0.5, 0)       // 光源方向（右上前方）
        _LightThreshold ("光影阈值", Range(-1, 1)) = 0.2      // 亮暗划分阈值
        _SpecThreshold ("高光阈值", Range(0.5, 0.95)) = 0.85  // 高光范围（值越高范围越小）
        _SpecPower ("高光强度", Range(5, 30)) = 15            // 高光锐利度（值越高越集中）
    }

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 变量声明
            fixed4 _LightColor;
            fixed4 _DarkColor;
            fixed4 _SpecColor;
            fixed4 _BgColor;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float3 _LightDir;
            float _LightThreshold;
            float _SpecThreshold;
            float _SpecPower;

            // 顶点输入
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // 顶点输出（传递给片元着色器的数据）
            struct v2f {
                float4 pos : SV_POSITION;          // 裁剪空间位置
                float2 screenUV : TEXCOORD0;       // 屏幕UV（中心原点）
                float3 normal : TEXCOORD1;         // 球体法线
                float3 viewDir : TEXCOORD2;        // 视角方向（从表面到相机）
            };

            // 顶点着色器：计算球体3D信息
            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // 计算屏幕UV（校正宽高比）
                o.screenUV = (v.uv * 2 - 1);
                o.screenUV.x *= _ScreenParams.x / _ScreenParams.y;

                // 模拟球体3D坐标与法线
                float z = sqrt(1 - dot(o.screenUV, o.screenUV)); // 球体Z轴分量
                o.normal = normalize(float3(o.screenUV, z));     // 法线方向

                // 视角方向（假设相机在Z轴正方向）
                o.viewDir = normalize(float3(0, 0, 2) - float3(o.screenUV, z));
                return o;
            }

            // 片元着色器：计算高光与颜色
            fixed4 frag (v2f i) : SV_Target {
                // 1. 球体与描边判断
                float sphereRadius = 0.5;
                float distToCenter = length(i.screenUV);
                float sphereDist = distToCenter - sphereRadius;
                bool isOutline = (sphereDist >= 0) && (sphereDist < _OutlineWidth);
                bool inSphere = sphereDist < 0;

                // 2. 漫反射亮暗分块（复用Day3逻辑）
                float3 lightDir = normalize(_LightDir.xyz);
                float dotNL = dot(i.normal, lightDir);
                bool isLight = step(_LightThreshold, dotNL);

                // 3. 卡通高光计算
                // 半程向量（光源方向与视角方向的中间向量）
                float3 halfDir = normalize(lightDir + i.viewDir);
                // 法线与半程向量的点积（高光强度基础）
                float dotNH = dot(i.normal, halfDir);
                // 增强高光锐利度（类似Phong模型的幂运算）
                float spec = pow(max(dotNH, 0.0), _SpecPower);
                // 高光阈值判断（只在亮部显示，且强度达标）
                bool hasSpec = isLight && (spec > _SpecThreshold);

                // 4. 颜色输出（优先级：描边 > 高光 > 亮/暗部 > 背景）
                if (isOutline) {
                    return _OutlineColor;
                } else if (inSphere) {
                    return hasSpec ? _SpecColor : (isLight ? _LightColor : _DarkColor);
                } else {
                    return _BgColor;
                }
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
    