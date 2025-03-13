Shader "Culture Miniature/Planet Terrain" {
		Properties {
				[Header(General)][Space]
				metallic ("Metallic", Range(0, 1)) = 0.1
				smoothness ("Smoothness", Range(0, 1)) = 0.4

				[Header(Tile)][Space]
				tileBaseColor ("Tile base color", Color) = (0.5, 0.5, 0.5, 1)

				[Header(Border)][Space]
				borderRatio ("Border Ratio", Range(0, 0.5)) = 0.05
				borderColor ("Border Color", Color) = (0.0, 0.0, 0.0, 1)

				[Header(Terrain)][Space]
				baseRadius ("Base radius", Range(0, 1000)) = 500
				[NoScaleOffset] terrainMap ("Terrain map", 2D) = "gray" {}
				terrainHeightScale ("Terrain height scale", Range(0, 20)) = 10
				[MaterialToggle] useBumpMapping ("Use bump-mapping", Float) = 1
				[Int] bumpMappingIteration ("Bump-mapping iteration", Range(1, 10)) = 7
				[MaterialToggle] useBakedLaplacian ("Use baked Laplacian", Float) = 1
				laplacianSampleRadiusNLog ("Laplacian sample radius negative log", Range(1, 4)) = 2
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
				#include "./Common Functions.hlsl"

				/* Properties */

				float metallic;
				float smoothness;

				float4 tileBaseColor;
				float4 tileHighlightColor;
				float tilePower;

				float borderRatio;
				float4 borderColor;

				float baseRadius;
				sampler2D terrainMap;
				float terrainHeightScale;
				float useBumpMapping;
				float bumpMappingIteration;
				float useBakedLaplacian;
				float laplacianSampleRadiusNLog;

				/* Structs */

				// Surface input.
				struct Input {
					float3 planetPos;
					float3 geographicalPos;  // (latitude, altitude, longtitude)
					float3 meshNormal;
					float centralness;
				};

				/* Auxiliary functions */

				/** Get height from a cubemap sample. */
				float GetAltitude(float4 info) {
					return 2 * (info.b - 0.5);
				}

				/** Calculate the offset caused by bump-mapping. */
				float3 BumpMap(float3 pos, float3 viewDir, float3 normalDir, float height) {
					viewDir = normalize(viewDir);
					float scale = 1 / dot(normalize(normalDir), viewDir);
					return viewDir * (scale * height);
				}

				/* Vertex program */

				void VertexProgram(inout appdata_full v, out Input o) {
					UNITY_INITIALIZE_OUTPUT(Input, o);

					float altitude = SampleTerrainHeightLocal(terrainMap, v.vertex.xyz);

					float radius = baseRadius;
					radius += altitude * terrainHeightScale;
					float3 planetPos = o.planetPos = v.vertex.xyz = v.vertex.xyz * (radius / baseRadius);
					float3 normalizedPos = normalize(planetPos);

					o.geographicalPos.x = atan2(normalizedPos.z, normalizedPos.x);
					o.geographicalPos.y = altitude;
					o.geographicalPos.z = atan2(normalizedPos.y, length(normalizedPos.zx));

					// Normal is warpped later in the surface program.
					o.meshNormal = v.normal;
					o.centralness = v.color.r;
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
							TerrainInfo terrain;
							SampleTerrainHeightFullLocal(terrainMap, visualPos, terrain);
							float bump = terrain.altitude;
							bump -= IN.geographicalPos.y;
							float3 viewDirObj = normalize(ObjSpaceViewDir(float4(IN.meshNormal, 1)));
							visualPos += step * BumpMap(visualPos, viewDirObj, IN.meshNormal, bump * terrainScale);
						}
					}

					/* Key properties */
					TerrainInfo terrain;
					SampleTerrainHeightFullLocal(terrainMap, visualPos, terrain);
					float isBorder = step(1 - IN.centralness, 1 - borderRatio);
					if(useBakedLaplacian < 0.5)
						terrain.laplacian = CalculateTerrainHeightLaplacianLocal(terrainMap, visualPos, exp(-laplacianSampleRadiusNLog));

					/* Output */
					o.Albedo = lerp(borderColor, tileBaseColor, isBorder);
					o.Albedo = float3(1, 1, 1) * ((terrain.altitude + 1) / 2);  // DEBUG
					o.Metallic = metallic;
					o.Smoothness = smoothness;
					o.Occlusion = -terrain.laplacian / 20 + 1;
					o.Alpha = 1;
				}
				ENDCG
		}
}
