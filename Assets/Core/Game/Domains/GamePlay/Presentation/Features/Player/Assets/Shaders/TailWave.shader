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
        
        [Header(Spiral Settings)]
        // This is the property you will control from your script
        _SpiralAmount ("Spiral Amount", Float) = 0.0 
        _MoveSpeedMultiplier ("Move Speed Multiplier", Range(0, 1)) = 1.0
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

        Pass
        {
            Name "Unlit"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _WaveSpeed;
                float _WaveFrequency;
                float _WaveAmplitude;
                float _SpiralAmount; // Replaced Bend with Spiral
                float _MoveSpeedMultiplier;
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

                // --- 1. THE SPIRAL MATH ---
                // Set the pivot point at the bottom center of the sprite
                float2 pivot = float2(0.5, 0.0); 
                
                // Shift UVs so the pivot is temporarily at (0,0) for rotation
                float2 centeredUV = uv - pivot;
                
                // Calculate the rotation angle. 
                // The higher the Y coordinate, the stronger the twist.
                float angle = _SpiralAmount * uv.y;
                
                // Standard 2D rotation matrix math
                float s = sin(angle);
                float c = cos(angle);
                
                float2 spiraledUV;
                spiraledUV.x = centeredUV.x * c - centeredUV.y * s;
                spiraledUV.y = centeredUV.x * s + centeredUV.y * c;
                
                // Shift the UVs back to their original position
                uv = spiraledUV + pivot;

                // --- 2. THE SINE WAVE ---
                // We apply the wave AFTER the spiral, so the ripples 
                // naturally follow the curve of the curled tail.
                float wave = sin(uv.y * _WaveFrequency + _Time.y * _WaveSpeed * _MoveSpeedMultiplier);
                float waveOffset = wave * _WaveAmplitude * uv.y;
                uv.x += waveOffset;

                // Sample the texture with our heavily distorted UVs
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _BaseColor * IN.color;

                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}