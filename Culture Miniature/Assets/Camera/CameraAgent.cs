using UnityEngine;

namespace CultureMiniature
{
	[RequireComponent(typeof(Camera))]
	public class CameraAgent : MonoBehaviour
	{
		[SerializeField] private Material material;
		public Material Material => material;

		Screen Screen => GameManager.Instance.Screen;

#if UNITY_EDITOR
		new
#endif
		private Camera camera;
		public Camera Camera
		{
			get
			{
				if(!camera)
					camera = GetComponent<Camera>();
				return camera;
			}
		}

		RenderTexture outputTexture;
		RenderTexture OutputTexture
		{
			get
			{
				if(outputTexture == null)
				{
					outputTexture = RenderTexture.GetTemporary(Camera.pixelWidth, Camera.pixelHeight, 16);
					camera.depthTextureMode |= DepthTextureMode.Depth;
					Camera.targetTexture = outputTexture;
				}
				return outputTexture;
			}
		}

		protected void Start()
		{
			if(material)
				material = new Material(material);

			var _ = OutputTexture;
		}

		protected void OnEnable()
		{
			Screen.cameraStack.Add(this);
		}

		protected void OnDisable()
		{
			Screen.cameraStack.Remove(this);
		}

		protected void OnPostRender()
		{
			Screen.OnCameraRendered(this);
		}
		public void Output(RenderTexture target)
		{
			if(material)
			{
				material.SetTexture("_ScreenTex", Screen.OutputTexture);
				RenderUtility.PostProcess(OutputTexture, material);
			}
			Graphics.Blit(OutputTexture, target);
		}

		protected void OnDestroy()
		{
			RenderTexture.ReleaseTemporary(OutputTexture);
		}
	}
}
