using UnityEngine;
using UnityEngine.Rendering;

namespace CultureMiniature
{
	public class HeightMapGenerator : MonoBehaviour
	{
		private int Size = 2048;
		private int PerlinGridCount = 16;
		public ComputeShader HeightmapComputer;
		private ComputeBuffer PerlinBuffer;
		public RenderTexture resultTexture;
		public Texture2D OutputTexture;

		void Start()
		{
			//Generator Perlin Noise Vector

			Vector3[,,] perlin = new Vector3[PerlinGridCount, PerlinGridCount, PerlinGridCount];
			for(int i = 0; i < PerlinGridCount; i++)
				for(int j = 0; j < PerlinGridCount; j++)
					for(int k = 0; k < PerlinGridCount; k++)
					{
						// fill in the buffer with random vector
						perlin[i, j, k].x = Random.Range(-1f, 1f);
						perlin[i, j, k].y = Random.Range(-1f, 1f);
						perlin[i, j, k].z = Random.Range(-1f, 1f);
						perlin[i, j, k].Normalize();
						Debug.Log(perlin[i, j, k]);
					}

			PerlinBuffer = new ComputeBuffer(PerlinGridCount * PerlinGridCount * PerlinGridCount, sizeof(float) * 3);
			PerlinBuffer.SetData(Vector3ArrayTo1DArray(perlin));

			//

			int kernel = HeightmapComputer.FindKernel("CSMain");
			HeightmapComputer.SetBuffer(kernel, "PerlinBuffer", PerlinBuffer);
			HeightmapComputer.SetTexture(kernel, "Result", resultTexture);
			HeightmapComputer.SetInt("MapSize", PerlinGridCount);
			HeightmapComputer.Dispatch(kernel, Size / 8, Size / 8, 1);

			AsyncGPUReadback.Request(resultTexture, 0, TextureFormat.ARGB32, OnCompleteReadback);
		}

		private void OnCompleteReadback(AsyncGPUReadbackRequest request)
		{
			if(request.hasError)
			{
				Debug.LogError("Failed to read RenderTexture from ComputeShader!");
				return;
			}

			OutputTexture.LoadRawTextureData(request.GetData<byte>());
			OutputTexture.Apply();
			Debug.Log("ComputeShader RenderTexture copied to Texture2D!");
		}

		void OnDestroy()
		{
			if(PerlinBuffer != null)
				PerlinBuffer.Release();
		}

		float[] Vector3ArrayTo1DArray(Vector3[,,] source)
		{
			int width = source.GetLength(0);
			int height = source.GetLength(1);
			int length = source.GetLength(2);
			float[] resultarray = new float[width * height * length * 3];

			for(int x = 0; x < width; x++)
			{
				for(int y = 0; y < height; y++)
				{
					for(int z = 0; z < length; z++)
					{
						int index = (x * height * length + y * length + z) * 3;
						resultarray[index] = source[x, y, z].x;
						resultarray[index + 1] = source[x, y, z].y;
						resultarray[index + 2] = source[x, y, z].z;
					}
				}
			}

			return resultarray;
		}
	}
}
