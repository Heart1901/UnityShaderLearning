Shader "Custom/TransparentSphere"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 1, 1, 0.5)
        _Transparency ("Transparency", Range(0, 1)) = 0.5
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0, 10)) = 2.0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // 透明混合模式

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            fixed4 _MainColor;
            fixed _Transparency;
            fixed4 _RimColor;
            fixed _RimPower;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag(v2f o) : SV_TARGET
            {
                half rim = 1.0 - saturate(dot(normalize(o.viewDir), normalize(o.normal)));
                rim = pow(rim, _RimPower);
                fixed4 rimColor = _RimColor * rim;

                fixed4 finalColor = _MainColor;
                finalColor.rgb += rimColor.rgb;
                finalColor.a = _Transparency;

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}