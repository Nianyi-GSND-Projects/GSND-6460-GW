using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CultureMiniature
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(SphereCollider))]
	public partial class Planet : MonoBehaviour
	{
		#region Constants
		private static int layerMask;
		public static int LayerMask
		{
			get
			{
				if(layerMask == 0)
					layerMask = UnityEngine.LayerMask.GetMask("Planet");
				return layerMask;
			}
		}
		#endregion

		#region Planet geometry
		[SerializeField] private float radius = 500;
		public float Radius
		{
			get => radius;
			set
			{
				if(value <= 0)
					throw new UnityException("The radius of a planet must be positive.");

				radius = value;

				// Reset the sphere collider.
				var sphere = GetComponent<SphereCollider>();
				sphere.radius = 1f;
				sphere.center = Vector3.zero;

				// Set the transform.
				transform.localScale = Vector3.one * radius;

				// Update the terrain material.
				EnsureTerrainMat();
				terrainMat.SetFloat("baseRadius", radius);
			}
		}
		[Range(0, 5)] public int debugSubdivisionLevel = 3;
		private Mesh planetMesh;
		private int subdivisionLevel;
		public int SubdivisionLevel => subdivisionLevel;

		void UpdatePlanetMesh(Mesh mesh)
		{
			if(planetMesh != null)
			{
				Destroy(planetMesh);
				planetMesh = null;
			}
			planetMesh = mesh;
			GetComponent<MeshFilter>().sharedMesh = planetMesh;
		}
		#endregion

		#region Terrain map
		const string terrainShaderName = "Culture Miniature/Planet Terrain";
		[SerializeField] private Texture debugHeightMap;
		private RenderTexture heightMap;
		void CreateHeightMap()
		{
			heightMap = new(new RenderTextureDescriptor
			{
				width = 2048,
				height = 1024,
				colorFormat = RenderTextureFormat.ARGB32,
				useMipMap = false,
				dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
				volumeDepth = 1,
				msaaSamples = 1,
			})
			{
				wrapMode = TextureWrapMode.Repeat
			};
		}
		void DestroyHeightMap()
		{
			Destroy(heightMap);
			heightMap = null;
		}
		#endregion

		#region Terrain rendering
		private Material terrainMat;
		void EnsureTerrainMat()
		{
			if(terrainMat)
				return;
			terrainMat = new Material(Shader.Find(terrainShaderName))
			{
				name = "Planet Terrain (instance)",
			};
			terrainMat.SetTexture("heightMap", heightMap);
			var renderer = GetComponent<MeshRenderer>();
			renderer.sharedMaterial = terrainMat;
		}
		#endregion

		#region Untiy life cycle
		protected void Start()
		{
			CreateHeightMap();
#if DEBUG
			Graphics.Blit(debugHeightMap, heightMap);
#endif
			EnsureTerrainMat();
			Radius = Radius;

			UpdatePlanetMesh(null);
		}

		protected void OnDestroy()
		{
			if(terrainMat)
				Destroy(terrainMat);
			DestroyHeightMap();

			// Planet mesh
			GetComponent<MeshFilter>().sharedMesh = null;
			Destroy(planetMesh);
			planetMesh = null;
		}
		#endregion

		#region Focus
		public bool UseFocus
		{
			set
			{
				if(!terrainMat)
					return;
				terrainMat.SetFloat("useFocus", value ? 1 : 0);
			}
		}

		public Vector3 FocusPosition
		{
			set
			{
				if(!terrainMat)
					return;
				terrainMat.SetVector("focusPosition", value);
			}
		}
		#endregion
	}

}
