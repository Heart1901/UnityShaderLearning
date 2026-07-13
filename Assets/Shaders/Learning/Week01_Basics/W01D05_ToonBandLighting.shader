Shader "Learning/Week01/05 Toon Band Lighting"
{
    Properties
    {
        _LitColor ("Lit Color", Color) = (1.0, 0.72, 0.25, 1.0)
        _ShadowColor ("Shadow Color", Color) = (0.12, 0.18, 0.35, 1.0)
        _BandCenter ("Band Center", Range(0, 1)) = 0.45
        _BandSoftness ("Band Softness", Range(0.001, 0.25)) = 0.04
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            fixed4 _LitColor;
            fixed4 _ShadowColor;
            half _BandCenter;
            half _BandSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half ndotl = saturate(dot(normalize(i.normalWS), normalize(_WorldSpaceLightPos0.xyz)));
                half band = smoothstep(_BandCenter - _BandSoftness, _BandCenter + _BandSoftness, ndotl);
                fixed3 color = lerp(_ShadowColor.rgb, _LitColor.rgb * _LightColor0.rgb, band);
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

            CBUFFER_START(UnityPerMaterial)
                half4 _LitColor;
                half4 _ShadowColor;
                half _BandCenter;
                half _BandSoftness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(normalize(input.normalWS), mainLight.direction));
                half band = smoothstep(_BandCenter - _BandSoftness, _BandCenter + _BandSoftness, ndotl);
                half3 color = lerp(_ShadowColor.rgb, _LitColor.rgb * mainLight.color, band);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
