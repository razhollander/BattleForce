Shader "Custom/WaterGunStream"
{
    Properties
    {
        _MainTex ("Cone Sprite", 2D) = "white" {}
        _ColorShallow ("Shallow / Foam Color", Color) = (0.7, 0.95, 1.0, 0.9)
        _ColorDeep ("Deep Core Color", Color) = (0.0, 0.35, 0.9, 1.0)
        _ScrollSpeed ("Scroll Speed", Float) = 1.5
        _NoiseScale ("Noise Scale", Float) = 4.0
        _BendAmount ("Bend Amount", Float) = 0.0

        _Emission ("Emission", Float) = 1.6
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.15

        [Header(Flow)]
        _WarpStrength ("Domain Warp Strength", Range(0, 1)) = 0.35
        _FoamSpeed ("Foam Streak Speed", Float) = 4.0
        _FoamSharpness ("Foam Streak Sharpness", Range(1, 24)) = 8.0
        _FoamAmount ("Foam Streak Amount", Range(0, 2)) = 1.0

        [Header(Pressure)]
        _PulseSpeed ("Pressure Pulse Speed", Float) = 3.0
        _PulseFreq ("Pressure Pulse Frequency", Float) = 3.0
        _PulseAmount ("Pressure Pulse Amount", Range(0, 1)) = 0.25

        [Header(Volume)]
        _CoreTightness ("Core Tightness", Range(0.5, 4)) = 1.6
        _RimBoost ("Rim Highlight", Range(0, 2)) = 0.6

        [Header(Tip Spray)]
        _SprayStart ("Spray Start (v)", Range(0, 1)) = 0.55
        _SprayScale ("Spray Droplet Scale", Float) = 10.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _ColorShallow;
                float4 _ColorDeep;
                float _ScrollSpeed;
                float _NoiseScale;
                float _BendAmount;
                float _Emission;
                float _EdgeSoftness;

                float _WarpStrength;
                float _FoamSpeed;
                float _FoamSharpness;
                float _FoamAmount;

                float _PulseSpeed;
                float _PulseFreq;
                float _PulseAmount;

                float _CoreTightness;
                float _RimBoost;

                float _SprayStart;
                float _SprayScale;
            CBUFFER_END

            sampler2D _MainTex;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            // --- Hash-based value noise ---------------------------------------
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

            // Fractal noise (3 octaves) — cheap turbulence.
            float fbm(float2 uv)
            {
                float sum = 0.0;
                float amp = 0.5;
                [unroll]
                for (int o = 0; o < 3; o++)
                {
                    sum += smoothNoise(uv) * amp;
                    uv *= 2.0;
                    amp *= 0.5;
                }
                return sum;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float t = _Time.y;

                // v = along the stream (0 at nozzle, 1 at tip), x = across the width.
                float along = uv.y;
                float across = uv.x;

                // Angular-velocity bend — stronger toward the tip.
                uv.x += _BendAmount * along * 0.15;

                // Cone sprite gives the base silhouette + alpha.
                float4 spriteColor = tex2D(_MainTex, uv);
                float shapeAlpha = spriteColor.a;

                // --- Domain-warped turbulent flow ----------------------------
                // Warp the sample coords with a slower noise so the water swirls
                // instead of scrolling as a flat sheet.
                float2 flow = float2(0.0, -t * _ScrollSpeed);
                float2 warp = float2(
                    smoothNoise(uv * _NoiseScale * 0.5 + flow * 0.5),
                    smoothNoise(uv * _NoiseScale * 0.5 + flow * 0.5 + 37.2)
                ) - 0.5;

                float2 baseUV = uv * _NoiseScale + flow + warp * _WarpStrength;
                float water = fbm(baseUV);

                // --- Fast foam streaks (bright thin highlights) --------------
                // A second, faster-scrolling noise, sharpened into ridges.
                float2 foamUV = uv * float2(_NoiseScale * 0.7, _NoiseScale * 1.6)
                                + float2(0.0, -t * _FoamSpeed);
                float foamNoise = fbm(foamUV);
                float foam = pow(saturate(foamNoise), _FoamSharpness) * _FoamAmount;

                // --- Pressure pulses travelling down the length --------------
                float pulse = sin((along * _PulseFreq - t * _PulseSpeed) * 6.2831);
                float pressure = 1.0 + pulse * _PulseAmount;

                // --- Volumetric cross-section shading ------------------------
                // Distance from the center line of the cone (0 center .. 1 edge).
                float centerDist = saturate(abs(across - 0.5) * 2.0);
                // Bright, deep core; lighter translucent edges.
                float core = pow(1.0 - centerDist, _CoreTightness);
                // Thin bright rim just before the edge fade.
                float rim = smoothstep(0.6, 1.0, centerDist) * _RimBoost;

                // --- Compose color -------------------------------------------
                float depth = saturate(water * core + foam);
                float3 waterColor = lerp(_ColorShallow.rgb, _ColorDeep.rgb, 1.0 - core);
                waterColor = lerp(waterColor, _ColorShallow.rgb, saturate(foam + rim));
                waterColor *= _Emission * pressure;

                // --- Edge fade across the width ------------------------------
                float edgeFade = smoothstep(0.0, _EdgeSoftness, uv.x) *
                                 smoothstep(1.0, 1.0 - _EdgeSoftness, uv.x);

                // --- Tip spray: erode into broken droplets near the tip ------
                float sprayNoise = fbm(uv * _SprayScale + float2(0.0, -t * _FoamSpeed));
                // 0 before SprayStart, ramps to 1 at the tip.
                float sprayZone = smoothstep(_SprayStart, 1.0, along);
                // Erode alpha where noise is low, only inside the spray zone.
                float sprayMask = 1.0 - sprayZone * step(sprayNoise, sprayZone * 0.9);

                float bodyAlpha = lerp(_ColorShallow.a, _ColorDeep.a, 1.0 - core);
                float alpha = shapeAlpha * edgeFade * bodyAlpha
                              * pressure * sprayMask;
                alpha = saturate(alpha + foam * shapeAlpha * edgeFade);

                return float4(waterColor, alpha);
            }
            ENDHLSL
        }
    }
}
