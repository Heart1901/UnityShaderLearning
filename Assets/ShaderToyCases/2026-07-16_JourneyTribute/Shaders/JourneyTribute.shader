Shader "ShaderToy/2026-07-16/JourneyTribute"
{
    // Reference recreation of ShaderToy ldlcRf, "Tribute - Journey!" by Shakemayster:
    // https://www.shadertoy.com/view/ldlcRf
    Properties
    {
        _TimeScale ("Time Scale", Range(0, 3)) = 0.65
        _SunGlow ("Sun Glow", Range(0, 4)) = 2.15
        _DuneContrast ("Dune Contrast", Range(0, 2)) = 1.45
        _CloudAmount ("Cloud Amount", Range(0, 2)) = 0.95
        _ParticleAmount ("Particle Amount", Range(0, 2)) = 1.15
        _TravelerScale ("Traveler Scale", Range(0.5, 2)) = 1.18
        _SkyTop ("Sky Top", Color) = (0.64, 0.42, 0.72, 1)
        _SkyHorizon ("Sky Horizon", Color) = (1.0, 0.63, 0.31, 1)
        _SunColor ("Sun Color", Color) = (1.0, 0.86, 0.34, 1)
        _DuneFar ("Dune Far", Color) = (0.77, 0.34, 0.24, 1)
        _DuneNear ("Dune Near", Color) = (1.0, 0.59, 0.19, 1)
        _RobeColor ("Robe Color", Color) = (0.46, 0.055, 0.07, 1)
        _ScarfColor ("Scarf Color", Color) = (1.0, 0.72, 0.22, 1)
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
            Name "JourneyTribute"
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
            float _SunGlow;
            float _DuneContrast;
            float _CloudAmount;
            float _ParticleAmount;
            float _TravelerScale;
            float4 _SkyTop;
            float4 _SkyHorizon;
            float4 _SunColor;
            float4 _DuneFar;
            float4 _DuneNear;
            float4 _RobeColor;
            float4 _ScarfColor;
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

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float Fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                [unroll]
                for (int i = 0; i < 5; i++)
                {
                    value += Noise(p) * amplitude;
                    p = float2(p.x * 1.82 - p.y * 0.58, p.x * 0.58 + p.y * 1.82) + float2(17.1, 9.2);
                    amplitude *= 0.5;
                }
                return value;
            }

            float SdCapsule(float2 p, float2 a, float2 b, float r)
            {
                float2 pa = p - a;
                float2 ba = b - a;
                float h = saturate(dot(pa, ba) / max(dot(ba, ba), 0.0001));
                return length(pa - ba * h) - r;
            }

            float SdBox(float2 p, float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            float DuneHeight(float x, float layer, float t)
            {
                float drift = t * (0.02 + layer * 0.01);
                float wave = sin(x * (1.2 + layer * 0.55) + layer * 1.7 + drift) * (0.025 + layer * 0.012);
                float broad = sin(x * (0.42 + layer * 0.11) - layer * 2.1) * (0.075 + layer * 0.018);
                float grain = (Fbm(float2(x * 0.7 + layer * 10.0 + drift, layer * 2.0)) - 0.5) * 0.055;
                return wave + broad + grain;
            }

            float SandRidge(float2 uv, float aspect, float t)
            {
                float2 p = uv - 0.5;
                p.x *= aspect;
                float perspective = saturate(1.0 - uv.y * 1.55);
                float n = Fbm(float2(p.x * 6.0 + t * 0.05, uv.y * 18.0));
                float lines = sin((uv.y * 220.0 + p.x * 24.0 / max(uv.y + 0.13, 0.05)) + n * 5.0);
                float fine = sin((uv.y * 440.0 - p.x * 42.0) + n * 9.0);
                return (lines * 0.5 + 0.5) * perspective + (fine * 0.5 + 0.5) * perspective * 0.35;
            }

            float PyramidMask(float2 uv, float aspect)
            {
                float baseY = 0.44;
                float topY = 0.735;
                float localY = saturate((uv.y - baseY) / (topY - baseY));
                float width = (1.0 - localY) * 0.18;
                float x = abs((uv.x - 0.52) * aspect);
                float insideX = smoothstep(width + 0.006, width - 0.006, x);
                float insideY = smoothstep(baseY - 0.006, baseY + 0.006, uv.y) * smoothstep(topY + 0.006, topY - 0.006, uv.y);
                return insideX * insideY;
            }

            float LightBeamMask(float2 uv, float aspect)
            {
                float2 p = uv - float2(0.52, 0.51);
                p.x *= aspect;
                float cone = smoothstep(0.34, 0.0, abs(p.x) - (uv.y - 0.28) * 0.16);
                float vertical = smoothstep(0.20, 0.58, uv.y) * smoothstep(0.84, 0.48, uv.y);
                float core = exp(-abs(p.x) * 11.0) * smoothstep(0.30, 0.66, uv.y);
                return saturate(cone * vertical * 0.65 + core * 0.6);
            }

            float TempleRuinsMask(float2 uv, float aspect)
            {
                float2 p = uv;
                p.x = (p.x - 0.5) * aspect;
                float ruins = 0.0;

                [unroll]
                for (int i = 0; i < 9; i++)
                {
                    float fi = (float)i;
                    float side = fi < 5.0 ? -1.0 : 1.0;
                    float lane = fi < 5.0 ? fi : fi - 5.0;
                    float x = side * (0.23 + lane * 0.075);
                    float y = 0.39 + Hash21(float2(fi, 2.7)) * 0.035;
                    float h = 0.035 + Hash21(float2(fi, 8.1)) * 0.05;
                    float w = 0.009 + Hash21(float2(fi, 4.9)) * 0.008;
                    float2 q = p - float2(x, y + h * 0.5);
                    ruins += smoothstep(0.01, 0.0, SdBox(q, float2(w, h)));
                    ruins += smoothstep(0.011, 0.0, SdCapsule(p, float2(x - w * 1.3, y + h), float2(x + w * 1.3, y + h), 0.004));
                }

                float fade = smoothstep(0.52, 0.35, uv.y);
                return saturate(ruins * fade);
            }

            float FlyingClothMask(float2 uv, float aspect, float t, out float spark)
            {
                float2 p = uv - float2(0.5, 0.49);
                p.x *= aspect;
                float cloth = 0.0;
                spark = 0.0;

                [unroll]
                for (int i = 0; i < 9; i++)
                {
                    float fi = (float)i;
                    float seed = Hash21(float2(fi, 13.1));
                    float side = fi < 5.0 ? -1.0 : 1.0;
                    float lane = fi < 5.0 ? fi : fi - 5.0;
                    float2 pos = float2(side * (0.18 + lane * 0.08 + sin(t * 0.45 + fi) * 0.02), 0.08 + sin(t * 0.35 + fi * 1.7) * 0.055 + seed * 0.04);
                    float2 q = p - pos;
                    float angle = side * 0.45 + sin(t + fi) * 0.25;
                    float ca = cos(angle);
                    float sa = sin(angle);
                    q = float2(ca * q.x - sa * q.y, sa * q.x + ca * q.y);
                    float body = smoothstep(0.006, 0.0, SdBox(q, float2(0.024 + seed * 0.011, 0.008 + seed * 0.006)));
                    float nose = smoothstep(0.011, 0.0, SdCapsule(q, float2(0.012, 0.0), float2(0.052 + seed * 0.02, 0.004), 0.006));
                    float tail = smoothstep(0.010, 0.0, SdCapsule(q, float2(-0.02, 0.0), float2(-0.075 - seed * 0.03, sin(t + fi) * 0.018), 0.004));
                    float wing = smoothstep(0.008, 0.0, SdCapsule(q, float2(-0.012, 0.004), float2(0.03, 0.022 + seed * 0.018), 0.004));
                    cloth += body + nose + tail + wing;
                    spark += smoothstep(0.01, 0.0, length(q - float2(0.045, 0.0)) - 0.005);
                }

                return saturate(cloth);
            }

            float ForegroundTravelerMask(float2 uv, float aspect, float t, out float scarfMask, out float goldTrim)
            {
                float2 center = float2(0.515, 0.205);
                float2 p = uv - center;
                p.x *= aspect;
                p /= max(_TravelerScale, 0.001);

                float bodyTop = 0.19;
                float bodyBottom = -0.18;
                float bodyY = saturate((bodyTop - p.y) / (bodyTop - bodyBottom));
                float width = lerp(0.045, 0.155, bodyY);
                float cloak = smoothstep(0.012, -0.004, abs(p.x) - width) *
                              smoothstep(bodyBottom - 0.018, bodyBottom + 0.015, p.y) *
                              smoothstep(bodyTop + 0.015, bodyTop - 0.005, p.y);

                float hoodSlope = abs(p.x) - max(0.01, (0.255 - p.y) * 0.58);
                float hood = smoothstep(0.016, -0.004, hoodSlope) *
                             smoothstep(0.145, 0.170, p.y) *
                             smoothstep(0.265, 0.242, p.y);
                hood += smoothstep(0.018, 0.0, SdCapsule(p, float2(-0.045, 0.172), float2(0.045, 0.172), 0.025));
                float faceDark = smoothstep(0.014, 0.0, SdBox(p - float2(0.0, 0.177), float2(0.024, 0.012)));
                float legs = smoothstep(0.014, 0.0, SdCapsule(p, float2(-0.045, -0.16), float2(-0.065, -0.225), 0.012));
                legs += smoothstep(0.014, 0.0, SdCapsule(p, float2(0.045, -0.16), float2(0.072, -0.225), 0.012));

                float flutter = sin(t * 1.8 + p.x * 16.0) * 0.02;
                scarfMask = smoothstep(0.018, 0.0, SdCapsule(p, float2(0.035, 0.145), float2(0.23, 0.16 + flutter), 0.018));
                scarfMask += smoothstep(0.018, 0.0, SdCapsule(p, float2(0.22, 0.16 + flutter), float2(0.37, 0.12 - flutter), 0.012));
                scarfMask += smoothstep(0.018, 0.0, length(p - float2(0.38, 0.12 - flutter)) - 0.018);

                float trimLine = abs(abs(p.x) - width * 0.82) - 0.006;
                goldTrim = smoothstep(0.008, 0.0, trimLine) * cloak;
                goldTrim += smoothstep(0.013, 0.0, SdCapsule(p, float2(-0.09, 0.04), float2(0.09, -0.11), 0.004)) * cloak;
                goldTrim = saturate(goldTrim);

                return saturate(cloak + hood + legs - faceDark * 0.45);
            }

            float ParticleField(float2 uv, float aspect, float t)
            {
                float2 p = uv;
                p.x *= aspect;
                float field = 0.0;

                [unroll]
                for (int i = 0; i < 36; i++)
                {
                    float fi = (float)i;
                    float seed = Hash21(float2(fi, fi * 3.17));
                    float2 pos = float2(0.12 + Hash21(float2(fi, 1.0)) * 0.76, 0.24 + Hash21(float2(fi, 7.0)) * 0.48);
                    pos.x += sin(t * (0.22 + seed * 0.28) + fi) * 0.07;
                    pos.y += frac(t * (0.035 + seed * 0.025) + seed) * 0.22 - 0.10;
                    float2 q = p - float2(pos.x * aspect, pos.y);
                    float angle = seed * 6.28;
                    float ca = cos(angle);
                    float sa = sin(angle);
                    q = float2(ca * q.x - sa * q.y, sa * q.x + ca * q.y);
                    float diamond = abs(q.x) + abs(q.y) - (0.004 + seed * 0.006);
                    field += smoothstep(0.006, 0.0, diamond);
                }

                return saturate(field * _ParticleAmount);
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float t = _Time.y * _TimeScale;
                float2 centered = uv - 0.5;
                centered.x *= aspect;

                float horizon = 0.39;
                float skyT = saturate((uv.y - horizon) / max(1.0 - horizon, 0.001));
                float3 col = lerp(_SkyHorizon.rgb, _SkyTop.rgb, skyT);

                float2 sunPos = float2(0.52, 0.61);
                float sunDist = length((uv - sunPos) * float2(aspect, 1.0));
                float sunDisk = smoothstep(0.088, 0.052, sunDist);
                float sunGlow = exp(-sunDist * 3.8) * _SunGlow;
                col += _SunColor.rgb * (sunDisk * 1.25 + sunGlow * 0.42);

                float beam = LightBeamMask(uv, aspect);
                col += _SunColor.rgb * beam * 0.62;

                float cloudNoise = Fbm(float2(centered.x * 1.35 + t * 0.016, uv.y * 2.25 - t * 0.01));
                float cloudBand = smoothstep(0.43, 0.78, cloudNoise + uv.y * 0.12) * smoothstep(0.43, 0.60, uv.y) * smoothstep(1.0, 0.70, uv.y);
                float3 cloudColor = float3(1.0, 0.64, 0.50);
                col = lerp(col, col + cloudColor * 0.36, cloudBand * _CloudAmount * 0.38);

                float pyramid = PyramidMask(uv, aspect);
                float pyramidFacet = smoothstep(0.0, 0.17, (uv.x - 0.52) * aspect);
                float3 pyramidColor = lerp(float3(0.78, 0.26, 0.18), float3(1.0, 0.63, 0.18), pyramidFacet);
                pyramidColor += _SunColor.rgb * (beam * 0.8 + sunGlow * 0.2);
                col = lerp(col, pyramidColor, pyramid * 0.78);
                col += _SunColor.rgb * pyramid * smoothstep(0.2, 0.0, abs((uv.x - 0.52) * aspect)) * 0.25;

                float haze = exp(-abs(uv.y - horizon) * 8.0);
                col = lerp(col, _SunColor.rgb, haze * 0.18);

                float x = centered.x;
                float farH = 0.37 + DuneHeight(x * 1.65, 0.0, t);
                float midH = 0.28 + DuneHeight(x * 2.25 + 2.0, 1.0, t);
                float nearH = 0.12 + DuneHeight(x * 3.0 - 1.5, 2.0, t);

                float farMask = smoothstep(farH + 0.012, farH - 0.012, uv.y);
                float midMask = smoothstep(midH + 0.014, midH - 0.014, uv.y);
                float nearMask = smoothstep(nearH + 0.018, nearH - 0.018, uv.y);

                float3 farDune = lerp(_DuneFar.rgb * 0.75, _DuneNear.rgb * 0.70, uv.y + 0.2);
                float3 midDune = lerp(_DuneFar.rgb * 0.95, _DuneNear.rgb * 0.92, uv.y + 0.1);
                float ridge = SandRidge(uv, aspect, t);
                float3 nearDune = _DuneNear.rgb * (0.56 + uv.y * 0.95) + float3(0.18, 0.055, 0.01) * ridge * _DuneContrast;

                col = lerp(col, farDune + _SunColor.rgb * beam * 0.22, farMask * 0.90);
                col = lerp(col, midDune + _SunColor.rgb * beam * 0.12, midMask * 0.96);
                col = lerp(col, nearDune, nearMask);

                float ruins = TempleRuinsMask(uv, aspect);
                col = lerp(col, float3(0.42, 0.15, 0.11), ruins * smoothstep(0.55, 0.30, uv.y));

                float clothSpark;
                float cloth = FlyingClothMask(uv, aspect, t, clothSpark);
                col = lerp(col, lerp(_RobeColor.rgb * 1.25, _ScarfColor.rgb, 0.42), cloth);
                col += _SunColor.rgb * clothSpark * 0.8;

                float scarfMask;
                float goldTrim;
                float traveler = ForegroundTravelerMask(uv, aspect, t, scarfMask, goldTrim);
                float shadow = smoothstep(0.38, 0.0, length(float2((uv.x - 0.515) * aspect * 1.8, uv.y - 0.045))) * nearMask;
                col = lerp(col, col * 0.55, shadow * 0.28);
                col = lerp(col, _RobeColor.rgb, traveler);
                col = lerp(col, _ScarfColor.rgb, saturate(scarfMask));
                col += _SunColor.rgb * goldTrim * 0.75;

                float particles = ParticleField(uv, aspect, t);
                float3 particleColor = lerp(_ScarfColor.rgb, _SunColor.rgb, Hash21(floor(uv * 90.0)));
                col += particleColor * particles * 0.9;

                float bloom = sunGlow * 0.08 + beam * 0.08 + particles * 0.06;
                col += _SunColor.rgb * bloom;

                float vignette = smoothstep(1.16, 0.18, length(centered));
                col *= lerp(0.74, 1.0, vignette);
                col = pow(saturate(col), 1.0 / 2.2);
                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
