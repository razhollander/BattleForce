Shader "Custom/WaterGunStream"
{
    Properties
    {
        _MainTex ("Cone Sprite", 2D) = "white" {}
        _ColorShallow ("Shallow Color", Color) = (0.4, 0.9, 1.0, 0.8)
        _ColorDeep ("Deep Color", Color) = (0.0, 0.4, 0.9, 1.0)
        _ScrollSpeed ("Scroll Speed", Float) = 1.5
        _NoiseScale ("Noise Scale", Float) = 4.0
        _BendAmount ("Bend Amount", Float) = 0.0
        _Emission ("Emission", Float) = 1.5
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.15
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

            // Simple hash-based noise
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

                // Bend UVs based on angular velocity — shifts horizontally more toward tip
                float bendInfluence = uv.y; // stronger bend at tip (v=1)
                uv.x += _BendAmount * bendInfluence * 0.15;

                // Sample the cone sprite for shape mask + alpha
                float4 spriteColor = tex2D(_MainTex, uv);
                float shapeAlpha = spriteColor.a;

                // Scrolling noise layers for water turbulence
                float2 noiseUV1 = uv * _NoiseScale + float2(0, -_Time.y * _ScrollSpeed);
                float2 noiseUV2 = uv * _NoiseScale * 1.7 + float2(0.3, -_Time.y * _ScrollSpeed * 0.6);

                float noise1 = smoothNoise(noiseUV1);
                float noise2 = smoothNoise(noiseUV2);
                float waterPattern = (noise1 * 0.6 + noise2 * 0.4);

                // Color from shallow (center of cone = wide base) to deep (tip)
                float3 waterColor = lerp(_ColorShallow.rgb, _ColorDeep.rgb, waterPattern);
                waterColor *= _Emission;

                // Soft edge fade (fade at horizontal edges)
                float edgeFade = smoothstep(0.0, _EdgeSoftness, uv.x) *
                                 smoothstep(1.0, 1.0 - _EdgeSoftness, uv.x);

                float alpha = shapeAlpha * edgeFade * lerp(_ColorShallow.a, _ColorDeep.a, waterPattern);

                return float4(waterColor, alpha);
            }
            ENDHLSL
        }
    }
}
