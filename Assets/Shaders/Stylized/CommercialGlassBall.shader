Shader "Codex/Stylized/Commercial Glass Ball"
{
    Properties
    {
        _BaseColor ("Glass Tint", Color) = (0.55, 0.85, 1.0, 0.32)
        _EdgeColor ("Fresnel Edge Color", Color) = (0.75, 0.95, 1.0, 1.0)
        _ReflectionColor ("Reflection Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        _GlassSpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _NormalMap ("Micro Distortion Normal", 2D) = "bump" {}
        _CubeMap ("Built-in Optional Cubemap", Cube) = "_Skybox" {}

        _NormalScale ("Normal Scale", Range(0, 2)) = 0.55
        _RefractionStrength ("Screen Refraction", Range(0, 0.15)) = 0.045
        _LensStrength ("Spherical Lens", Range(0, 0.2)) = 0.075
        _DistortionStrength ("Micro Distortion", Range(0, 0.06)) = 0.018
        _FresnelPower ("Fresnel Power", Range(1, 8)) = 3.2
        _FresnelIntensity ("Fresnel Intensity", Range(0, 3)) = 1.15
        _ReflectionIntensity ("Reflection Intensity", Range(0, 3)) = 0.9
        _SpecularIntensity ("Specular Intensity", Range(0, 5)) = 1.7
        _SpecularPower ("Specular Power", Range(8, 256)) = 96
        _Opacity ("Base Opacity", Range(0, 1)) = 0.35
        _RimAlphaBoost ("Rim Alpha Boost", Range(0, 1)) = 0.42
        _Absorption ("Tint Absorption", Range(0, 2)) = 0.55
        _CenterDarkness ("Center Density", Range(0, 1)) = 0.18
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+20"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        GrabPass { "_GlassBallGrabTexture" }

        Pass
        {
            Name "BuiltInGlassBall"
            Tags { "LightMode"="ForwardBase" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            fixed4 _BaseColor;
            fixed4 _EdgeColor;
            fixed4 _ReflectionColor;
            fixed4 _GlassSpecColor;
            sampler2D _NormalMap;
            float4 _NormalMap_ST;
            samplerCUBE _CubeMap;
            sampler2D _GlassBallGrabTexture;

            half _NormalScale;
            half _RefractionStrength;
            half _LensStrength;
            half _DistortionStrength;
            half _FresnelPower;
            half _FresnelIntensity;
            half _ReflectionIntensity;
            half _SpecularIntensity;
            half _SpecularPower;
            half _Opacity;
            half _RimAlphaBoost;
            half _Absorption;
            half _CenterDarkness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float4 tangentWS : TEXCOORD4;
                float3 positionOS : TEXCOORD5;
                UNITY_FOG_COORDS(6)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv = TRANSFORM_TEX(v.uv, _NormalMap);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                float3 tangentWS = UnityObjectToWorldDir(v.tangent.xyz);
                o.tangentWS = float4(tangentWS, v.tangent.w);
                o.positionOS = v.vertex.xyz;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half3 baseNormal = normalize(i.normalWS);
                half3 tangentWS = normalize(i.tangentWS.xyz);
                half3 bitangentWS = normalize(cross(baseNormal, tangentWS) * i.tangentWS.w);
                half3 normalTS = UnpackNormal(tex2D(_NormalMap, i.uv));
                normalTS.xy *= _NormalScale;
                normalTS.z = sqrt(saturate(1.0h - dot(normalTS.xy, normalTS.xy)));
                half3 normalWS = normalize(tangentWS * normalTS.x + bitangentWS * normalTS.y + baseNormal * normalTS.z);

                half3 viewDirWS = normalize(UnityWorldSpaceViewDir(i.worldPos));
                half noV = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(saturate(1.0h - noV), _FresnelPower);

                float2 screenUV = i.grabPos.xy / max(i.grabPos.w, 1e-5);
                float4 centerGrabPos = ComputeGrabScreenPos(UnityObjectToClipPos(float4(0, 0, 0, 1)));
                float2 centerUV = centerGrabPos.xy / max(centerGrabPos.w, 1e-5);
                float2 radial = screenUV - centerUV;
                float radialLen = max(length(radial), 1e-4);
                float2 radialDir = radial / radialLen;

                half3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, normalWS));
                float2 refractionOffset = normalVS.xy * _RefractionStrength * (0.25h + fresnel * 1.5h);
                float2 lensOffset = radialDir * _LensStrength * (0.15h + fresnel * 0.85h);
                float2 microOffset = normalTS.xy * _DistortionStrength * (0.35h + fresnel);
                float2 refractUV = saturate(screenUV + refractionOffset + lensOffset + microOffset);

                fixed3 sceneColor = tex2D(_GlassBallGrabTexture, refractUV).rgb;
                half absorption = saturate(_Absorption * (0.2h + (1.0h - noV) * 0.8h));
                half3 refracted = lerp(sceneColor, sceneColor * _BaseColor.rgb, absorption);
                refracted *= 1.0h - _CenterDarkness * saturate(noV * 0.75h);

                half3 lightDirWS = normalize(_WorldSpaceLightPos0.xyz);
                half3 halfDir = normalize(lightDirWS + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), _SpecularPower) * _SpecularIntensity;

                half3 reflDir = reflect(-viewDirWS, normalWS);
                half4 encodedReflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflDir);
                half3 reflection = DecodeHDR(encodedReflection, unity_SpecCube0_HDR) * _ReflectionColor.rgb * _ReflectionIntensity;
                reflection += texCUBE(_CubeMap, reflDir).rgb * _ReflectionColor.rgb * (_ReflectionIntensity * 0.35h);
                half3 rim = _EdgeColor.rgb * fresnel * _FresnelIntensity;

                half3 color = refracted + reflection * (0.12h + fresnel) + rim + _GlassSpecColor.rgb * spec;
                UNITY_APPLY_FOG(i.fogCoord, color);

                half alpha = saturate(_Opacity + fresnel * _RimAlphaBoost + spec * 0.12h);
                return fixed4(color, alpha);
            }
            ENDCG
        }
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent+20"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "URPGlassBall"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EdgeColor;
                half4 _ReflectionColor;
                half4 _GlassSpecColor;
                float4 _NormalMap_ST;
                half _NormalScale;
                half _RefractionStrength;
                half _LensStrength;
                half _DistortionStrength;
                half _FresnelPower;
                half _FresnelIntensity;
                half _ReflectionIntensity;
                half _SpecularIntensity;
                half _SpecularPower;
                half _Opacity;
                half _RimAlphaBoost;
                half _Absorption;
                half _CenterDarkness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float3 positionOS : TEXCOORD5;
                half fogFactor : TEXCOORD6;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = half4(normalInputs.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.uv = TRANSFORM_TEX(input.uv, _NormalMap);
                output.screenPos = ComputeScreenPos(posInputs.positionCS);
                output.positionOS = input.positionOS.xyz;
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 baseNormal = normalize(input.normalWS);
                half3 tangentWS = normalize(input.tangentWS.xyz);
                half3 bitangentWS = normalize(cross(baseNormal, tangentWS) * input.tangentWS.w);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv), _NormalScale);
                half3 normalWS = normalize(tangentWS * normalTS.x + bitangentWS * normalTS.y + baseNormal * normalTS.z);

                half3 viewDirWS = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                half noV = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(saturate(1.0h - noV), _FresnelPower);

                float2 screenUV = input.screenPos.xy / max(input.screenPos.w, 1e-5);
                float4 centerClip = TransformObjectToHClip(float3(0, 0, 0));
                float4 centerScreen = ComputeScreenPos(centerClip);
                float2 centerUV = centerScreen.xy / max(centerScreen.w, 1e-5);
                float2 radial = screenUV - centerUV;
                float radialLen = max(length(radial), 1e-4);
                float2 radialDir = radial / radialLen;

                half3 normalVS = normalize(TransformWorldToViewDir(normalWS, true));
                float2 refractionOffset = normalVS.xy * _RefractionStrength * (0.25h + fresnel * 1.5h);
                float2 lensOffset = radialDir * _LensStrength * (0.15h + fresnel * 0.85h);
                float2 microOffset = normalTS.xy * _DistortionStrength * (0.35h + fresnel);
                float2 refractUV = saturate(screenUV + refractionOffset + lensOffset + microOffset);

                half3 sceneColor = SampleSceneColor(refractUV);
                half absorption = saturate(_Absorption * (0.2h + (1.0h - noV) * 0.8h));
                half3 refracted = lerp(sceneColor, sceneColor * _BaseColor.rgb, absorption);
                refracted *= 1.0h - _CenterDarkness * saturate(noV * 0.75h);

                Light mainLight = GetMainLight();
                half3 lightDirWS = normalize(mainLight.direction);
                half3 halfDir = normalize(lightDirWS + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), _SpecularPower) * _SpecularIntensity;

                half3 reflDir = reflect(-viewDirWS, normalWS);
                half3 reflection = GlossyEnvironmentReflection(reflDir, input.positionWS, 0.02h, 1.0h) * _ReflectionColor.rgb * _ReflectionIntensity;
                half3 rim = _EdgeColor.rgb * fresnel * _FresnelIntensity;

                half3 color = refracted + reflection * (0.12h + fresnel) + rim + _GlassSpecColor.rgb * spec * mainLight.color;
                color = MixFog(color, input.fogFactor);

                half alpha = saturate(_Opacity + fresnel * _RimAlphaBoost + spec * 0.12h);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
