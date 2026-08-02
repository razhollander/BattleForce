Shader "Custom/SineWaveSpiralURP"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        
        [Header(Wave Settings)]
        _WaveSpeed ("Wave Speed", Float) = 3.0
        _WaveFrequency ("Wave Frequency", Float) = 10.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.1
        // Driven from C# (PlayerTailView) instead of _Time so the wave can be frozen in place.
        _WavePhase ("Wave Phase", Float) = 0.0
        
        [Header(Spiral Settings)]
        _SpiralAmount ("Spiral Amount", Float) = 0.0 
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Transparent"
        }
        
        LOD 100

        // --- PASS 0: REGULAR UNLIT DISPLAY ---
        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST; float4 _BaseColor; float _WaveSpeed; float _WaveFrequency; float _WaveAmplitude; float _SpiralAmount; float _WavePhase;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 pivot = float2(0.5, 0.0); 
                float2 centeredUV = uv - pivot;
                float angle = _SpiralAmount * uv.y;
                float s = sin(angle); float c = cos(angle);
                float2 spiraledUV;
                spiraledUV.x = centeredUV.x * c - centeredUV.y * s;
                spiraledUV.y = centeredUV.x * s + centeredUV.y * c;
                uv = spiraledUV + pivot;

                float wave = sin(uv.y * _WaveFrequency + _WavePhase * _WaveSpeed);
                uv.x += wave * _WaveAmplitude * uv.y;

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _BaseColor * IN.color;
            }
            ENDHLSL
        }

        // --- PASS 1: DEDICATED SILHOUETTE SHADOW PASS ---
        Pass
        {
            Name "MovementShadow"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST; float4 _BaseColor; float _WaveSpeed; float _WaveFrequency; float _WaveAmplitude; float _SpiralAmount; float _WavePhase;
            CBUFFER_END

            // Populated via MaterialPropertyBlock inside our Render Feature loop
            float4 _ShadowColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                
                // Mirror the exact same UV wrapping math so the shapes sync perfectly
                float2 pivot = float2(0.5, 0.0); 
                float2 centeredUV = uv - pivot;
                float angle = _SpiralAmount * uv.y;
                float s = sin(angle); float c = cos(angle);
                float2 spiraledUV;
                spiraledUV.x = centeredUV.x * c - centeredUV.y * s;
                spiraledUV.y = centeredUV.x * s + centeredUV.y * c;
                uv = spiraledUV + pivot;

                float wave = sin(uv.y * _WaveFrequency + _WavePhase * _WaveSpeed);
                uv.x += wave * _WaveAmplitude * uv.y;

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // Flatten to your designated shadow color while preserving transparency details
                return half4(_ShadowColor.rgb, texColor.a * _ShadowColor.a * IN.color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}