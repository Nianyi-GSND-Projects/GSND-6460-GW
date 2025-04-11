using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
		[Header("Geometry")]
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

		#region Heightmap
		[Header("Heightmap")]
		[SerializeField] private int noiseSeed = 137;
		const string terrainShaderName = "Culture Miniature/Planet Terrain";
		private RenderTexture heightMap;
		void CreateHeightMap()
		{
			if(heightMap)
				return;

			heightMap = RenderTexture.GetTemporary(2048, 1024, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			heightMap.enableRandomWrite = true;
			heightMap.wrapModeU = TextureWrapMode.Repeat;
			heightMap.wrapModeV = TextureWrapMode.Mirror;

			if(terrainMat)
				terrainMat.SetTexture("heightMap", heightMap);
		}
		[Range(1, 10)][SerializeField] private int maxNoiseLevel = 5;
		[Range(0, 5)][SerializeField] private int noiseFrequencyLevel = 2;
		[Range(0.1f, 0.9f)][SerializeField] private float noisePower = 0.5f;
		void LayerHeightMap(int level)
		{
			RenderTexture temp = RenderTexture.GetTemporary(heightMap.descriptor);
			Graphics.Blit(heightMap, temp);
			Material mat = new(Shader.Find("Culture Miniature/Generate Planet Heightmap"));
			mat.SetTexture("_MainTex", temp);
			float frequency = Mathf.Pow(2f, level + noiseFrequencyLevel);
			mat.SetFloat("frequency", frequency);
			float amplitude = Mathf.Pow(noisePower, level);
			mat.SetFloat("amplitude", amplitude);
			mat.SetFloat("seed", noiseSeed);
			Graphics.Blit(temp, heightMap, mat);
			Destroy(mat);
			RenderTexture.ReleaseTemporary(temp);
		}
		void DestroyHeightMap()
		{
			if(!heightMap)
				return;
			RenderTexture.ReleaseTemporary(heightMap);
			heightMap = null;
		}
		#endregion

		#region Baking
		protected void BakingTexture(string ShaderName, RenderTexture TargetTexture, Mesh mesh, Material material)
		{

			var shader = Shader.Find(ShaderName);
			if(!shader)
			{
				Debug.LogError($"Shader \"{ShaderName}\" not found!");
				return;
			}

			if(material == null) material = new Material(shader);

			RenderTexture temp = RenderTexture.active;
			RenderTexture.active = TargetTexture;

			GL.Clear(true, true, Color.black);
			material.SetPass(0);
			Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
			RenderTexture.active = temp;
		}

		#endregion

		#region HexColor
		RenderTexture HexColortex;
		[Header("Hex color")]
		[SerializeField] private bool bakeHexColor = false;
		public RawImage testOutput;
		void CreateHexColorMap()
		{
			if(HexColortex)
				return;

			HexColortex = new RenderTexture(2048, 1024, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			HexColortex.enableRandomWrite = true;
			HexColortex.Create();

			//HexColortex.wrapModeU = TextureWrapMode.Repeat;
			//HexColortex.wrapModeV = TextureWrapMode.Mirror;

		}
		void BakeHexColor()
		{
			BakingTexture("Culture Miniature/BakeVertexColor", HexColortex, planetMesh, null);
			testOutput.texture = HexColortex;
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
			noiseSeed = (int)(0xffff * Random.value);

			EnsureTerrainMat();
			Radius = Radius;

			UpdatePlanetMesh(null);
		}

		protected void OnDestroy()
		{
			DestroyWater();

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

		#region Generation
		[Header("Generation")]
		[Range(0, 1)] public float generationDelay = 0.1f;
		[Range(0, 5)] public float generationInterval = 1f;
		public IEnumerator GenerationCoroutine()
		{
			yield return new WaitForSeconds(generationInterval);

			PlayVFX(dirtEffect);
			yield return new WaitForSeconds(generationDelay);
			CreateMesh();
			yield return new WaitForSeconds(generationInterval);

			for(int i = 0; i < debugSubdivisionLevel; ++i)
			{
				PlayVFX(dirtEffect);
				yield return new WaitForSeconds(generationDelay);
				SubdivideMesh();
				yield return new WaitForSeconds(generationInterval);
			}
			FinalizeMesh();

			PlayVFX(dirtEffect);
			yield return new WaitForSeconds(generationDelay);
			CreateHeightMap();
			yield return new WaitForSeconds(generationInterval);

			PlayVFX(dirtEffect);
			yield return new WaitForSeconds(generationDelay);
			CreateWater();
			yield return new WaitForSeconds(generationInterval);

			for(int i = 0; i <= maxNoiseLevel; ++i)
			{
				PlayVFX(dirtEffect);
				yield return new WaitForSeconds(generationDelay);
				LayerHeightMap(i);
				yield return new WaitForSeconds(generationInterval);
			}

			yield return new WaitForSeconds(generationDelay);
			CreateHexColorMap();
			yield return new WaitForSeconds(generationInterval);

			if(bakeHexColor)
			{
				yield return new WaitForSeconds(generationDelay);
				BakeHexColor();
				yield return new WaitForSeconds(generationInterval);
			}
		}
		#endregion

		#region VFX
		[SerializeField] private ParticleSystem dirtEffect;

		void PlayVFX(ParticleSystem ps)
		{
			StartCoroutine(PlayVFXCoroutine(ps));
		}

		IEnumerator PlayVFXCoroutine(ParticleSystem ps)
		{
			ps.Play();
			yield return new WaitForSeconds(ps.main.duration);
			ps.Stop();
		}
		#endregion

		#region Water
		[SerializeField] private Material waterMat;
		private GameObject water;

		void CreateWater()
		{
			water = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			water.transform.SetParent(transform, false);
			water.GetComponent<MeshFilter>().sharedMesh = planetMesh;
			water.GetComponent<Renderer>().sharedMaterial = waterMat;
		}

		void DestroyWater()
		{
			if(!water)
				return;
			Destroy(water);
		}
		#endregion
	}

}
