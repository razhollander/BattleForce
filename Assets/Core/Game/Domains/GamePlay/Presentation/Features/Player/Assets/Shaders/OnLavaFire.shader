Shader "Custom/OnLavaFire"
{
    Properties
    {
        _MainTex ("Shape Sprite (alpha = mask)", 2D) = "white" {}

        [Header(Palette)]
        _ColorCore ("Core Color (hottest)", Color) = (1.0, 0.92, 0.45, 1.0)
        _ColorMid  ("Mid Color", Color)            = (1.0, 0.55, 0.10, 1.0)
        _ColorEdge ("Edge Color (coolest)", Color) = (0.85, 0.14, 0.03, 1.0)
        _Emission  ("Emission", Float)             = 1.35

        [Header(Cel Shading)]
        // Number of flat color steps. Low = chunky/vector, high = smoother.
        _Bands       ("Color Bands", Range(2, 8)) = 3
        // Crispness of the band edges. Tiny value = hard vector cuts (+AA).
        _BandSharpness ("Band Edge AA", Range(0.001, 0.15)) = 0.03

        [Header(Inked Outline)]
        _OutlineColor ("Outline Color", Color)      = (0.28, 0.02, 0.0, 1.0)
        _OutlineWidth ("Outline Width", Range(0, 0.4)) = 0.12

        [Header(Motion)]
        _RiseSpeed    ("Rise Speed (base)", Float) = 1.5
        // Rise speed is modulated by a sine: speed = base * (1 + wave*sin(freq*t)).
        _RiseWave     ("Rise Speed Wave", Range(0, 1)) = 0.6
        _RiseWaveFreq ("Rise Wave Frequency", Float) = 2.0
        _NoiseScale   ("Noise Scale", Float)      = 3.0
        _WarpStrength ("Turbulence (domain warp)", Range(0, 1)) = 0.45
        _WarpSpeed    ("Turbulence Speed", Float) = 0.6

        [Header(Flame Shape)]
        // How much the flame tapers to a point toward the top (v = 1).
        _Taper      ("Vertical Taper (cone)", Range(0, 4)) = 1.8
        // Bottom is hottest; controls how fast heat falls off going up.
        _HeatFalloff ("Heat Falloff", Range(0.2, 4)) = 1.2
        // Erode the flame outline; higher = thinner, more separated tongues.
        _Threshold  ("Flame Threshold", Range(0, 1)) = 0.34
        _EdgeSoftness ("Silhouette AA", Range(0.005, 0.3)) = 0.04

        [Header(Flicker)]
        _FlickerSpeed  ("Flicker Speed", Float)        = 8.0
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.15

        [Header(Tint)]
        // Multiplies final color+alpha, and picks up SpriteRenderer color.
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

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _ColorCore;
                float4 _ColorMid;
                float4 _ColorEdge;
                float _Emission;

                float _Bands;
                float _BandSharpness;

                float4 _OutlineColor;
                float _OutlineWidth;

                float _RiseSpeed;
                float _RiseWave;
                float _RiseWaveFreq;
                float _NoiseScale;
                float _WarpStrength;
                float _WarpSpeed;

                float _Taper;
                float _HeatFalloff;
                float _Threshold;
                float _EdgeSoftness;

                float _FlickerSpeed;
                float _FlickerAmount;

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

            // --- Hash-based value noise (matches project convention) ----------
            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float smoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // Fractal noise — only 2 octaves so the flame reads as bold,
            // vector-like blobs instead of fine realistic wisps.
            float fbm(float2 uv)
            {
                float sum = 0.0;
                float amp = 0.5;
                [unroll]
                for (int o = 0; o < 2; o++)
                {
                    sum += smoothNoise(uv) * amp;
                    uv *= 2.0;
                    amp *= 0.5;
                }
                return sum / 0.75; // renormalize ~0..1
            }

            // Quantize a 0..1 value into flat cel bands with a thin AA seam.
            float posterize(float h)
            {
                h = saturate(h);
                float scaled = h * _Bands;
                float lower = floor(scaled);
                // Smooth only across the band seam so cuts stay crisp.
                float seam = smoothstep(0.5 - _BandSharpness, 0.5 + _BandSharpness, frac(scaled));
                return (lower + seam) / _Bands;
            }

            // Two hard-ish transitions across a 3-color palette.
            float3 fireRamp(float h)
            {
                float3 c = lerp(_ColorEdge.rgb, _ColorMid.rgb, saturate(h * 2.0));
                c = lerp(c, _ColorCore.rgb, saturate(h * 2.0 - 1.0));
                return c;
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

                // Sprite alpha is the containing silhouette / mask.
                float shapeAlpha = tex2D(_MainTex, uv).a;

                // v runs bottom(0) -> top(1). Fire rises, so scroll noise DOWN
                // in uv over time and the visible tongues travel UP.
                // The rise SPEED is sinusoidal: speed = base*(1 + wave*sin(freq*t)).
                // The scroll offset below is the integral of that speed, so the
                // motion stays smooth while visibly easing faster/slower.
                float riseOffset = _RiseSpeed * (t - (_RiseWave / max(1e-4, _RiseWaveFreq)) * cos(t * _RiseWaveFreq));
                float2 rise = float2(0.0, -riseOffset);

                // --- Domain warp: slow swirling turbulence --------------------
                float2 warpFlow = float2(0.0, -t * _WarpSpeed);
                float2 warp = float2(
                    smoothNoise(uv * _NoiseScale * 0.5 + warpFlow),
                    smoothNoise(uv * _NoiseScale * 0.5 + warpFlow + 41.7)
                ) - 0.5;

                float2 sampleUV = uv * _NoiseScale + rise + warp * _WarpStrength;
                float noise = fbm(sampleUV);

                // --- Flame shaping --------------------------------------------
                // Hotter at the base, and tapered toward the top so the flame
                // narrows into a cone / licking tongues.
                float verticalFalloff = pow(saturate(1.0 - uv.y), _HeatFalloff);
                float centerDist = abs(uv.x - 0.5) * 2.0;             // 0 center .. 1 side
                float taperMask = saturate(1.0 - centerDist * (1.0 + uv.y * _Taper));

                float heat = noise * verticalFalloff * taperMask;

                // Global flicker (breathing brightness).
                float flicker = 1.0 + sin(t * _FlickerSpeed) * _FlickerAmount
                                    + (smoothNoise(float2(t * _FlickerSpeed * 0.5, 0.0)) - 0.5) * _FlickerAmount;
                heat *= flicker;

                // --- Crisp silhouette + inked outline -------------------------
                // Outer edge of the whole flame (includes the outline ring).
                float flameOuter = smoothstep(_Threshold, _Threshold + _EdgeSoftness, heat);
                // Inner fill, pushed in by the outline width.
                float innerT = _Threshold + _OutlineWidth;
                float flameInner = smoothstep(innerT, innerT + _EdgeSoftness, heat);
                float outlineMask = saturate(flameOuter - flameInner);

                // --- Cel-banded interior color --------------------------------
                float rampH = saturate((heat - innerT) / max(1e-4, 1.0 - innerT));
                float3 interior = fireRamp(posterize(rampH)) * _Emission;

                // Lay the inked outline over the interior.
                float3 col = lerp(interior, _OutlineColor.rgb, outlineMask);

                float alpha = flameOuter * shapeAlpha * _OverallAlpha;

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
