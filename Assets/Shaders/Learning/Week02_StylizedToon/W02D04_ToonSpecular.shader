Shader "Learning/Week02/04 Toon Specular"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.35, 0.72, 1.0, 1.0)
        _ShadowColor ("Shadow Color", Color) = (0.08, 0.16, 0.32, 1.0)
        _ToonSpecColor ("Specular Color", Color) = (1.0, 0.96, 0.78, 1.0)
        _BandCenter ("Band Center", Range(0, 1)) = 0.45
        _BandSoftness ("Band Softness", Range(0.001, 0.25)) = 0.04
        _SpecThreshold ("Spec Threshold", Range(0, 1)) = 0.72
        _SpecSoftness ("Spec Softness", Range(0.001, 0.2)) = 0.035
        _SpecPower ("Spec Power", Range(8, 256)) = 72
        _SpecStrength ("Spec Strength", Range(0, 2)) = 0.9
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "ToonSpecularBuiltin"
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            fixed4 _BaseColor;
            fixed4 _ShadowColor;
            fixed4 _ToonSpecColor;
            fixed _BandCenter;
            fixed _BandSoftness;
            half _SpecThreshold;
            half _SpecSoftness;
            half _SpecPower;
            half _SpecStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                fixed3 normalWS : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 normalWS = normalize(i.normalWS);
                fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.positionWS));
                fixed3 halfDir = normalize(lightDir + viewDir);
                fixed ndotl = saturate(dot(normalWS, lightDir));
                fixed band = smoothstep(_BandCenter - _BandSoftness, _BandCenter + _BandSoftness, ndotl);
                half specRaw = pow(saturate(dot(normalWS, halfDir)), _SpecPower);
                half specBand = smoothstep(_SpecThreshold, _SpecThreshold + _SpecSoftness, specRaw);
                fixed3 toon = lerp(_ShadowColor.rgb, _BaseColor.rgb * _LightColor0.rgb, band);
                return fixed4(toon + _ToonSpecColor.rgb * specBand * _SpecStrength, 1.0);
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
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _ToonSpecColor;
                half _BandCenter;
                half _BandSoftness;
                half _SpecThreshold;
                half _SpecSoftness;
                half _SpecPower;
                half _SpecStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 halfDirWS = normalize(mainLight.direction + viewDirWS);
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half band = smoothstep(_BandCenter - _BandSoftness, _BandCenter + _BandSoftness, ndotl);
                half specRaw = pow(saturate(dot(normalWS, halfDirWS)), _SpecPower);
                half specBand = smoothstep(_SpecThreshold, _SpecThreshold + _SpecSoftness, specRaw);
                half3 toon = lerp(_ShadowColor.rgb, _BaseColor.rgb * mainLight.color, band);
                return half4(toon + _ToonSpecColor.rgb * specBand * _SpecStrength, 1.0h);
            }
            ENDHLSL
        }
    }
}
