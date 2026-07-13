Shader "Custom/Day3_DistSegment"
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

            float distSegment(float2 p, float2 a, float2 b) {
                float2 ab = b - a;
                float2 ap = p - a;
                float t = dot(ap, ab) / dot(ab, ab);
                t = clamp(t, 0.0, 1.0);
                float2 closest = a + ab * t;
                return length(p - closest);
            }

            float4 frag (v2f i) : SV_Target {
                float2 uv = i.uv;
                // 映射到 [-1,1] 范围，保持正方形
                float2 p = (uv - 0.5) * 2.0;
                p.x *= _ScreenParams.x / _ScreenParams.y;

                float2 a = float2(-0.5, 0.0);
                float2 b = float2(0.5, 0.0);

                float d = distSegment(p, a, b);

                float3 col = float3(0,0,0);
                if (d < 0.02)
                    col = float3(1,1,1);
                else if (d < 0.1)
                    col = float3(0.2, 0.4, 0.8);
                else if (d < 0.2)
                    col = float3(0.1, 0.2, 0.5);

                // 等高线
                float level = frac(d * 20.0);
                if (level < 0.05)
                    col = lerp(col, float3(0.8,0.8,0.8), 0.5);

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}