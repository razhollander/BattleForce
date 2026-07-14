Shader "Custom/TalentActiveEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _RingColor ("Ring Color", Color) = (0.35, 0.85, 1, 1)
        [HDR] _GlowColor ("Glow Color", Color) = (0.15, 0.55, 1, 1)

        _Radius ("Ring Radius", Range(0, 0.5)) = 0.42
        _RingThickness ("Ring Thickness", Range(0.001, 0.3)) = 0.05
        _GlowFalloff ("Glow Falloff", Range(0.001, 0.5)) = 0.25

        _RotationSpeed ("Rotation Speed (rev/sec)", Range(-3, 3)) = 0.6
        _DashCount ("Dash Count", Range(0, 64)) = 12
        _DashSharpness ("Dash Sharpness", Range(0.01, 1)) = 0.35

        _PulseSpeed ("Pulse Speed (cycles/sec)", Range(0, 6)) = 2
        _PulseAmount ("Pulse Amount", Range(0, 0.15)) = 0.04

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
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            #define TWO_PI 6.28318530718

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _RingColor;
            fixed4 _GlowColor;
            float _Radius;
            float _RingThickness;
            float _GlowFalloff;
            float _RotationSpeed;
            float _DashCount;
            float _DashSharpness;
            float _PulseSpeed;
            float _PulseAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Centered coordinates: center of the quad is (0,0), edges are at +-0.5.
                float2 centered = IN.texcoord - 0.5;
                float dist = length(centered);
                float angle = atan2(centered.y, centered.x); // -PI..PI

                float time = _Time.y;

                // Pulse the ring radius so the whole circle gently breathes.
                float pulse = sin(time * _PulseSpeed * TWO_PI) * _PulseAmount;
                float radius = _Radius + pulse;

                // Soft ring band centered on the pulsing radius.
                float ringBand = 1.0 - smoothstep(0.0, _RingThickness, abs(dist - radius));

                // Rotating dashes around the ring: a spinning dashed outline.
                float rotatedAngle = angle + time * _RotationSpeed * TWO_PI;
                float dashWave = 0.5 + 0.5 * cos(rotatedAngle * _DashCount);
                float dashes = smoothstep(_DashSharpness, 1.0, dashWave);
                float dashMask = lerp(1.0, dashes, step(0.5, _DashCount));

                float ring = ringBand * dashMask;

                // Inner glow that fades out from the ring toward the center.
                float glow = (1.0 - smoothstep(0.0, _GlowFalloff, abs(dist - radius))) * 0.5;

                float3 rgb = _RingColor.rgb * ring + _GlowColor.rgb * glow;
                float alpha = saturate(ring * _RingColor.a + glow * _GlowColor.a);

                fixed4 result = fixed4(rgb, alpha) * IN.color;
                return result;
            }
        ENDCG
        }
    }
}
