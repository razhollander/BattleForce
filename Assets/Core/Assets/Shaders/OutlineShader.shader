Shader "Custom/OutlineShader"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness ("Outline Thickness", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
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
            float4 _MainTex_TexelSize; // Automatically filled by Unity: x=1/w, y=1/h, z=w, w=h

            float4 _Color;
            float4 _OutlineColor;
            float _OutlineThickness;

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
                float4 mainCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                
                // Get the distance of one pixel scaled by thickness
                float2 offset = _MainTex_TexelSize.xy * _OutlineThickness;

                // Sample neighbors (Up, Down, Left, Right)
                float alphaUp = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, offset.y)).a;
                float alphaDown = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(0, offset.y)).a;
                float alphaLeft = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(offset.x, 0)).a;
                float alphaRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(offset.x, 0)).a;

                // Combined neighbor alpha
                float combinedAlpha = saturate(alphaUp + alphaDown + alphaLeft + alphaRight);
                
                // Only show outline where the main sprite is transparent
                float outlineMask = max(0, combinedAlpha - mainCol.a);
                
                float4 finalColor = lerp(mainCol, _OutlineColor, outlineMask);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}