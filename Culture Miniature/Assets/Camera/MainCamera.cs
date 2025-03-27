using UnityEngine;

namespace CultureMiniature
{
	[RequireComponent(typeof(Camera))]
	public class MainCamera : MonoBehaviour
	{
		[SerializeField] public PlanetCameraController planetCamera;
		public CameraController[] CameraControllers => new CameraController[] { planetCamera };
		private CameraController controller;
#if UNITY_EDITOR
		new
#endif
		private Camera camera;
		public Camera Camera => camera;

		protected void Start()
		{
			camera = GetComponent<Camera>();
			controller = planetCamera;
		}

		protected void Update()
		{
			camera.transform.SetPositionAndRotation(controller.Position, controller.Orientation);
		}
	}
}
