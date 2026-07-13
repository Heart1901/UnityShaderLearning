Shader "Custom/Day4_2D_SDF"
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

            float sdBox(float2 p, float2 b) {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            float sdSegment(float2 p, float2 a, float2 b) {
                float2 pa = p - a;
                float2 ba = b - a;
                float t = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
                return length(pa - ba * t);
            }

            float opUnion(float d1, float d2) { return min(d1, d2); }
            float opIntersection(float d1, float d2) { return max(d1, d2); }
            float opSubtract(float d1, float d2) { return max(-d1, d2); }

            float4 frag (v2f i) : SV_Target {
                // 归一化 UV，原点居中，y 范围 -0.5~0.5，x 根据宽高比缩放
                float2 uv = (i.uv - 0.5) * float2(_ScreenParams.x / _ScreenParams.y, 1.0);
                
                // 三个图元
                float d_circle = sdCircle(uv - float2(0.3, 0.0), 0.3);
                float d_rect   = sdBox(uv - float2(-0.3, 0.0), float2(0.25, 0.15));
                float d_line   = sdSegment(uv, float2(-0.2, -0.4), float2(0.2, 0.4));
                
                // 圆形 ∪ 矩形
                float d_union = opUnion(d_circle, d_rect);
                
                float3 col = (d_union < 0.0) ? float3(0.3, 0.8, 0.3) : float3(0,0,0);
                if (d_line < 0.03) col = float3(1.0, 1.0, 1.0);
                
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}