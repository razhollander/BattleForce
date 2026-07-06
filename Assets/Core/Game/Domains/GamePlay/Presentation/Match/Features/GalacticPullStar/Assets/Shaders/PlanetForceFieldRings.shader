Shader "Custom/PlanetForceFieldRings"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _StartColor ("Start Color (at outer radius)", Color) = (0.3, 0.7, 1, 1)
        [HDR] _EndColor ("End Color (at inner radius)", Color) = (0.1, 0.2, 1, 0)

        _RadiusOuter ("Outer Radius X (spawn)", Range(0, 1)) = 0.95
        _RadiusInner ("Inner Radius Y (planet surface)", Range(0, 1)) = 0.45
        _RingWidth ("Ring Width", Range(0.001, 0.5)) = 0.06

        _SpawnInterval ("Spawn Interval (seconds)", Range(0.01, 5)) = 0.6
        _ShrinkSpeed ("Shrink Speed (radius/sec)", Range(0.01, 2)) = 0.25
        _ShrinkEase ("Shrink Ease (1=linear, >1 slow-in, <1 slow-out)", Range(0.1, 5)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define MAX_RINGS 24

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float4 _StartColor;
            float4 _EndColor;
            float _RadiusOuter;
            float _RadiusInner;
            float _RingWidth;
            float _SpawnInterval;
            float _ShrinkSpeed;
            float _ShrinkEase;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Distance from the sprite center, normalized so that 1.0 == half the sprite size.
                float dist = length(IN.uv - 0.5) * 2.0;

                float outer = _RadiusOuter;
                float inner = min(_RadiusInner, _RadiusOuter);
                float travel = max(outer - inner, 1e-4);

                // Time it takes a single ring to shrink from the outer to the inner radius.
                float lifetime = travel / max(_ShrinkSpeed, 1e-4);
                float halfWidth = _RingWidth * 0.5;
                float time = _Time.y;

                // Newest ring index for the current time; older rings are previous indices.
                float currentIndex = floor(time / _SpawnInterval);

                float3 accumulatedColor = 0;
                float accumulatedAlpha = 0;

                [loop]
                for (int i = 0; i < MAX_RINGS; i++)
                {
                    float ringIndex = currentIndex - i;
                    float spawnTime = ringIndex * _SpawnInterval;
                    float age = time - spawnTime;

                    float lifeProgress = age / lifetime; // 0 at outer, 1 at inner
                    if (lifeProgress < 0.0 || lifeProgress > 1.0)
                    {
                        continue;
                    }

                    float easedProgress = pow(saturate(lifeProgress), _ShrinkEase);
                    float radius = lerp(outer, inner, easedProgress);

                    // Soft band centered on the ring radius.
                    float band = 1.0 - smoothstep(0.0, halfWidth, abs(dist - radius));

                    // Fade rings in as they spawn and out as they reach the surface to avoid popping.
                    float birthFade = smoothstep(0.0, 0.12, lifeProgress);
                    float deathFade = 1.0 - smoothstep(0.85, 1.0, lifeProgress);
                    float lifeFade = birthFade * deathFade;

                    float4 ringColor = lerp(_StartColor, _EndColor, easedProgress);
                    float ringAlpha = band * ringColor.a * lifeFade;

                    // Additive accumulation reads as glowing energy where rings overlap.
                    accumulatedColor += ringColor.rgb * ringAlpha;
                    accumulatedAlpha = saturate(accumulatedAlpha + ringAlpha);
                }

                float4 spriteCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float3 finalRgb = accumulatedColor * IN.color.rgb;
                float finalAlpha = accumulatedAlpha * IN.color.a * spriteCol.a;

                return float4(finalRgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
