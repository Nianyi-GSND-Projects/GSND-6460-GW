Shader "Culture Miniature/Anaglyph Camera"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ScreenTex ("Screen Texture", 2D) = "black" {}

		_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _ScreenTex;

			float4 _Color;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
				return tex2D(_ScreenTex, uv) + tex2D(_MainTex, uv) * _Color;
			}
			ENDCG
		}
	}
}
