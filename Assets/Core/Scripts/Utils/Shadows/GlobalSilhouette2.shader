Shader "Custom/GlobalSilhouette2"
{
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Set natively by the RenderGraph C# script
            float4 _GlobalSilhouetteOffset;
            half4 _GlobalSilhouetteColor;

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                
                // Apply the XY offset, and push the Z slightly back to avoid Z-fighting
                posWS.xy += _GlobalSilhouetteOffset.xy;
                posWS.z += _GlobalSilhouetteOffset.z;
                
                OUT.positionCS = TransformWorldToHClip(posWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target 
            { 
                return _GlobalSilhouetteColor; 
            }
            ENDHLSL
        }
    }
}