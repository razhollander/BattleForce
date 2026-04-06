Shader "Custom/SineWaveOffsetURP"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        
        [Header(Wave Settings)]
        _WaveSpeed ("Wave Speed", Float) = 3.0
        _WaveFrequency ("Wave Frequency", Float) = 10.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.1 
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
                float4 color        : COLOR; // ADDED: Grabs the color from the SpriteRenderer
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR; // ADDED: Passes the color to the fragment shader
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
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                
                OUT.color = IN.color; // ADDED: Transfer the color data
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float wave = sin(uv.y * _WaveFrequency + _Time.y * _WaveSpeed);
                float offset = wave * _WaveAmplitude * uv.y;
                
                uv.x += offset;

                // ADDED: Multiply by IN.color so the SpriteRenderer tint is applied
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _BaseColor * IN.color;

                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}