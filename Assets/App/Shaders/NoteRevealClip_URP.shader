Shader "Custom/NoteRevealClip_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RevealY ("Reveal Y Threshold", Float) = 0
        _RevealDir ("Reveal Direction (1 or -1)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _RevealY;
                float _RevealDir;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float localY : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.localY = IN.positionOS.y; // local space Y, giống bản Built-in
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float signedY = IN.localY * _RevealDir;
                float signedThreshold = _RevealY * _RevealDir;
                clip(signedThreshold - signedY); // âm -> discard pixel

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return col;
            }
            ENDHLSL
        }
    }
}