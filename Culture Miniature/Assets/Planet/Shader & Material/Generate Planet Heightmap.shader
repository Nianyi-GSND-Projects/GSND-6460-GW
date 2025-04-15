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

			float GradientBasedAttenuation(in float gradient) {
				return 1 / (1 + 0.5 * gradient);
			}

			float3 ProjectOntoPlane(in float3 v, in float3 n) {
				n = normalize(n);
				return v - n * dot(v, n);
			}

			float4 FragmentProgram(v2f i) : SV_Target {
				float2 geo = Uv2Geo(i.uv);
				float3 local = Geo2Local(geo);

				float oldValue = tex2D(_MainTex, float2(i.uv)).a;
				float oldGradient = length(CalculateHeightGradient_Geo(_MainTex, geo, 7));
				float newValue = Perlin3D(local, frequency, amplitude, (uint)seed);
				float newGradient = length(ProjectOntoPlane(Perlin3D_Gradient(local, frequency, amplitude, (uint)seed), local));

				float value = oldValue + newValue * GradientBasedAttenuation(oldGradient + newGradient);
				return value * float4(1, 1, 1, 1);
			}
			ENDCG
		}
	}
}
