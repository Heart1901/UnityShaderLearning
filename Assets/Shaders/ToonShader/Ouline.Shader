Shader "Unlit/Ouline"
{
    Properties
    {
	_OutlineWidth ("Outline Width", Range(0.01, 1)) = 0.24
        _OutLineColor ("OutLine Color", Color) = (0.5,0.5,0.5,1)

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        pass
        {
           Tags {"LightMode"="ForwardBase"}
			 
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 vert(appdata_base v): SV_POSITION
	    {
                return UnityObjectToClipPos(v.vertex);
            }

            half4 frag() : SV_TARGET 
	   {
                return half4(1,1,1,1);
            }

            ENDCG
        }

        Pass
	{
	    Tags {"LightMode"="ForwardBase"}
			 
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            half _OutlineWidth;
            half4 _OutLineColor;

            struct a2v 
	    {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 vertColor : COLOR;
                float4 tangent : TANGENT;
            };

            struct v2f
	   {
                float4 pos : SV_POSITION;
            };


            v2f vert (a2v v) 
	   {
                v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal * _OutlineWidth * 0.1 ,1));//顶点沿着法线方向外扩
                return o;
            }

            half4 frag(v2f i) : SV_TARGET 
	    {
                return _OutLineColor;
            }
            ENDCG
        }
    }
}