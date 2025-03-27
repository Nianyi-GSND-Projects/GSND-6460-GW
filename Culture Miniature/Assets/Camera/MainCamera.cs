using UnityEngine;

namespace CultureMiniature
{
	[RequireComponent(typeof(Camera))]
	public class MainCamera : MonoBehaviour
	{
		[SerializeField] private PlanetCameraController planetCamera;
		public CameraController[] CameraControllers => new CameraController[] { planetCamera };

#if UNITY_EDITOR
		new
#endif
		private Camera camera;

		protected void Start()
		{
			camera = GetComponent<Camera>();
		}
	}
}
