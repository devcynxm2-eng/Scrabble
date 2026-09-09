// The water running down out of a broken jar, before it reaches the ground
// and spreads.
//
// This is the piece that was missing. The spill used to be a scatter of
// separate droplets, so nothing ever read as "water falling" - it read as
// dots. Here the fall is ONE connected stream on a single vertical quad,
// and the stream is drawn by the shader:
//
//   * the leading front travels down the quad as the particle ages, so the
//     water visibly descends instead of appearing all at once
//   * the column is wide where it pours out and necks down as it falls,
//     with a rounded head at the front - the shape a real stream takes
//   * the edges wobble and scroll downward, which is what makes it read as
//     moving liquid rather than a painted stripe
//   * the top retreats once the jar has emptied, so the stream detaches
//     from the jar and finishes falling on its own
//   * the background refracts through it, same as the puddle
//
// The particle's normalised age arrives in TEXCOORD0.z through a custom
// vertex stream, which is what drives the whole animation. No script has to
// touch the material, so every stream in the scene still shares one draw
// call.
//
// Mobile budget:
//   * no texture except the one scene-colour read the refraction needs
//   * no lighting, no shadows, no probes, no depth read
//   * half precision, single pass, plain sines - no noise, no atan2

Shader "Slope Ball/Water Fall"
{
    Properties
    {
        [HDR] _BaseColor ("Water Color", Color) = (1, 1, 1, 1)

        [Header(Stream)]
        _Width ("Stream Width", Range(0.05, 1)) = 0.55
        _Neck ("Neck Narrowing", Range(0, 1)) = 0.45
        _HeadBulge ("Head Bulge", Range(0, 1)) = 0.55
        _EdgeSoft ("Edge Softness", Range(0.005, 0.3)) = 0.06

        [Header(Flow)]
        _FlowSpeed ("Flow Speed", Range(0, 6)) = 2.4
        _WaveStrength ("Edge Wave", Range(0, 0.5)) = 0.16
        _WaveScale ("Edge Wave Detail", Range(2, 30)) = 11
        _FrontSpeed ("Fall Speed", Range(0.5, 3)) = 1.25

        [Header(Refraction)]
        _RefractionStrength ("Refraction Strength", Range(0, 0.08)) = 0.018
        _Tinting ("Colour Tint Strength", Range(0, 1)) = 0.5

        [Header(Water)]
        _BodyAlpha ("Body Opacity", Range(0, 1)) = 0.9
        _RimBoost ("Rim Brightness", Range(0, 2)) = 1.3
        _Glint ("Glint", Range(0, 1)) = 0.4
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
            Name "SlopeWaterFall"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex FallVertex
            #pragma fragment FallFragment
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                // .xy = quad UV, .z = particle age 0..1 (custom vertex stream)
                float3 uv         : TEXCOORD0;
                half4  color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 normalWS : TEXCOORD3;
                half4  color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half  _Width;
                half  _Neck;
                half  _HeadBulge;
                half  _EdgeSoft;
                half  _FlowSpeed;
                half  _WaveStrength;
                half  _WaveScale;
                half  _FrontSpeed;
                half  _RefractionStrength;
                half  _Tinting;
                half  _BodyAlpha;
                half  _RimBoost;
                half  _Glint;
            CBUFFER_END

            Varyings FallVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            half4 FallFragment(Varyings input) : SV_Target
            {
                // 0 at the jar, 1 at the ground
                half down = 1.0h - (half)input.uv.y;
                half across = (half)input.uv.x - 0.5h;

                half age = saturate((half)input.uv.z);
                half time = (half)_Time.y * _FlowSpeed;

                /*
                 * The front of the water travels down as the particle ages.
                 * Everything below the front has not been reached yet, so it
                 * is simply not drawn - that is what makes the stream fall.
                 */
                half front = saturate(age * _FrontSpeed);

                half reached = saturate((front - down) / 0.04h);

                /*
                 * Once the jar has emptied the top end lets go and travels
                 * down too, so the stream detaches instead of staying glued
                 * to a jar that is no longer pouring.
                 */
                half tailStart = (age - 0.72h) * 3.2h;
                half tail = saturate((down - tailStart) / 0.14h);

                /*
                 * Width: wide where it pours, necking down as it falls, with
                 * a rounded head at the leading edge. The scrolling waves on
                 * the sides are what stop it reading as a painted stripe.
                 */
                half neck = lerp(1.0h, 1.0h - _Neck, saturate(down * 1.2h));

                half headNear = saturate(1.0h - abs(down - front) * 9.0h);
                half head = 1.0h + _HeadBulge * headNear;

                /*
                 * Lehrein neeche ki taraf behti hain. Do alag scale par,
                 * warna kinara ek yaksaan zigzag ban jata hai aur dhaar
                 * shishe ki naali jaisi lagti hai, paani jaisi nahi.
                 */
                half wave =
                    sin(down * _WaveScale - time * 3.0h) * 0.6h +
                    sin(down * _WaveScale * 2.3h - time * 4.7h) * 0.25h +
                    sin(down * _WaveScale * 0.57h - time * 2.1h) * 0.4h;

                half halfWidth =
                    0.5h * _Width * neck * head * (1.0h + wave * _WaveStrength);

                half insideX = saturate((halfWidth - abs(across)) / _EdgeSoft);

                half shape = insideX * reached * tail;

                if (shape <= 0.001h)
                {
                    discard;
                }

                // Wet, bright boundary along the sides and the head
                half rim = saturate(1.0h - (halfWidth - abs(across)) / _EdgeSoft * 0.5h) *
                           saturate(insideX * 2.0h);

                rim = max(rim, headNear * reached * 0.8h);

                /*
                 * Refraction: the ground behind the stream is sampled with a
                 * sideways offset that follows the waves, so the world bends
                 * as the water runs over it.
                 */
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                float2 refractUV = screenUV + float2(
                    wave * _RefractionStrength,
                    -front * _RefractionStrength * 0.4h);

                half3 behind = SampleSceneColor(saturate(refractUV));

                half4 tint = _BaseColor * input.color;

                half3 waterBody = lerp(
                    behind,
                    behind * lerp(half3(0.88h, 0.97h, 1.0h), tint.rgb, 0.3h),
                    _Tinting
                );

                half3 rimColour = lerp(tint.rgb, half3(1.0h, 1.0h, 1.0h), 0.7h);

                half glint =
                    saturate(1.0h - abs(across + 0.12h) * 14.0h) *
                    saturate(1.0h - abs(down - front + 0.08h) * 6.0h) * _Glint;

                half3 rgb = saturate(
                    lerp(waterBody, rimColour, saturate(rim * _RimBoost)) + glint);

                half alpha = tint.a * shape * lerp(_BodyAlpha, 1.0h, rim);

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
