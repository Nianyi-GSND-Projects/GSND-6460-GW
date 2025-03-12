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
		[SerializeField][Range(0, 5)] private int subdivisionIteration = 3;
		private Mesh planetMesh;
		#endregion

		#region Terrain map
		const string terrainShaderName = "Culture Miniature/Planet Terrain";
		[SerializeField] private Texture debugTerrainMap;
		private RenderTexture terrainMap;
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
			terrainMat.SetTexture("terrainMap", terrainMap);
			var renderer = GetComponent<MeshRenderer>();
			renderer.sharedMaterial = terrainMat;
		}
		#endregion

		#region Untiy life cycle
		protected void Start()
		{
			EnsureTerrainMat();
			Radius = Radius;

			// Planet mesh
			planetMesh = GenerateTerrainMesh();
			GetComponent<MeshFilter>().sharedMesh = planetMesh;
		}

		protected void OnDestroy()
		{
			if(terrainMat)
				Destroy(terrainMat);

			// Planet mesh
			GetComponent<MeshFilter>().sharedMesh = null;
			Destroy(planetMesh);
			planetMesh = null;
		}
		#endregion
	}
}
