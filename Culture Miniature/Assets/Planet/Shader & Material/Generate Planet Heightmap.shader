Shader "Culture Miniature/Generate Planet Heightmap" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		[Int] frequency ("Frequency", Float) = 1
		amplitude ("Amplitude", Float) = 1
		[Int] seed ("Seed", Float) = 137
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always
		Pass {
			CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			#include "UnityCG.cginc"
			#include "./Common Functions.hlsl"
			#include "./Perlin Noise.hlsl"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f VertexProgram(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float frequency;
			float amplitude;
			float seed;

			float4 FragmentProgram(v2f i) : SV_Target {
				float v = tex2D(_MainTex, float2(i.uv)).a;
				float3 local = Geo2Local(Uv2Geo(i.uv));
				v += Perlin3D(
					local,
					frequency,
					amplitude,
					(uint)seed
				);
				return float4(1, 1, 1, 1) * v;
			}
			ENDCG
		}
	}
}
