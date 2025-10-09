Shader "UI/SanityGlowUnlitURP"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.65,0.0,0.8,1)
        _fill01 ("Fill 0-1", Range(0,1)) = 1
        _BandWidth ("Band Width", Range(0,1)) = 0.02
        _Softness ("Softness", Range(0,1)) = 0.02
        _GlowColor ("Glow Color", Color) = (0.85,0.2,1.0,1)
        _GlowStrength ("Glow Strength", Range(0,10)) = 0
        _InvertVertical ("Invert Vertical (0/1)", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _BaseColor;
            float _fill01;
            float _BandWidth;
            float _Softness;
            float4 _GlowColor;
            float _GlowStrength;
            float _InvertVertical;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float4 posWS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionHCS = posWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            float smoothBand(float dist, float inner, float soft)
            {
                // band = 1 - smoothstep(inner, inner+soft, dist)
                float t = saturate( (dist - inner) / max(soft, 1e-6) );
                return 1.0 - t;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Sample sprite texture (so we keep the sprite's alpha/shape)
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float4 baseCol = tex * _BaseColor * IN.color;

                // Axis along vertical by default
                float axis = uv.y;
                if (_InvertVertical > 0.5) axis = 1.0 - axis;

                float dist = abs(axis - _fill01);
                float band = smoothstep(_BandWidth, _BandWidth + _Softness, dist);
                band = 1.0 - band;

                // Optional: mask to only show glow within the "filled" area (axis <= _fill01)
                float filledMask = step(axis, _fill01);
                band *= filledMask;

                float3 glow = _GlowColor.rgb * _GlowStrength * band;

                float4 col;
                col.rgb = baseCol.rgb + glow;
                col.a   = baseCol.a;
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}