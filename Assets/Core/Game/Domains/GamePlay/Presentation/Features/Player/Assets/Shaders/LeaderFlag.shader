Shader "Custom/LeaderFlag"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Toggle] _IsAttachedFromLeft ("Attached From Left", Float) = 1.0 // Added toggle

        [Header(Wave Settings)]
        _WaveSpeed ("Wave Speed", Float) = 3.0
        _WaveFrequency ("Wave Frequency", Float) = 8.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.08

        [Header(Swirl Settings)]
        _SpiralAmount ("Swirl Amount", Float) = 0.6
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
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST; 
                float4 _BaseColor; 
                float _WaveSpeed; 
                float _WaveFrequency; 
                float _WaveAmplitude; 
                float _SpiralAmount;
                float _IsAttachedFromLeft; // Added to CBUFFER
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

                // Determine if attached from left (evaluates to true if > 0.5)
                bool isLeft = _IsAttachedFromLeft > 0.5;

                // Anchor the swirl at the flag pole edge. (0.0 for left, 1.0 for right)
                float pivotX = isLeft ? 0.0 : 1.0;
                float2 pivot = float2(pivotX, 0.5);
                float2 centeredUV = uv - pivot;

                // Calculate distance from the pole based on attachment side
                float distanceFromPole = isLeft ? uv.x : (1.0 - uv.x);
                
                // Motion grows toward the free end of the flag
                float angle = _SpiralAmount * distanceFromPole * sin(_Time.y * _WaveSpeed);
                float s = sin(angle);
                float c = cos(angle);

                float2 swirledUV;
                swirledUV.x = centeredUV.x * c - centeredUV.y * s;
                swirledUV.y = centeredUV.x * s + centeredUV.y * c;
                uv = swirledUV + pivot;

                // Swapped uv.x with distanceFromPole so the wave direction mirrors correctly
                float wave = sin(distanceFromPole * _WaveFrequency + _Time.y * _WaveSpeed);
                uv.y += wave * _WaveAmplitude * distanceFromPole;

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _BaseColor * IN.color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}