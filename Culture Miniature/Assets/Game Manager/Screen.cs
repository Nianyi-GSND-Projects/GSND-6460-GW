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

		private readonly List<CameraAgent> awaitingCamera = new();
		public void OnCameraRendered(CameraAgent ca)
		{
			if(awaitingCamera.Count == 0)
			{
				StartFrame();
				awaitingCamera.Clear();
				awaitingCamera.AddRange(cameraStack);
			}
			awaitingCamera.Remove(ca);

			ca.Output(OutputTexture);
		}
		private void StartFrame()
		{
			if(clearBlack)
				RenderUtility.PostProcess(OutputTexture, clearMat);
		}

		protected void OnDestroy()
		{
			if(outputTexture)
				RenderTexture.ReleaseTemporary(outputTexture);
		}
	}
}
