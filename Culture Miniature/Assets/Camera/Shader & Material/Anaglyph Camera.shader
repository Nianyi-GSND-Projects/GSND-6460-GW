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
				float4 color = tex2D(_MainTex, uv);
				// Formula for luminance.
				// Source: https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
				float value = dot(float3(0.299, 0.587, 0.114), color * color);
				return tex2D(_ScreenTex, uv) + value * lerp(color, _Color, 0.8);
			}
			ENDCG
		}
	}
}
