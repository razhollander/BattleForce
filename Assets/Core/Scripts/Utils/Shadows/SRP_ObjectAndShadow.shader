Shader "Custom/2D/SRP_ObjectAndShadow"
{
    Properties
    {
        _BaseColor ("Object Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.5)
        _ShadowOffset ("Shadow Offset (World Space)", Vector) = (0.1, -0.1, 0, 0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }

        // ==================================================
        // PASS 1: THE MAIN OBJECT
        // ==================================================
        Pass
        {
            Name "MainObject"
            // URP automatically looks for and draws this LightMode
            Tags { "LightMode" = "Universal2D" } 
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // SRP BATCHER CBUFFER
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                float4 _ShadowOffset;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // Draw the object exactly where it is
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target 
            { 
                return _BaseColor; 
            }
            ENDHLSL
        }

        // ==================================================
        // PASS 2: THE SHADOW SILHOUETTE
        // ==================================================
        Pass
        {
            Name "ShadowSilhouette"
            // This is a custom tag. URP will ignore this pass until we 
            // tell it to look for "Custom2DShadow" in the Renderer Feature.
            Tags { "LightMode" = "Custom2DShadow" } 
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // The CBUFFER must be identical in both passes for the batcher
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                float4 _ShadowOffset;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                
                // Offset the shadow in world space
                positionWS += _ShadowOffset.xyz;
                
                OUT.positionCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target 
            { 
                return _ShadowColor; 
            }
            ENDHLSL
        }
    }
}