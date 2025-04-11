using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CultureMiniature
{
	public class Screen : MonoBehaviour
	{
		[SerializeField] private RawImage graphic;
		RenderTexture outputTexture;

		public bool clearBlack = true;
		public List<CameraAgent> cameraStack = new();

		public RenderTexture OutputTexture {
			get
			{
				if(!outputTexture)
				{
					outputTexture = RenderTexture.GetTemporary(UnityEngine.Screen.width, UnityEngine.Screen.height);
					graphic.texture = outputTexture;
				}
				return outputTexture;
			}
		}

		private Material clearMat;
		protected void Start()
		{
			clearMat = new Material(Shader.Find("Culture Miniature/Clear Black"));
		}

		protected void Update()
		{
			if(clearBlack)
				RenderUtility.PostProcess(OutputTexture, clearMat);
			foreach(var ca in cameraStack)
			{
				if(!ca.isActiveAndEnabled)
					continue;
				ca.Output(OutputTexture);
			}
		}

		protected void OnDestroy()
		{
			if(outputTexture)
				RenderTexture.ReleaseTemporary(outputTexture);
		}
	}
}
