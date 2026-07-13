Shader "Learning/Week02/02 Outline Pass"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.95, 0.66, 0.3, 1.0)
        _ShadowColor ("Shadow Color", Color) = (0.15, 0.18, 0.32, 1.0)
        _OutlineColor ("Outline Color", Color) = (0.025, 0.03, 0.055, 1.0)
        _BandCenter ("Band Center", Range(0, 1)) = 0.48
        _BandSoftness ("Band Softness", Range(0.001, 0.25)) = 0.035
        _OutlineWidth ("Outline Width", Range(0, 0.08)) = 0.025
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "OutlineBuiltin"
            Tags { "LightMode"="Always" }
            Cull Front
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _OutlineColor;
            float _OutlineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 positionOS = v.vertex.xyz + normalize(v.normal) * _OutlineWidth;
                o.pos = UnityObjectToClipPos(float4(positionOS, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        Pass
        {
            Name "ToonBodyBuiltin"
            Tags { "LightMode"="ForwardBase" }
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            fixed4 _BaseColor;
            fixed4 _ShadowColor;
            fixed _BandCenter;
            fixed _BandSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed3 normalWS : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 normalWS = normalize(i.normalWS);
                fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                fixed ndotl = saturate(dot(normalWS, lightDir));
                fixed band = smoothstep(_BandCenter - _BandSoftness, _BandCenter + _BandSoftness, ndotl);
                fixed3 color = lerp(_ShadowColor.rgb, _BaseColor.rgb * _LightColor0.rgb, band);
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
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _OutlineColor;
                half _BandCenter;
                half _BandSoftness;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionOS = input.positionOS.xyz + normalize(input.normalOS) * _OutlineWidth;
                output.positionCS = TransformObjectToHClip(positionOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ToonBody"
            Tags { "LightMode"="UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _OutlineColor;
                half _BandCenter;
                half _BandSoftness;
                float _OutlineWidth;
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
                half3 color = lerp(_ShadowColor.rgb, _BaseColor.rgb * mainLight.color, band);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
