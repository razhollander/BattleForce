Shader "Custom/URPSpriteVertexJiggleTiled"
{
    Properties
    {
        // Keep the mandatory hidden texture for SpriteRenderer compatibility
        [PerRendererData] [HideInInspector] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1, 1, 1, 1)
        
        // This is your custom texture that you can tile!
        _DetailTex("Tiled Texture", 2D) = "white" {}
        
        [Header(Mesh Scale Settings)]
        _MeshScale("Mesh Size Multiplier", Float) = 1.0
        
        [Header(Jiggle Settings)]
        _JiggleSpeed("Jiggle Speed", Float) = 4.0
        _JiggleScale("Jiggle Scale", Float) = 0.05
        _NoiseFrequency("Wave Frequency", Float) = 3.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SpriteForward"
            Tags { "LightMode" = "Universal2D" }

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
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float2 detailUV     : TEXCOORD1; 
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            Texture2D _DetailTex;
            SamplerState sampler_DetailTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _DetailTex_ST; 
                float _MeshScale;
                float _JiggleSpeed;
                float _JiggleScale;
                float _NoiseFrequency;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // 1. Scale the raw Object Space coordinates first
                float4 scaledPositionOS = input.positionOS;
                scaledPositionOS.xy *= _MeshScale;

                // 2. Continuous rolling wave calculations based on the scaled position
                float t = _Time.y * _JiggleSpeed;
                float waveX = sin(t + scaledPositionOS.y * _NoiseFrequency) * cos(t * 0.7 + scaledPositionOS.x);
                float waveY = cos(t * 1.2 + scaledPositionOS.x * _NoiseFrequency) * sin(t * 0.4 + scaledPositionOS.y);
                
                float2 jiggleOffset = float2(waveX, waveY);
                
                // 3. Apply the jiggle offset to the scaled position
                scaledPositionOS.xy += jiggleOffset * _JiggleScale;

                // 4. Transform to HClip space
                output.positionCS = TransformObjectToHClip(scaledPositionOS.xyz);
                
                // Pass raw sprite UV coordinates (keeps the structural alpha frame intact)
                output.uv = input.uv; 
                
                // Generate tiled UVs based on your inspector tiling inputs
                output.detailUV = TRANSFORM_TEX(input.uv, _DetailTex);
                
                output.color = input.color * _Color;
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 detailColor = SAMPLE_TEXTURE2D(_DetailTex, sampler_DetailTex, input.detailUV);
                float4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                float4 finalColor = detailColor * input.color;
                finalColor.a *= spriteColor.a; 
                
                if (finalColor.a == 0.0)
                    discard;
                    
                return finalColor;
            }
            ENDHLSL
        }
    }
}