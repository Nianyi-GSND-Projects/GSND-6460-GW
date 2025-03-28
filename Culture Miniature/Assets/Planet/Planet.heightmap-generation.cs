using UnityEngine;

namespace CultureMiniature
{
	public partial class Planet
	{
		public ComputeShader HeightmapComputer;

		private RenderTexture GenerateHeightMap()
		{
			int Size = 2048;
			int PerlinGridCount = 16;

			RenderTexture rt = RenderTexture.GetTemporary(2048, 2048, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			rt.enableRandomWrite = true;
			rt.wrapModeU = TextureWrapMode.Repeat;
			rt.wrapModeV = TextureWrapMode.Mirror;

			Vector3[,,] perlin = new Vector3[PerlinGridCount, PerlinGridCount, PerlinGridCount];
			// Fill in the buffer with random vector
			for(int i = 0; i < PerlinGridCount; i++)
			{
				for(int j = 0; j < PerlinGridCount; j++)
				{
					for(int k = 0; k < PerlinGridCount; k++)
					{
						perlin[i, j, k].x = Random.Range(-1f, 1f);
						perlin[i, j, k].y = Random.Range(-1f, 1f);
						perlin[i, j, k].z = Random.Range(-1f, 1f);
						perlin[i, j, k].Normalize();
					}
				}
			}

			var PerlinBuffer = new ComputeBuffer(PerlinGridCount * PerlinGridCount * PerlinGridCount, sizeof(float) * 3);
			PerlinBuffer.SetData(Vector3ArrayTo1DArray(perlin));

			int kernel = HeightmapComputer.FindKernel("CSMain");
			HeightmapComputer.SetBuffer(kernel, "PerlinBuffer", PerlinBuffer);
			HeightmapComputer.SetTexture(kernel, "Result", rt);
			HeightmapComputer.SetInt("MapSize", PerlinGridCount);
			HeightmapComputer.Dispatch(kernel, Size / 8, Size / 8, 1);

			PerlinBuffer.Release();

			return rt;
		}

		static float[] Vector3ArrayTo1DArray(Vector3[,,] source)
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