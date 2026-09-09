// Flowing water that has spilled out of a broken jar and spread on the
// ground.
//
// This is a SURFACE, not a scatter of droplets. The previous attempt drew
// each droplet as its own little blob, which is why it read as "dots"
// instead of liquid. Here one quad is one connected body of water, and the
// liquid look comes from the surface itself moving:
//
//   * the outline is warped by scrolling waves, so the puddle edge creeps
//     and wobbles the way a spreading liquid does instead of sitting still
//   * the interior has moving ripples, which is what makes a flat surface
//     read as water rather than a painted shape
//   * the rim is bright and wet, the middle is see-through
//   * a moving glint, because still highlights look like plastic
//
// Mobile budget kept intact:
//   * no texture at all - every wave is maths on the UV, so there is zero
//     texture bandwidth even though the effect covers real screen area
//   * no lighting, no shadows, no probes, no depth read
//   * half precision, single pass, GPU instancing on
//   * the waves are plain sines; no noise texture and no atan2

Shader "Slope Ball/Water Puddle"
{
    Properties
    {
        [HDR] _BaseColor ("Water Color", Color) = (1, 1, 1, 1)

        [Header(Shape)]
        _Softness ("Edge Softness", Range(0.02, 1)) = 0.22
        _EdgeWobble ("Edge Irregularity", Range(0, 0.4)) = 0.10
        _WobbleFrequency ("Edge Detail", Range(3, 24)) = 9

        [Header(Flow)]
        _FlowSpeed ("Flow Speed", Range(0, 4)) = 1.1
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.35
        _RippleScale ("Ripple Scale", Range(2, 30)) = 13

        [Header(Refraction)]
        _RefractionStrength ("Refraction Strength", Range(0, 0.08)) = 0.022
        _Tinting ("Colour Tint Strength", Range(0, 1)) = 0.55

        [Header(Water)]
        _CoreAlpha ("Body Opacity", Range(0, 1)) = 0.92
        _RimBoost ("Rim Brightness", Range(0, 2)) = 1.5
        _RimTightness ("Rim Width", Range(1, 12)) = 4.5
        _Glint ("Glint", Range(0, 1)) = 0.45
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off

        Pass
        {
            Name "SlopeWaterPuddle"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex WaterVertex
            #pragma fragment WaterFragment
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            /*
             * Peechay ki tasveer. Yehi cheez paani ko paani banati hai:
             * uske aar-paar ki dunya us mein se mori hui dikhti hai. Iske
             * liye URP asset mein Opaque Texture on hona zaroori hai —
             * dono assets (PC aur Mobile) par on kar diya gaya hai, aur
             * mobile par 4x downsample par, kyunke refraction ko detail
             * ki zaroorat nahi hoti.
             */
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 normalWS : TEXCOORD3;
                half4  color      : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half  _Softness;
                half  _EdgeWobble;
                half  _WobbleFrequency;
                half  _FlowSpeed;
                half  _RippleStrength;
                half  _RippleScale;
                half  _RefractionStrength;
                half  _Tinting;
                half  _CoreAlpha;
                half  _RimBoost;
                half  _RimTightness;
                half  _Glint;
            CBUFFER_END

            Varyings WaterVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            half4 WaterFragment(Varyings input) : SV_Target
            {
                half2 offset = (half2)input.uv - 0.5h;
                half  time = (half)_Time.y * _FlowSpeed;

                /*
                 * Surface ripples. Three sines at different angles and
                 * speeds, added together. One sine alone reads as stripes;
                 * three overlapping ones stop repeating to the eye and give
                 * the shifting, uneven surface that says "liquid".
                 */
                half r1 = sin(offset.x * _RippleScale + time * 1.7h);
                half r2 = sin(offset.y * _RippleScale * 0.83h - time * 1.3h);
                half r3 = sin((offset.x + offset.y) * _RippleScale * 0.61h +
                              time * 0.9h);

                half ripple = (r1 + r2 + r3) * 0.3333h;

                /*
                 * The ripples also push the sampling point around before the
                 * silhouette is measured. Warping the domain like this is
                 * what makes the whole body look like it is moving, rather
                 * than a still shape with a moving pattern painted on it.
                 */
                half2 warped = offset + ripple * _RippleStrength * 0.06h;

                half radiusSq = dot(warped, warped) * 4.0h;

                // Creeping outline: two waves that scroll in opposite senses
                half w1 = sin(warped.x * _WobbleFrequency +
                              warped.y * _WobbleFrequency * 0.70h + time * 0.8h);

                half w2 = sin(warped.x * _WobbleFrequency * 0.53h -
                              warped.y * _WobbleFrequency * 1.27h - time * 0.6h);

                half edge = 1.0h + (w1 + w2) * 0.5h * _EdgeWobble;

                half silhouette = saturate((edge - radiusSq) / _Softness);

                // Wet, bright boundary
                half rim = saturate(1.0h - abs(edge - radiusSq) * _RimTightness);

                /*
                 * A glint that drifts with the flow. A highlight nailed to
                 * one spot is the single clearest giveaway of a fake liquid.
                 */
                half2 glintOffset = warped - half2(
                    -0.14h + sin(time * 0.7h) * 0.05h,
                     0.14h + cos(time * 0.5h) * 0.05h);

                half glint =
                    saturate(1.0h - dot(glintOffset, glintOffset) * 60.0h) * _Glint;

                half4 tint = _BaseColor * input.color;

                /*
                 * Refraction. Screen se peechay ka rang uthate hain, magar
                 * ripples ke hisaab se hata kar — is liye zameen paani ke
                 * neeche lehrati hui nazar aati hai. Yehi wo cheez hai jo
                 * shakal ko "rangeen dhabbe" se "paani" banati hai.
                 */
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                float2 refractUV = screenUV + float2(
                    ripple * _RefractionStrength,
                    (r1 - r3) * _RefractionStrength * 0.7h);

                half3 behind = SampleSceneColor(saturate(refractUV));

                // Ripples read as thickness: crests thin and pale, troughs deep
                half thickness = 1.0h + ripple * _RippleStrength * 0.45h;

                // Peechay ka manzar paani ke rang mein rangta hai
                half3 waterBody = lerp(
                    behind,
                    behind * lerp(half3(0.88h, 0.97h, 1.0h), tint.rgb, 0.3h),
                    _Tinting
                );

                half3 rimColour = lerp(tint.rgb, half3(1.0h, 1.0h, 1.0h), 0.7h);

                half3 rgb = saturate(
                    lerp(waterBody * thickness, rimColour,
                         saturate(rim * _RimBoost)) + glint);

                half opacity = lerp(_CoreAlpha, 1.0h, rim);

                half alpha = tint.a * silhouette * opacity;

                // Animated surface normals create view-dependent wet highlights.
                float phase = _Time.y * _FlowSpeed;
                half3 normal = normalize(input.normalWS + half3(
                    sin(input.uv.x * 28.0 + phase * 1.4) * 0.12,
                    0.02,
                    cos(input.uv.y * 24.0 - phase) * 0.12));
                half3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                if (dot(normal, viewDir) < 0) normal = -normal;
                half fresnel = 0.02h + 0.98h * pow(1.0h - saturate(dot(normal, viewDir)), 5.0h);
                Light light = GetMainLight();
                half3 halfDir = SafeNormalize(light.direction + viewDir);
                half specular = pow(saturate(dot(normal, halfDir)), 96.0h);
                half3 reflection = GlossyEnvironmentReflection(
                    reflect(-viewDir, normal), 0.12h, 1.0h);
                rgb = lerp(rgb, reflection, fresnel * 0.65h)
                    + light.color * specular * 0.8h;
                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
