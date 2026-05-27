Shader "Custom/StylizedLavaWorldUV_Stretch"
{
    Properties
    {
        _MainTex ("Lava Noise", 2D) = "white" {}
        _ColorDeep ("Deep Color", Color) = (0.5, 0, 0, 1)
        _ColorBright ("Bright Color", Color) = (1, 0.5, 0, 1)
        _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0.05, 0.1, -0.05)
        _Stretch ("World Stretch (X, Y)", Vector) = (1, 1, 0, 0)
        _Bands ("Color Bands", Float) = 4.0
        _Emission ("Emission Intensity", Float) = 2.0
        _WobbleStrength ("Wobble Strength", Range(0.0, 0.2)) = 0.03
        _WobbleSpeed ("Wobble Speed", Float) = 2.0
        _WobbleFrequency ("Wobble Frequency", Float) = 4.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off Blend One OneMinusSrcAlpha 

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // --- INSTANCING BUFFER ---
            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
                UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ColorDeep)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ColorBright)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ScrollSpeed)
                UNITY_DEFINE_INSTANCED_PROP(float2, _Stretch)
                UNITY_DEFINE_INSTANCED_PROP(float, _Bands)
                UNITY_DEFINE_INSTANCED_PROP(float, _Emission)
                UNITY_DEFINE_INSTANCED_PROP(float, _WobbleStrength)
                UNITY_DEFINE_INSTANCED_PROP(float, _WobbleSpeed)
                UNITY_DEFINE_INSTANCED_PROP(float, _WobbleFrequency)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            sampler2D _MainTex;

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;    
                float2 worldUV      : TEXCOORD1;    
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert (Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(worldPos);
                
                float4 st = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                output.uv = input.uv * st.xy + st.zw;
                output.color = input.color;
                
                float2 stretch = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Stretch);
                output.worldUV = worldPos.xy * stretch;

                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Fetch props via instancing macro
                float wobbleFreq = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _WobbleFrequency);
                float wobbleSpeed = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _WobbleSpeed);
                float wobbleStr = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _WobbleStrength);
                float4 scroll = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ScrollSpeed);
                float bands = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Bands);
                float emission = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Emission);

                float waveX = sin(input.worldUV.y * wobbleFreq + _Time.y * wobbleSpeed) * wobbleStr;
                float waveY = cos(input.worldUV.x * wobbleFreq + _Time.y * wobbleSpeed) * wobbleStr;
                
                float2 distortedWorldUV = input.worldUV + float2(waveX, waveY);
                float2 uv1 = frac(distortedWorldUV + scroll.xy * _Time.y);
                float2 uv2 = frac(distortedWorldUV - scroll.zw * _Time.y);

                float noise1 = tex2D(_MainTex, uv1).r;
                float noise2 = tex2D(_MainTex, uv2).r;
                float combinedNoise = floor(((noise1 + noise2) * 0.5) * bands) / bands;

                float3 deep = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ColorDeep).rgb;
                float3 bright = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ColorBright).rgb;
                float3 lavaColor = lerp(deep, bright, combinedNoise) * emission;

                float4 finalColor = float4(lavaColor, tex2D(_MainTex, input.uv).a * input.color.a);
                finalColor.rgb *= finalColor.a; 
                return finalColor;
            }
            ENDHLSL
        }
    }
}