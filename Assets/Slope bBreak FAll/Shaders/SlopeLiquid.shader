// Liquid that spills out of a jar when it breaks, and the puddle it
// leaves on the ground.
//
// The look is built around how water actually reads to the eye:
//
//   * the RIM is bright and opaque, the CENTRE is see-through. Water
//     bends light at its silhouette, so the edge catches a highlight
//     while you look straight through the middle. A blob that is solid
//     in the centre and soft at the edge reads as a bubble instead,
//     which is what this shader used to do.
//   * the edge is slightly irregular, not a perfect circle. A puddle
//     with a mathematically round outline never looks like liquid.
//   * a small offset glint, because a wet surface always has one.
//
// Written for mobile, so the expensive things are left out on purpose:
//
//   * no texture at all - the shape is maths on the UV, so there is
//     zero texture bandwidth on an effect with heavy overdraw
//   * no lighting, no shadows, no probes
//   * half precision, and distances stay squared so there is no sqrt
//   * single pass, GPU instancing on

Shader "Slope Ball/Liquid"
{
    Properties
    {
        [HDR] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Header(Shape)]
        _Softness ("Edge Softness", Range(0.02, 1)) = 0.25
        _EdgeWobble ("Edge Irregularity", Range(0, 0.5)) = 0.12
        _WobbleFrequency ("Edge Irregularity Detail", Range(4, 40)) = 19

        [Header(Water)]
        _CoreAlpha ("Centre Transparency", Range(0, 1)) = 0.45
        _RimBoost ("Rim Brightness", Range(0, 2)) = 0.55
        _RimTightness ("Rim Width", Range(1, 12)) = 3.5
        _Glint ("Glint", Range(0, 1)) = 0.35
        _Depth ("Depth Shading", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType"       = "Transparent"
            "Queue"            = "Transparent"
            "RenderPipeline"   = "UniversalPipeline"
            "IgnoreProjector"  = "True"
            "PreviewType"      = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off

        Pass
        {
            Name "SlopeLiquidUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex SlopeLiquidVertex
            #pragma fragment SlopeLiquidFragment
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half  _Softness;
                half  _EdgeWobble;
                half  _WobbleFrequency;
                half  _CoreAlpha;
                half  _RimBoost;
                half  _RimTightness;
                half  _Glint;
                half  _Depth;
            CBUFFER_END

            Varyings SlopeLiquidVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            half4 SlopeLiquidFragment(Varyings input) : SV_Target
            {
                half2 offset = (half2)input.uv - 0.5h;

                // Squared radius: 0 in the centre, 1 at the edge. Squared on
                // purpose, so the whole silhouette costs one dot product.
                half radiusSq = dot(offset, offset) * 4.0h;

                /*
                 * Do lehrein alag zaviyon par. Ek hi lehar seedhi
                 * pattiyan banati hai aur kinara kat-a hua, kaghaz
                 * jaisa lagta hai; do milne par shakal organic hoti hai.
                 */
                half wave1 = sin(offset.x * _WobbleFrequency +
                                 offset.y * _WobbleFrequency * 0.70h);

                half wave2 = sin(offset.x * _WobbleFrequency * 0.53h -
                                 offset.y * _WobbleFrequency * 1.27h);

                half wobble = (wave1 + wave2) * 0.5h * _EdgeWobble;

                half edge = 1.0h + wobble;

                half silhouette = saturate((edge - radiusSq) / _Softness);

                /*
                 * Rim: brightest right at the outline and falling away
                 * inwards. This is what sells it as liquid rather than a
                 * soft ball - the boundary reads, the middle does not.
                 */
                half rim =
                    saturate(1.0h - abs(edge - radiusSq) * _RimTightness);

                /*
                 * Chhota aur tez glint. Pehle ye chaura aur narm tha,
                 * jis se poori cheez plastic ki gend lagti thi — paani
                 * ki chamak ek chhoti si roshan bindi hoti hai.
                 */
                half2 glintOffset = offset - half2(-0.17h, 0.17h);
                half  glint =
                    saturate(1.0h - dot(glintOffset, glintOffset) * 70.0h) *
                    _Glint;

                half4 tint = _BaseColor * input.color;

                /*
                 * Kinara apne rang mein nahi, safaid ki taraf jhukta hai.
                 * Paani ka kinara roshni ko morta hai, is liye wo apne
                 * andar ke rang se hamesha halka hota hai.
                 */
                half3 rimColour = lerp(tint.rgb, half3(1.0h, 1.0h, 1.0h), 0.65h);

                /*
                 * Upar patla, neeche gehra. Bilkul yaksaan bhara hua
                 * rang paani nahi lagta — asal paani mein jitni gehrai
                 * hoti hai utna rang gehra hota jata hai.
                 */
                half depth = lerp(
                    1.0h + _Depth * 0.5h,
                    1.0h - _Depth,
                    saturate((half)input.uv.y)
                );

                half3 body = saturate(tint.rgb * depth);

                half3 rgb = saturate(
                    lerp(body, rimColour, saturate(rim * _RimBoost)) +
                    glint
                );

                /*
                 * See-through in the middle, solid at the rim. Alpha still
                 * rides on the particle colour, so the fade after the puddle
                 * lands costs nothing here.
                 */
                half opacity = lerp(_CoreAlpha, 1.0h, rim);

                half alpha = tint.a * silhouette * opacity;

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
