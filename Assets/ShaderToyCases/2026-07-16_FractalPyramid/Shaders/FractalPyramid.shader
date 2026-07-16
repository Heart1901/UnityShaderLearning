Shader "ShaderToy/2026-07-16/FractalPyramid"
{
    // Adapted from ShaderToy tsXBzS:
    // https://www.shadertoy.com/view/tsXBzS
    Properties
    {
        _TimeScale ("Time Scale", Range(0, 3)) = 1
        _Zoom ("Zoom", Range(0.3, 2.5)) = 1
        _GlowIntensity ("Glow Intensity", Range(0, 50)) = 4
        _Exposure ("Exposure", Range(0.1, 5)) = 0.55
        _FoldOffset ("Fold Offset", Range(0.1, 1.0)) = 0.5
        _HitThreshold ("Hit Threshold", Range(0.001, 0.08)) = 0.02
        _MaxDistance ("Max Distance", Range(10, 160)) = 100
        _ColorA ("Color A", Color) = (0.2, 0.7, 0.9, 1)
        _ColorB ("Color B", Color) = (1.0, 0.0, 1.0, 1)
        _BackgroundColor ("Background Color", Color) = (0.02, 0.0, 0.04, 1)
        _DrawBackground ("Draw Background", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            Name "FractalPyramid"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _TimeScale;
            float _Zoom;
            float _GlowIntensity;
            float _Exposure;
            float _FoldOffset;
            float _HitThreshold;
            float _MaxDistance;
            float4 _ColorA;
            float4 _ColorB;
            float4 _BackgroundColor;
            float _DrawBackground;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(input.positionOS);
                o.uv = input.uv;
                return o;
            }

            float2 Rotate2D(float2 p, float a)
            {
                float c = cos(a);
                float s = sin(a);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            float3 Palette(float d)
            {
                return lerp(_ColorA.rgb, _ColorB.rgb, saturate(d));
            }

            float MapFractal(float3 p, float time)
            {
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    float foldTime = time * 0.2;
                    p.xz = Rotate2D(p.xz, foldTime);
                    p.xy = Rotate2D(p.xy, foldTime * 1.89);
                    p.xz = abs(p.xz);
                    p.xz -= _FoldOffset;
                }

                return dot(sign(p), p) / 5.0;
            }

            float4 Raymarch(float3 ro, float3 rd, float time)
            {
                float travel = 0.0;
                float3 color = float3(0.0, 0.0, 0.0);
                float distanceField = 1.0;
                bool hit = false;

                [loop]
                for (int stepIndex = 0; stepIndex < 64; stepIndex++)
                {
                    float3 p = ro + rd * travel;
                    distanceField = MapFractal(p, time) * 0.5;

                    if (distanceField < _HitThreshold)
                    {
                        hit = true;
                        break;
                    }

                    if (distanceField > _MaxDistance || travel > _MaxDistance)
                    {
                        break;
                    }

                    float safeDistance = max(distanceField, 0.0001);
                    color += Palette(length(p) * 0.1) * _GlowIntensity / (400.0 * safeDistance);
                    travel += safeDistance;
                }

                float3 background = _DrawBackground > 0.5 ? _BackgroundColor.rgb : float3(0.0, 0.0, 0.0);
                float3 finalColor = background + color;
                finalColor = 1.0 - exp(-finalColor * _Exposure);
                return float4(finalColor, 1.0);
            }

            float4 frag(Varyings i) : SV_Target
            {
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float2 uv = i.uv - 0.5;
                uv.y /= max(aspect, 0.001);
                uv /= max(_Zoom, 0.001);

                float time = _Time.y * _TimeScale;
                float3 ro = float3(0.0, 0.0, -50.0);
                ro.xz = Rotate2D(ro.xz, time);

                float3 cameraForward = normalize(-ro);
                float3 cameraSide = normalize(cross(cameraForward, float3(0.0, 1.0, 0.0)));
                float3 cameraUp = normalize(cross(cameraForward, cameraSide));
                float3 viewPoint = ro + cameraForward * 3.0 + uv.x * cameraSide + uv.y * cameraUp;
                float3 rd = normalize(viewPoint - ro);

                return Raymarch(ro, rd, time);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

