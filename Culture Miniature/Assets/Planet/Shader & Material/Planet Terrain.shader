Shader "Culture Miniature/Planet Terrain" {
		Properties {

				[Header(Mesh)][Space]
				baseRadius ("Base radius", Range(0, 1000)) = 500
				subdivisionLevel ("Subdivision level", Range(2, 6)) = 5

				[Header(General)][Space]
				metallic ("Metallic", Range(0, 1)) = 0
				smoothness ("Smoothness", Range(0, 1)) = 0

				[Header(Debug)][Space]
				centralnessLerp ("Centralness Lerp", Range(0, 1)) = 0

				[Header(Tile)][Space]
				latitudePower ("Latitude power", Range(-2, 2)) = 0.5
				tileBaseColor ("Tile base color", Color) = (0.8, 0.7, 0.3, 1)
				sandHeight ("Sand height", Range(-1, 1)) = 0.2
				sandColor ("Sand color", Color) = (1, 0.8, 0.5, 1)
				dirtColor ("Dirt color", Color) = (0.8, 0.5, 0.2, 1)
				grassColor ("Grass color", Color) = (0.5, 1, 0.2, 1)
				grassHeight ("Grass height", Range(-1, 1)) = 0.6
				iceColor ("Ice color", Color) = (0.9, 0.95, 1, 1)
				iceHeight ("Ice height", Range(-1, 1)) = 0.8

				[Header(Border)][Space]
				borderRatio ("Border Ratio", Range(0, 0.5)) = 0.02
				borderBaseColor ("Border Base Color", Color) = (0.0, 0.0, 0.0, 1)
				borderFocusedColor ("Border Focused Color", Color) = (1.0, 1.0, 1.0, 1)
				borderEmissionIntensity ("Border Emission Intensity", Range(0, 1)) = 0.1

				[Header(Focus)][Space]
				focusRadius ("Focus Radius", Float) = 100
				[MaterialToggle] useFocus ("Use Focus", Float) = 0
				focusPosition ("Focus Position", Vector) = (0, 0, 0, 0)
				focusGradientPower ("Focus Gradient Power", Range(0, 1)) = 1

				[Header(Height map)][Space]
				[NoScaleOffset] heightMap ("Height map", 2D) = "gray" {}
				heightScale ("Terrain height scale", Range(0, 100)) = 10
				[MaterialToggle] useBumpMapping ("Use bump-mapping", Float) = 1
				[Int] bumpMappingIteration ("Bump-mapping iteration", Range(1, 10)) = 7
				[MaterialToggle] useBakedLaplacian ("Use baked Laplacian", Float) = 0
				laplacianStrength ("Laplacian strength", Range(0, 1)) = 0.01
				normalStrength ("Normal strength", Range(0, 4)) = 1
		}
		SubShader {
				Tags {
					"RenderType" = "Opaque"
				}
				LOD 200
				Cull Back

				CGPROGRAM
				#pragma surface SurfaceProgram Standard fullforwardshadows vertex:VertexProgram
				#pragma target 4.0
				#include "UnityCG.cginc"
				#include "./Common Functions.hlsl"

				/* Properties */

				float baseRadius;
				float subdivisionLevel;

				float metallic;
				float smoothness;

				float centralnessLerp;

				float4 tileBaseColor;
				float latitudePower;
				float4 sandColor;
				float sandHeight;
				float4 dirtColor;
				float4 grassColor;
				float grassHeight;
				float4 iceColor;
				float iceHeight;

				float borderRatio;
				float4 borderBaseColor;
				float4 borderFocusedColor;
				float focusGradientPower;

				float focusRadius;
				float useFocus;
				float4 focusPosition;
				float borderEmissionIntensity;

				sampler2D heightMap;
				float heightScale;
				float useBumpMapping;
				float bumpMappingIteration;
				float useBakedLaplacian;
				float laplacianStrength;
				float normalStrength;

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

					TerrainInfo terrain;
					SampleHeight_Local(heightMap, v.vertex.xyz, terrain);

					float radius = baseRadius;
					radius += terrain.altitude * heightScale;
					float3 planetPos = o.planetPos = v.vertex.xyz = v.vertex.xyz * (radius / baseRadius);
					float3 normalizedPos = normalize(planetPos);

					o.geographicalPos.x = atan2(normalizedPos.z, normalizedPos.x);
					o.geographicalPos.y = terrain.altitude;
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
					float terrainScale = heightScale / baseRadius;

					/* Bump mapping */
					float3 visualPos = IN.planetPos;
					if(!useBumpMapping)
						visualPos = IN.planetPos;
					else {
						float step = pow(0.9, bumpMappingIteration);
						for(int i = 0; i < bumpMappingIteration; ++i) {
							TerrainInfo terrain;
							SampleHeight_Local(heightMap, visualPos, terrain);
							float bump = terrain.altitude;
							bump -= IN.geographicalPos.y;
							float3 viewDirObj = normalize(ObjSpaceViewDir(float4(IN.meshNormal, 1)));
							visualPos += step * BumpMap(visualPos, viewDirObj, IN.meshNormal, bump * terrainScale);
						}
					}

					/* Center position */
					float3 centerPos = normalize(IN.meshNormal);
					visualPos = lerp(visualPos, centerPos, centralnessLerp);

					/* Key properties */
					TerrainInfo terrain;
					SampleHeight_Local(heightMap, visualPos, terrain);

					// Complex terrain info.
					if(useBakedLaplacian < 0.5)
						terrain.laplacian = CalculateHeightLaplacianLayered_Local(heightMap, terrain, (int)subdivisionLevel + 1);
					terrain.gradient = CalculateHeightGradient_Geo(heightMap, Local2Geo(visualPos), subdivisionLevel);

					// Terrain type.
					float terrainValue = terrain.altitude;
					terrainValue = lerp(terrainValue, 1, pow(abs(IN.planetPos.y), exp(latitudePower)));

					// Terrain color.
					float3 tileColor = tileBaseColor;
					tileColor = lerp(tileColor, sandColor, step(0, terrainValue));
					tileColor = lerp(tileColor, grassColor, step(sandHeight, terrainValue));
					tileColor = lerp(tileColor, dirtColor, step(grassHeight, terrainValue));
					tileColor = lerp(tileColor, iceColor, step(iceHeight, terrainValue));

					// Border.
					float isBorder = step(IN.centralness, borderRatio);
					float focusedness = 1 - clamp(distance(IN.planetPos * baseRadius, focusPosition) / focusRadius, 0, 1);
					float3 borderColor = lerp(borderBaseColor, borderFocusedColor, pow(focusedness, focusGradientPower));

					/* Output */
					o.Albedo = lerp(tileColor, borderColor, isBorder);
					o.Albedo = float3(1, 1, 1) * (terrain.altitude * .5 + .5);  // DEBUG
					// o.Emission = borderColor * isBorder * borderEmissionIntensity;
					o.Normal = CalculateTangentSpaceNormal(terrain, normalStrength);
					o.Metallic = metallic;
					o.Smoothness = smoothness;
					o.Occlusion = 1 + clamp(-terrain.laplacian * laplacianStrength, -1, 1);
					o.Alpha = 1;
				}
				ENDCG
		}
}
