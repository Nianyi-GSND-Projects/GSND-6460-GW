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
		private RenderTexture heightMap;
		public void CreateHeightMap()
		{
			if(heightMap)
				return;
			heightMap = GenerateHeightMap();
			if(terrainMat)
				terrainMat.SetTexture("heightMap", heightMap);
		}
		void DestroyHeightMap()
		{
			if(!heightMap)
				return;
			RenderTexture.ReleaseTemporary(heightMap);
			heightMap = null;
		}
		#endregion

		#region Terrain rendering
		private Material terrainMat;
		public Material TerrainMat => terrainMat;
		void EnsureTerrainMat()
		{
			if(terrainMat)
				return;
			terrainMat = new Material(Shader.Find(terrainShaderName))
			{
				name = "Planet Terrain (instance)",
			};
			if(heightMap)
				terrainMat.SetTexture("heightMap", heightMap);
			var renderer = GetComponent<MeshRenderer>();
			renderer.sharedMaterial = terrainMat;
		}
		#endregion

		#region Untiy life cycle
		protected void Start()
		{
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
