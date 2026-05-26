Shader "Custom/SpriteShadowInstanced"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.5)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // CRITICAL: Enables instancing variants

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // CRITICAL
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // CRITICAL
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            float4 _ShadowColor;

            // Arrays passed from our MaterialPropertyBlock
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_UVs) // Fixed: Using the correct Unity macro
            UNITY_INSTANCING_BUFFER_END(Props)

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                // Remap the basic quad UVs to the specific sprite's UVs inside the atlas
                float4 uvRange = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UVs);
                output.uv = lerp(uvRange.xy, uvRange.zw, input.uv);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Sample alpha to retain the shape of the sprite
                float4 texColor = _MainTex.Sample(sampler_MainTex, input.uv);
                
                // Return shadow color modulated by sprite opacity
                return float4(_ShadowColor.rgb, texColor.a * _ShadowColor.a);
            }
            ENDHLSL
        }
    }
}