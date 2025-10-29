Shader "Custom/DissolveURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Base Map", 2D) = "white" {}
        _NoiseMap ("Noise Map", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeColor ("Edge Color", Color) = (1,0.5,0,1)
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _BaseMap;
            sampler2D _NoiseMap;
            float4 _BaseColor;
            float _DissolveAmount;
            float _EdgeWidth;
            float4 _EdgeColor;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 baseColor = tex2D(_BaseMap, IN.uv) * _BaseColor;
                half noise = tex2D(_NoiseMap, IN.uv).r;

                // Dissolve logic
                half dissolve = smoothstep(_DissolveAmount - _EdgeWidth, _DissolveAmount + _EdgeWidth, noise);
                half edge = step(_DissolveAmount, noise) * step(noise, _DissolveAmount + _EdgeWidth);

                half4 finalColor = baseColor;
                finalColor.rgb = lerp(_EdgeColor.rgb, baseColor.rgb, dissolve);
                finalColor.a = dissolve;

                clip(finalColor.a - 0.01); // Cut off invisible pixels
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
