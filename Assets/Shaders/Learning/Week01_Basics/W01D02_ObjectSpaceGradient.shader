Shader "Learning/Week01/02 Object Space Gradient"
{
    Properties
    {
        _BottomColor ("Bottom Color", Color) = (0.1, 0.25, 0.95, 1.0)
        _TopColor ("Top Color", Color) = (1.0, 0.72, 0.2, 1.0)
        _HeightMin ("Height Min", Float) = -1
        _HeightMax ("Height Max", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _BottomColor;
            fixed4 _TopColor;
            float _HeightMin;
            float _HeightMax;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float height01 : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.height01 = saturate((v.vertex.y - _HeightMin) / max(0.0001, _HeightMax - _HeightMin));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return lerp(_BottomColor, _TopColor, i.height01);
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

            CBUFFER_START(UnityPerMaterial)
                half4 _BottomColor;
                half4 _TopColor;
                float _HeightMin;
                float _HeightMax;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float height01 : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.height01 = saturate((input.positionOS.y - _HeightMin) / max(0.0001, _HeightMax - _HeightMin));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return lerp(_BottomColor, _TopColor, input.height01);
            }
            ENDHLSL
        }
    }
}
