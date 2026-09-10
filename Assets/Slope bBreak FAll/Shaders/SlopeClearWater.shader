Shader "Slope Ball/Clear Water"
{
    Properties
    {
        _BaseColor ("Water absorption tint", Color) = (0.83, 0.96, 1, 1)
        _Refraction ("Refraction", Range(0, 0.03)) = 0.008
        _Roughness ("Surface roughness", Range(0.03, 0.4)) = 0.14
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Refraction;
                float _Roughness;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half4 color : COLOR;
            };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 v = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float ndv = saturate(dot(n, v));
                // Water's IOR is 1.333: only about 2% reflects head on.
                float fresnel = 0.0204 + 0.9796 * pow(1 - ndv, 5);
                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                float2 bend = mul((float3x3)UNITY_MATRIX_V, n).xy;
                float thickness = i.uv.x;
                half3 transmitted = SampleSceneColor(saturate(screenUV + bend * _Refraction * thickness));
                half3 tint = lerp(half3(1,1,1), _BaseColor.rgb * i.color.rgb, 0.16 * thickness);
                // A thin film darkens the wet ground slightly without filling it with colour.
                transmitted *= tint * lerp(0.92, 1.0, saturate(thickness));
                half3 reflected = GlossyEnvironmentReflection(reflect(-v, n), _Roughness, 1.0h);
                // Some level scenes have no reflection probe; retain their ambient sky light.
                reflected = max(reflected, SampleSH(reflect(-v, n)) * 0.6h);
                Light sun = GetMainLight();
                float3 h = SafeNormalize(sun.direction + v);
                float ndh = saturate(dot(n,h));
                float ndl = saturate(dot(n,sun.direction));
                float a2 = pow(_Roughness, 4);
                float d = a2 / max(3.14159 * pow(ndh * ndh * (a2 - 1) + 1, 2), 0.000001);
                float specular = min(d * 0.0204 * ndl / max(4 * ndv, 0.15), 8);
                half3 water = lerp(transmitted, reflected, fresnel) + sun.color * specular;
                // Alpha fades the whole refracted result; no painted white outline.
                return half4(water, saturate(i.color.a * _BaseColor.a));
            }
            ENDHLSL
        }
    }
}
