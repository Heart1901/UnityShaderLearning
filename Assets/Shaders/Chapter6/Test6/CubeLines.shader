Shader "Custom/CubeLines"
{
    Properties
    {
        _MainTex("Background Texture", 2D) = "white" {}
        _RotationSpeed("Rotation Speed", Range(0, 5)) = 1
        _StaticShape("Static Shape", Range(0, 1)) = 0.5
        _CameraFar("Camera Distance", Range(0.1, 10)) = 2
        _UseCustomColor("Use Custom Colors", Int) = 0
        _ColorBlue("Blue Color", Color) = (0.5,0.65,0.8,1)
        _ColorRed("Red Color", Color) = (0.99,0.2,0.1,1)
        _AnimateShape("Animate Shape", Int) = 0
        _AnimateColor("Animate Color", Int) = 0
    }

        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            LOD 100
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float3 viewDir : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _RotationSpeed;
                float _StaticShape;
                float _CameraFar;
                int _UseCustomColor;
                float4 _ColorBlue;
                float4 _ColorRed;
                int _AnimateShape;
                int _AnimateColor;

                #define PI 3.1415926
                #define FDIST 0.7
                #define BOXDIMS float3(0.75, 0.75, 1.25)
                #define IOR 1.33

                float3x3 rotx(float a)
                {
                    float s = sin(a);
                    float c = cos(a);
                    return float3x3(1,0,0, 0,c,s, 0,-s,c);
                }

                float3x3 roty(float a)
                {
                    float s = sin(a);
                    float c = cos(a);
                    return float3x3(c,0,s, 0,1,0, -s,0,c);
                }

                float3x3 rotz(float a)
                {
                    float s = sin(a);
                    float c = cos(a);
                    return float3x3(c,s,0, -s,c,0, 0,0,1);
                }

                float3 fcos(float3 x) { return cos(x); }

                float3 getColor(float3 p)
                {
                    p = abs(p) * 1.25;
                    p = 0.5 * p / dot(p, p);
                    #if _AnimateColor
                    p += 0.072 * _Time.y;
                    #endif
                    float t = 0.13 * length(p);
                    float3 col = 0.3 + 0.12 * fcos(6.283 * t + 0) + 0.11 * fcos(6.283 * 3.1 * t + 0.3)
                        + 0.1 * fcos(6.283 * 5.1 * t + 0.1) + 0.1 * fcos(6.283 * 17.1 * t + 0.2)
                        + 0.1 * fcos(6.283 * 31.1 * t + 0.1) + 0.1 * fcos(6.283 * 65.1 * t + 0)
                        + 0.1 * fcos(6.283 * 115.1 * t + 0.1) + 0.1 * fcos(6.283 * 265.1 * t + 1.1);
                    return saturate(col);
                }

                float3 background(float3 ro, float3 rd, out float alpha)
                {
                    alpha = 1;
                    float t = (-BOXDIMS.z - ro.z) / rd.z;
                    if (t < 0) return 0.01;
                    float2 uv = ro.xy + t * rd.xy;
                    alpha = smoothstep(7,10,length(uv));
                    return lerp(0.336, 0.01, alpha);
                }

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.viewDir = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz;
                    return o;
                }

                float box(float3 ro, float3 rd, float3 r, out float3 nn, bool entering)
                {
                    rd += 0.0001 * (1 - abs(sign(rd)));
                    float3 invRd = 1 / rd;
                    float3 n = ro * invRd;
                    float3 k = r * abs(invRd);
                    float3 pin = -k - n;
                    float3 pout = k - n;
                    float tin = max(pin.x, max(pin.y, pin.z));
                    float tout = min(pout.x, min(pout.y, pout.z));
                    if (tin > tout) return -1;
                    nn = entering ? -sign(rd) * step(pin.xyz, pin.zxy) * step(pin.xyz, pin.yzx) :
                                    sign(rd) * step(pout.zxy, pout.xyz) * step(pout.yzx, pout.xyz);
                    return entering ? tin : tout;
                }

                float4 frag(v2f i) : SV_TARGET
                {
                    float2 uv = i.uv * _ScreenParams.xy / _ScreenParams.y;
                    float2 q = uv * 2 - 1; q.x *= _ScreenParams.x / _ScreenParams.y;

                    float mouseY = _AnimateShape ? 0 : PI * 0.49;
                    float mouseX = -2 * PI - 0.25 * _Time.y * _RotationSpeed;
                    float3 eye = (2 + _CameraFar) * float3(cos(mouseX) * cos(mouseY), sin(mouseX) * cos(mouseY), sin(mouseY));

                    float3 w = normalize(-eye);
                    float3 u = normalize(cross(w, float3(0,0,1)));
                    float3 v = cross(u, w);
                    float3 rd = normalize(w * FDIST + q.x * u + q.y * v);

                    float3 ni;
                    float t = box(eye, rd, BOXDIMS, ni, true);
                    float3 ro = eye + t * rd;

                    float alpha_bg; // 定义变量用于background的out参数
                    float4 color = t > 0 ?
                        float4(getColor(ro), 1) :
                        float4(background(eye, rd, alpha_bg), alpha_bg); // 传入变量

                    color = lerp(tex2D(_MainTex, i.uv), color, color.a);
                    color.rgb *= max(0.5, dot(ni, _WorldSpaceLightPos0.xyz));

                    return color;
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}