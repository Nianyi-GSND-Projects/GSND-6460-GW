Shader "Culture Miniature/Planet Water" {
	Properties {
		_Color ("Water Color", Color) = (0.2, 0.5, 0.8, 0.5)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.9
		_Metallic ("Metallic", Range(0, 1)) = 0.1
		_Transparency ("Transparency", Range(0, 1)) = 0.5
		_FresnelPower ("Fresnel Power", Range(1, 10)) = 5
	}

	SubShader {
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		LOD 200
		Cull Back
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		GrabPass { "_GrabTexture" }

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha:fade
		#include "UnityCG.cginc"
		#include "./Common Functions.hlsl"
		#include "./Perlin Noise.hlsl"

		// Grab-passing depth info.
		sampler2D _GrabTexture;
		sampler2D _CameraDepthTexture;
		float4 _GrabTexture_TexelSize;

		fixed4 _Color;
		float _Smoothness;
		float _Metallic;
		float _Transparency;
		float _FresnelPower;

		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float3 viewDir;
			float3 worldPos;
			float4 screenPos;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Alpha = _Transparency;
			o.Smoothness = _Smoothness;
			o.Metallic = _Metallic;

			// Fresnel effect.
			float fresnel = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), _FresnelPower);
			o.Albedo = _Color.rgb + fresnel * 0.3;
		}
		ENDCG
	}

	FallBack "Transparent/Diffuse"
}
