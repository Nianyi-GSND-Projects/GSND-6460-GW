Shader "Culture Miniature/BakeVertexColor"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "./Common Functions.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            v2f vert (appdata v)
            {
                v2f o;
             //  o.pos = UnityObjectToClipPos(v.vertex);
                o.pos = float4(v.uv,0.0,1.0);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }
            float4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}