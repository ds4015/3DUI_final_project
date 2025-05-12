Shader "Hidden/URP_InvertOutline"
{
    Properties
    {
        _Color      ("Outline Color", Color) = (1,1,0,1)
        _Thickness  ("Push Factor", Float) = 0.03
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" }
        Pass
        {
            Name      "Outline"
            Tags      { "LightMode"="UniversalForward" }
            Cull Front
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };
            struct Varyings { float4 positionCS : SV_POSITION; };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float  _Thickness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 p = IN.positionOS + IN.normalOS * _Thickness;
                OUT.positionCS = TransformObjectToHClip(p);
                return OUT;
            }
            half4 frag(Varyings IN) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
