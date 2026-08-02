Shader "Custom/CartoonFire"
{
    Properties
    {
        _MainTex ("Shape Sprite (alpha = mask)", 2D) = "white" {}

        [Header(Palette)]
        _ColorCore ("Core Color (hottest)", Color) = (1.0, 0.94, 0.55, 1.0)
        _ColorMid  ("Mid Color", Color)            = (1.0, 0.60, 0.12, 1.0)
        _ColorEdge ("Edge Color (coolest)", Color) = (0.95, 0.20, 0.06, 1.0)
        _OutlineColor ("Outline (ink)", Color)     = (0.22, 0.03, 0.0, 1.0)
        _Emission  ("Emission", Float)             = 1.25

        [Header(Flame Tongues)]
        // Number of rounded flame tongues across the width.
        _Tongues        ("Tongue Count", Range(1, 12)) = 4
        // Pointiness of each tongue (higher = spikier).
        _TongueSharpness ("Tongue Sharpness", Range(0.4, 4)) = 1.6
        // How fast the flame fades toward the center (higher = emptier middle).
        _CenterFade     ("Center Fade Power", Range(0.2, 5)) = 1.4
        // Overall flame height (0..1 of the sprite).
        _Height         ("Flame Height", Range(0.1, 1.5)) = 0.95
        // Height of the mid / core layers relative to the outer flame.
        _MidScale       ("Mid Layer Height", Range(0.2, 1)) = 0.72
        _CoreScale      ("Core Layer Height", Range(0.1, 1)) = 0.45

        [Header(Animation)]
        _LickSpeed ("Lick Speed", Float)          = 2.2
        _Sway      ("Side Sway Amount", Range(0, 0.3)) = 0.06
        _SwaySpeed ("Side Sway Speed", Float)     = 1.6

        [Header(Ink Outline)]
        _OutlineWidth ("Outline Width", Range(0, 0.25)) = 0.06
        _AA           ("Edge AA", Range(0.002, 0.08)) = 0.012

        [Header(Tint)]
        [Toggle] _UseVertexColor ("Use Vertex Color", Float) = 1
        _OverallAlpha ("Overall Alpha", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _USEVERTEXCOLOR_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define TAU 6.28318530718

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _ColorCore;
                float4 _ColorMid;
                float4 _ColorEdge;
                float4 _OutlineColor;
                float _Emission;

                float _Tongues;
                float _TongueSharpness;
                float _CenterFade;
                float _Height;
                float _MidScale;
                float _CoreScale;

                float _LickSpeed;
                float _Sway;
                float _SwaySpeed;

                float _OutlineWidth;
                float _AA;

                float _OverallAlpha;
            CBUFFER_END

            sampler2D _MainTex;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            // Flame silhouette height (0..1) at column x for a layer of the given
            // scale. Built symmetrically around the center from scrolling cosines
            // so the tongues travel from the sides INWARD and fade out in the
            // middle (no noise -> reads as hand-drawn cartoon).
            float flameTop(float x, float t, float scale)
            {
                // d: 0 at the center, 1 at the left/right edges. Working in d
                // mirrors the flame across the center line.
                float d = abs(x - 0.5) * 2.0;

                float sway = sin(t * _SwaySpeed + d * 3.0) * _Sway;
                float dd = d + sway;

                // Adding +t to the phase makes each crest satisfy its phase at a
                // smaller d as time passes -> tongues slide toward the center.
                float a = cos((dd * _Tongues + t * _LickSpeed * 0.5) * TAU);
                float b = cos((dd * _Tongues * 0.5 + t * _LickSpeed * 0.3) * TAU + 1.7);
                float tongue = saturate((a * 0.6 + b * 0.4) * 0.5 + 0.5);
                tongue = pow(tongue, _TongueSharpness);

                // Tall at the sides, fading to nothing at the center.
                float sideEnv = pow(saturate(d), _CenterFade);

                float bob = 1.0 + 0.12 * sin(t * _LickSpeed * 1.7);
                return _Height * scale * tongue * sideEnv * bob;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float t = _Time.y;
                float aa = _AA;

                float shapeAlpha = tex2D(_MainTex, uv).a;

                // Per-layer flame tops (outer >= mid >= core).
                float topEdge = flameTop(uv.x, t, 1.0);
                float topMid  = flameTop(uv.x, t, _MidScale);
                float topCore = flameTop(uv.x, t, _CoreScale);

                // Masks: 1 below the top edge (inside the flame), 0 above.
                float mEdge = smoothstep(topEdge + aa, topEdge - aa, uv.y);
                float mMid  = smoothstep(topMid  + aa, topMid  - aa, uv.y);
                float mCore = smoothstep(topCore + aa, topCore - aa, uv.y);

                // Flat color layers.
                float3 col = _ColorEdge.rgb;
                col = lerp(col, _ColorMid.rgb,  mMid);
                col = lerp(col, _ColorCore.rgb, mCore);
                col *= _Emission;

                // Ink outline: band just inside the outer silhouette.
                float innerTop = topEdge - _OutlineWidth;
                float mInner = smoothstep(innerTop + aa, innerTop - aa, uv.y);
                float ink = saturate(mEdge - mInner);
                col = lerp(col, _OutlineColor.rgb, ink);

                float alpha = mEdge * shapeAlpha * _OverallAlpha;

                #ifdef _USEVERTEXCOLOR_ON
                    col *= input.color.rgb;
                    alpha *= input.color.a;
                #endif

                return float4(col, saturate(alpha));
            }
            ENDHLSL
        }
    }

    Fallback Off
}
