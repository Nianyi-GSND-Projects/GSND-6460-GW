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

		RenderTexture output;

		protected void Start()
		{
			if(material)
				material = new Material(material);

			output = RenderTexture.GetTemporary(UnityEngine.Screen.width, UnityEngine.Screen.height);
			Camera.targetTexture = output;

			Screen.cameraStack.Add(this);
		}

		public void Output(RenderTexture target)
		{
			if(material)
			{
				material.SetTexture("_ScreenTex", Screen.OutputTexture);
				RenderUtility.PostProcess(output, material);
			}
			Graphics.Blit(output, target);
		}

		protected void OnDestroy()
		{
			GameManager.Instance.Screen.cameraStack.Remove(this);
			RenderTexture.ReleaseTemporary(output);
		}
	}
}
