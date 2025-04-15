Shader "Culture Miniature/Planet Water" {
	Properties {
		_Color ("Water Color", Color) = (0.2, 0.5, 0.8, 0.5)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.9
		_Metallic ("Metallic", Range(0, 1)) = 0.1
		_Transparency ("Transparency", Range(0, 1)) = 0.5
		_FresnelPower ("Fresnel Power", Range(1, 10)) = 5
		_NormalStrength ("Normal Strength", Range(0, 1)) = 0.1
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

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha:fade
		#include "UnityCG.cginc"
		#include "./Common Functions.hlsl"
		#include "./Perlin Noise.hlsl"

		fixed4 _Color;
		float _Smoothness;
		float _Metallic;
		float _Transparency;
		float _FresnelPower;
		float _NormalStrength;

		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float3 viewDir;
			float3 worldPos;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Alpha = _Transparency;
			o.Smoothness = _Smoothness;
			o.Metallic = _Metallic;

			// Fresnel effect.
			float fresnel = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), _FresnelPower);
			o.Albedo = _Color.rgb + fresnel * 0.3;

			float3 normal = float3(0, 0, 0);
			for(int i = 0; i < 5; ++i)
				normal += Perlin3D(normalize(IN.worldPos), pow(2, i + 4), 1, 137) * pow(.5, i);
			o.Normal = normalize(float3(normal.xy * _NormalStrength, 1));
		}
		ENDCG
	}

	FallBack "Transparent/Diffuse"
}
