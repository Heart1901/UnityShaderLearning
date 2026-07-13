Shader "Custom/Day5_SmoothUnion"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float sdCircle(float2 p, float r) {
                return length(p) - r;
            }

            float smin(float a, float b, float k) {
                k *= 6.0;
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * h * k * (1.0 / 6.0);
            }

            float4 frag (v2f i) : SV_Target {
                float2 uv = (i.uv - 0.5) * float2(_ScreenParams.x / _ScreenParams.y, 1.0);
                
                // 定义两个圆的圆心
                float2 p1 = float2(0.0, 0.0);
                float2 p2 = float2(0.5, 0.3);
                
                float d1 = sdCircle(uv - p1, 0.4);
                float d2 = sdCircle(uv - p2, 0.4);
                
                // 平滑融合 (k=0.3)
                float d = smin(d1, d2, 0.3);
                
                float3 col = float3(0,0,0);
                if (abs(d) < 0.02)
                    col = float3(1,1,1);
                else if (d < 0.0)
                    col = float3(0.2, 0.8, 0.2);
                
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}