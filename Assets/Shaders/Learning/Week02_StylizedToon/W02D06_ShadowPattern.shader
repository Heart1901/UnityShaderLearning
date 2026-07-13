Shader "Learning/Week02/06 Shadow Pattern"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _PatternMap ("Shadow Pattern", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.92, 0.62, 0.38, 1.0)
        _ShadowColor ("Shadow Color", Color) = (0.12, 0.16, 0.32, 1.0)
        _PatternColor ("Pattern Color", Color) = (0.04, 0.06, 0.1, 1.0)
        _BandCenter ("Band Center", Range(0, 1)) = 0.48
        _BandSoftness ("Band Softness", Range(0.001, 0.25)) = 0.04
        _PatternScale ("Pattern Scale", Range(0.25, 8)) = 2.5
        _PatternStrength ("Pattern Strength", Range(0, 1)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "ShadowPatternBuiltin"
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _BaseMap;
            sampler2D _PatternMap;
            float4 _BaseMap_ST;
            fixed4 _BaseColor;
            fixed4 _ShadowColor;
            fixed4 _PatternColor;
            fixed _BandCenter;
            fixed _BandSoftness;
            half _PatternScale;
            half _PatternStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 normalWS = normalize(i.normalWS);
                fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                fixed ndotl = saturate(dot(normalWS, lightDir));
                fixed litBand = smoothstep(_BandCenter - _BandSoftness, _BandCenter + _BandSoftness, ndotl);
                fixed shadowMask = 1.0 - litBand;
                fixed3 albedo = tex2D(_BaseMap, i.uv).rgb * _BaseColor.rgb;
                fixed pattern = tex2D(_PatternMap, i.uv * _PatternScale).r;
                fixed3 shadow = lerp(_ShadowColor.rgb, _PatternColor.rgb, pattern * _PatternStrength);
                fixed3 color = lerp(shadow * albedo, albedo * _LightColor0.rgb, litBand);
                color = lerp(color, shadow, shadowMask * pattern * _PatternStrength * 0.35);
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_PatternMap);
            SAMPLER(sampler_PatternMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _PatternColor;
                half _BandCenter;
                half _BandSoftness;
                half _PatternScale;
                half _PatternStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(normalize(input.normalWS), mainLight.direction));
                half litBand = smoothstep(_BandCenter - _BandSoftness, _BandCenter + _BandSoftness, ndotl);
                half shadowMask = 1.0h - litBand;
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half pattern = SAMPLE_TEXTURE2D(_PatternMap, sampler_PatternMap, input.uv * _PatternScale).r;
                half3 shadow = lerp(_ShadowColor.rgb, _PatternColor.rgb, pattern * _PatternStrength);
                half3 color = lerp(shadow * albedo, albedo * mainLight.color, litBand);
                color = lerp(color, shadow, shadowMask * pattern * _PatternStrength * 0.35h);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
