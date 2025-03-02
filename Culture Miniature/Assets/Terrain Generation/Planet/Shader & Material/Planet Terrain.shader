Shader "Culture Miniature/Planet Terrain" {
		Properties {
				baseRadius ("Base radius", Range(0, 1000)) = 500
				[NoScaleOffset] terrainMap ("Terrain map", Cube) = "black" {}
				terrainHeightScale ("Terrain height scale", Range(0, 500)) = 10
				[MaterialToggle] useBumpMapping ("Use bump-mapping", Float) = 0
				[Int] bumpMappingIteration ("Bump-mapping iteration", Range(1, 10)) = 1
		}
		SubShader {
				Tags {
					"RenderType" = "Opaque"
				}
				LOD 200

				CGPROGRAM
				#pragma surface SurfaceProgram Standard fullforwardshadows vertex:VertexProgram
				#pragma target 4.0
				#include "UnityCG.cginc"

				/* Properties */

				float baseRadius;
				UNITY_DECLARE_TEXCUBE(terrainMap);
				float terrainHeightScale;
				float useBumpMapping;
				float bumpMappingIteration;

				/* Structs */

				// Surface input.
				struct Input {
					float3 planetPos;
					float3 geographicalPos;  // (latitude, altitude, longtitude)
					float3 meshNormal;
				};

				/* Auxiliary functions */

				/** Get height from a cubemap sample. */
				float GetHeight(float4 info) {
					return info.b - 0.5;
				}

				/** Calculate the offset caused by bump-mapping. */
				float3 BumpMap(float3 pos, float3 viewDir, float3 normalDir, float height) {
					viewDir = normalize(viewDir);
					float scale = 1 / dot(normalize(normalDir), viewDir);
					return viewDir * (scale * height);
				}

				/* Vertex program */

				void VertexProgram(inout appdata_full v, out Input o) {
					float radius = baseRadius;
					float altitude = GetHeight(UNITY_SAMPLE_TEXCUBE_LOD(terrainMap, v.vertex.xyz, 0));
					radius += altitude * terrainHeightScale;
					float3 planetPos = o.planetPos = v.vertex.xyz = v.vertex.xyz * (radius / baseRadius);
					float3 normalizedPos = normalize(planetPos);

					o.geographicalPos.x = atan2(normalizedPos.z, normalizedPos.x);
					o.geographicalPos.y = altitude;
					o.geographicalPos.z = atan2(normalizedPos.y, length(normalizedPos.zx));

					// TODO: Update the normal to match the tweaked shape.
					o.meshNormal = v.normal;
				}

				/* Surface program */

				/*
				struct SurfaceOutputStandard {
						fixed3 Albedo;      // base (diffuse or specular) color
						fixed3 Normal;      // tangent space normal, if written
						half3 Emission;
						half Metallic;      // 0=non-metal, 1=metal
						half Smoothness;    // 0=rough, 1=smooth
						half Occlusion;     // occlusion (default 1)
						fixed Alpha;        // alpha for transparencies
				};
				*/

				void SurfaceProgram(Input IN, inout SurfaceOutputStandard o) {
					/* Pre-process */
					float terrainScale = terrainHeightScale / baseRadius;

					/* Bump mapping */
					float3 visualPos = IN.planetPos;
					if(!useBumpMapping)
						visualPos = IN.planetPos;
					else {
						float step = pow(0.9, bumpMappingIteration);
						for(int i = 0; i < bumpMappingIteration; ++i) {
							float bump = GetHeight(UNITY_SAMPLE_TEXCUBE(terrainMap, visualPos));
							bump -= IN.geographicalPos.y;
							float3 viewDirObj = normalize(ObjSpaceViewDir(float4(IN.meshNormal, 1)));
							visualPos += step * BumpMap(visualPos, viewDirObj, IN.meshNormal, bump * terrainScale);
						}
					}

					/* Output */
					o.Albedo = float3(1, 1, 1) * (GetHeight(UNITY_SAMPLE_TEXCUBE(terrainMap, visualPos)) + .5);
					o.Metallic = 0;
					o.Smoothness = 0;
					o.Occlusion = 1;
					o.Alpha = 1;
				}
				ENDCG
		}
}
