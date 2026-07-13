Shader "Learning/Week02/05 MatCap"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _MatCapMap ("MatCap Map", 2D) = "gray" {}
        _BaseColor ("Base Color", Color) = (0.86, 0.72, 0.52, 1.0)
        _MatCapStrength ("MatCap Strength", Range(0, 2)) = 0.7
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "MatCapBuiltin"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BaseMap;
            sampler2D _MatCapMap;
            float4 _BaseMap_ST;
            fixed4 _BaseColor;
            half _MatCapStrength;

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
                fixed3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, normalize(i.normalWS)));
                float2 matcapUV = normalVS.xy * 0.5 + 0.5;
                fixed3 albedo = tex2D(_BaseMap, i.uv).rgb * _BaseColor.rgb;
                fixed3 matcap = tex2D(_MatCapMap, matcapUV).rgb;
                return fixed4(albedo + matcap * _MatCapStrength, 1.0);
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

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MatCapMap);
            SAMPLER(sampler_MatCapMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _MatCapStrength;
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
                half3 normalVS = normalize(mul((float3x3)GetWorldToViewMatrix(), normalize(input.normalWS)));
                float2 matcapUV = normalVS.xy * 0.5h + 0.5h;
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half3 matcap = SAMPLE_TEXTURE2D(_MatCapMap, sampler_MatCapMap, matcapUV).rgb;
                half3 color = albedo + matcap * _MatCapStrength;
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
