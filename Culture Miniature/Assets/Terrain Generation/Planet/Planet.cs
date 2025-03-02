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
		#region Geometry
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
		private Mesh terrainMesh;
		void RefreshTerrainMesh()
		{
			DestroyTerrainMesh();
			terrainMesh = GenerateTerrainMesh();
			GetComponent<MeshFilter>().sharedMesh = terrainMesh;
		}
		void DestroyTerrainMesh()
		{
			if(!terrainMesh)
				return;
			GetComponent<MeshFilter>().sharedMesh = null;
			Destroy(terrainMesh);
			terrainMesh = null;
		}
		#endregion

		#region Material
		const string terrainShaderName = "Culture Miniature/Planet Terrain";
		private Material terrainMat;
		[SerializeField] private Cubemap terrainMap;
		void EnsureTerrainMat()
		{
			if(terrainMat)
				return;
			terrainMat = new Material(Shader.Find(terrainShaderName)) {
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
			RefreshTerrainMesh();
		}

		protected void OnDestroy()
		{
			if(terrainMat)
				Destroy(terrainMat);
			DestroyTerrainMesh();
		}
		#endregion
	}
}
