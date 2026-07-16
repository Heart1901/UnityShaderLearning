Shader "ShaderToy/2026-07-15/ShaderArtCodingIntro"
{
    // Adapted from ShaderToy mtyGWy by kishimisu:
    // https://www.shadertoy.com/view/mtyGWy
    Properties
    {
        _Intensity ("Intensity", Range(0, 3)) = 1
        _TimeScale ("Time Scale", Range(0, 3)) = 1
        _TileScale ("Tile Scale", Range(1, 3)) = 1.5
        _LineScale ("Line Scale", Range(1, 16)) = 8
        _GlowWidth ("Glow Width", Range(0.001, 0.05)) = 0.01
        _GlowPower ("Glow Power", Range(0.2, 3)) = 1.2
        _Aspect ("Aspect Override", Range(0, 4)) = 0
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
            Name "ShaderToy"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _Intensity;
            float _TimeScale;
            float _TileScale;
            float _LineScale;
            float _GlowWidth;
            float _GlowPower;
            float _Aspect;
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

            float3 Palette(float t)
            {
                float3 a = float3(0.5, 0.5, 0.5);
                float3 b = float3(0.5, 0.5, 0.5);
                float3 c = float3(1.0, 1.0, 1.0);
                float3 d = float3(0.263, 0.416, 0.557);
                return a + b * cos(6.28318 * (c * t + d));
            }

            float4 frag(Varyings i) : SV_Target
            {
                float aspect = _Aspect > 0.001 ? _Aspect : _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float2 uv = i.uv * 2.0 - 1.0;
                uv.x *= aspect;

                float2 uv0 = uv;
                float t = _Time.y * _TimeScale;
                float3 finalColor = float3(0.0, 0.0, 0.0);

                [unroll]
                for (int layer = 0; layer < 4; layer++)
                {
                    uv = frac(uv * _TileScale) - 0.5;

                    float d = length(uv) * exp(-length(uv0));
                    float3 layerColor = Palette(length(uv0) + layer * 0.4 + t * 0.4);

                    d = sin(d * _LineScale + t) / _LineScale;
                    d = max(abs(d), 0.0001);
                    d = pow(_GlowWidth / d, _GlowPower);

                    finalColor += layerColor * d;
                }

                return float4(finalColor * _Intensity, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
