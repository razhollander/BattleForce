Shader "Custom/FrigidBlockTrail"
{
    Properties
    {
        _MainTex ("Trail Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _WorldUvScale ("World UV Scale", Float) = 0.5
        _WorldUvScroll ("World UV Scroll (xy)", Vector) = (0, 0, 0, 0)
        _Alpha ("Global Alpha", Range(0, 1)) = 1

        [Header(Stencil)]
        // Each trail fragment writes _StencilRef and only draws where the buffer does not
        // already hold it, so overlapping ribbon layers blend exactly once per frame
        // (no alpha accumulation / dark self-overlap). The stencil buffer is cleared with
        // depth each frame, so the guard resets automatically.
        [IntRange] _StencilRef ("Stencil Ref", Range(1, 255)) = 42
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Stencil
        {
            Ref [_StencilRef]
            Comp NotEqual
            Pass Replace
        }

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _WorldUvScale;
                float4 _WorldUvScroll;
                float _Alpha;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 worldUv    : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.color = input.color * _Color;
                // World-space UV mapping: the texture is anchored to world space, so the trail
                // slides across a static texture rather than the texture being stretched to the mesh.
                output.worldUv = positionWS.xy * _WorldUvScale + _WorldUvScroll.xy * _Time.y;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.worldUv);
                float4 color = texColor * input.color;
                color.a *= _Alpha;
                return color;
            }
            ENDHLSL
        }
    }
}
