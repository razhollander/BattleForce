Shader "Custom/StylizedLavaWorldUV_Stretch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Lava Noise / Sprite", 2D) = "white" {}
        _ColorDeep ("Deep Color", Color) = (0.5, 0, 0, 1)
        _ColorBright ("Bright Color", Color) = (1, 0.5, 0, 1)
        _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0.05, 0.1, -0.05)
        _Stretch ("World Stretch (X, Y)", Vector) = (1, 1, 0, 0) // New Stretch Control
        _Bands ("Color Bands", Float) = 4.0
        _Emission ("Emission Intensity", Float) = 2.0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha 

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON

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
                float2 worldUV      : TEXCOORD1;     
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorDeep;
            float4 _ColorBright;
            float4 _ScrollSpeed;
            float2 _Stretch;
            float _Bands;
            float _Emission;

            Varyings vert (Attributes input)
            {
                Varyings output;
                
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;

                // Apply the X and Y stretch to the world position coordinates
                output.worldUV = worldPos.xy * _Stretch;

                #ifdef PIXELSNAP_ON
                output.positionCS = UnityPixelSnap(output.positionCS);
                #endif

                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                // Calculate scrolling using the stretched worldUV
                float2 uv1 = frac(input.worldUV + _ScrollSpeed.xy * _Time.y);
                float2 uv2 = frac(input.worldUV - _ScrollSpeed.zw * _Time.y);

                float noise1 = tex2D(_MainTex, uv1).r;
                float noise2 = tex2D(_MainTex, uv2).r;
                
                float maskAlpha = tex2D(_MainTex, input.uv).a * input.color.a;

                float combinedNoise = (noise1 + noise2) * 0.5;
                float bandedNoise = floor(combinedNoise * _Bands) / _Bands;

                float3 lavaColor = lerp(_ColorDeep.rgb, _ColorBright.rgb, bandedNoise);
                lavaColor *= _Emission;

                float4 finalColor = float4(lavaColor, maskAlpha);
                finalColor.rgb *= finalColor.a; 

                return finalColor;
            }
            ENDHLSL
        }
    }
}