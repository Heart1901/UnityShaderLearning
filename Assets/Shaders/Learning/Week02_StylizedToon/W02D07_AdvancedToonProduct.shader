Shader "Learning/Week02/07 Advanced Toon Product"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _RampMap ("Ramp Map", 2D) = "white" {}
        _MatCapMap ("MatCap Map", 2D) = "gray" {}
        _PatternMap ("Shadow Pattern", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.95, 0.68, 0.34, 1.0)
        _ShadowColor ("Shadow Color", Color) = (0.12, 0.15, 0.32, 1.0)
        _RimColor ("Rim Color", Color) = (0.3, 0.9, 1.0, 1.0)
        _ToonSpecColor ("Specular Color", Color) = (1.0, 0.94, 0.65, 1.0)
        _OutlineColor ("Outline Color", Color) = (0.025, 0.03, 0.055, 1.0)
        _OutlineWidth ("Outline Width", Range(0, 0.08)) = 0.026
        _BandOffset ("Band Offset", Range(-0.5, 0.5)) = 0
        _BandContrast ("Band Contrast", Range(0.25, 2)) = 1.0
        _RimPower ("Rim Power", Range(0.5, 8)) = 3.0
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.55
        _SpecThreshold ("Spec Threshold", Range(0, 1)) = 0.72
        _SpecSoftness ("Spec Softness", Range(0.001, 0.2)) = 0.035
        _SpecPower ("Spec Power", Range(8, 256)) = 96
        _SpecStrength ("Spec Strength", Range(0, 2)) = 0.75
        _MatCapStrength ("MatCap Strength", Range(0, 2)) = 0.35
        _PatternScale ("Pattern Scale", Range(0.25, 8)) = 2.4
        _PatternStrength ("Pattern Strength", Range(0, 1)) = 0.22
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
            Name "AdvancedToonBuiltin"
            Tags { "LightMode"="ForwardBase" }
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _BaseMap;
            sampler2D _RampMap;
            sampler2D _MatCapMap;
            sampler2D _PatternMap;
            float4 _BaseMap_ST;
            fixed4 _BaseColor;
            fixed4 _ShadowColor;
            fixed4 _RimColor;
            fixed4 _ToonSpecColor;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            half _BandOffset;
            half _BandContrast;
            half _RimPower;
            half _RimStrength;
            half _SpecThreshold;
            half _SpecSoftness;
            half _SpecPower;
            half _SpecStrength;
            half _MatCapStrength;
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
                float3 positionWS : TEXCOORD0;
                fixed3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 normalWS = normalize(i.normalWS);
                fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.positionWS));
                fixed3 halfDir = normalize(lightDir + viewDir);

                fixed ndotl = saturate(dot(normalWS, lightDir));
                fixed rampU = saturate((ndotl + _BandOffset) * _BandContrast);
                fixed3 ramp = tex2D(_RampMap, float2(rampU, 0.5)).rgb;

                fixed3 albedo = tex2D(_BaseMap, i.uv).rgb * _BaseColor.rgb;
                fixed shadowMask = saturate(1.0 - rampU);
                fixed pattern = tex2D(_PatternMap, i.uv * _PatternScale).r;
                fixed3 shadowTint = lerp(_ShadowColor.rgb, _ShadowColor.rgb * (0.55 + pattern), _PatternStrength);

                half specRaw = pow(saturate(dot(normalWS, halfDir)), _SpecPower);
                half specBand = smoothstep(_SpecThreshold, _SpecThreshold + _SpecSoftness, specRaw) * _SpecStrength;
                half rim = pow(saturate(1.0 - dot(normalWS, viewDir)), _RimPower) * _RimStrength;

                fixed3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, normalWS));
                float2 matcapUV = normalVS.xy * 0.5 + 0.5;
                fixed3 matcap = tex2D(_MatCapMap, matcapUV).rgb * _MatCapStrength;

                fixed3 toonBase = albedo * ramp * _LightColor0.rgb;
                toonBase = lerp(toonBase, albedo * shadowTint, shadowMask * 0.65);
                fixed3 color = toonBase + _ToonSpecColor.rgb * specBand + _RimColor.rgb * rim + matcap;
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
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _RimColor;
                half4 _ToonSpecColor;
                half4 _OutlineColor;
                float _OutlineWidth;
                half _BandOffset;
                half _BandContrast;
                half _RimPower;
                half _RimStrength;
                half _SpecThreshold;
                half _SpecSoftness;
                half _SpecPower;
                half _SpecStrength;
                half _MatCapStrength;
                half _PatternScale;
                half _PatternStrength;
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
            Name "AdvancedToon"
            Tags { "LightMode"="UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_RampMap);
            SAMPLER(sampler_RampMap);
            TEXTURE2D(_MatCapMap);
            SAMPLER(sampler_MatCapMap);
            TEXTURE2D(_PatternMap);
            SAMPLER(sampler_PatternMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _RimColor;
                half4 _ToonSpecColor;
                half4 _OutlineColor;
                float _OutlineWidth;
                half _BandOffset;
                half _BandContrast;
                half _RimPower;
                half _RimStrength;
                half _SpecThreshold;
                half _SpecSoftness;
                half _SpecPower;
                half _SpecStrength;
                half _MatCapStrength;
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
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 halfDirWS = normalize(mainLight.direction + viewDirWS);

                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half rampU = saturate((ndotl + _BandOffset) * _BandContrast);
                half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(rampU, 0.5h)).rgb;

                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half shadowMask = saturate(1.0h - rampU);
                half pattern = SAMPLE_TEXTURE2D(_PatternMap, sampler_PatternMap, input.uv * _PatternScale).r;
                half3 shadowTint = lerp(_ShadowColor.rgb, _ShadowColor.rgb * (0.55h + pattern), _PatternStrength);

                half specRaw = pow(saturate(dot(normalWS, halfDirWS)), _SpecPower);
                half specBand = smoothstep(_SpecThreshold, _SpecThreshold + _SpecSoftness, specRaw) * _SpecStrength;

                half rim = pow(saturate(1.0h - dot(normalWS, viewDirWS)), _RimPower) * _RimStrength;

                half3 normalVS = normalize(mul((float3x3)GetWorldToViewMatrix(), normalWS));
                float2 matcapUV = normalVS.xy * 0.5h + 0.5h;
                half3 matcap = SAMPLE_TEXTURE2D(_MatCapMap, sampler_MatCapMap, matcapUV).rgb * _MatCapStrength;

                half3 toonBase = albedo * ramp * mainLight.color;
                toonBase = lerp(toonBase, albedo * shadowTint, shadowMask * 0.65h);
                half3 color = toonBase + _ToonSpecColor.rgb * specBand + _RimColor.rgb * rim + matcap;
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
