using UnityEngine;

namespace CultureMiniature
{
	public class MainCamera : MonoBehaviour
	{
		[SerializeField] public PlanetCameraController planetCamera;
		public CameraController[] CameraControllers => new CameraController[] { planetCamera };
		private CameraController controller;

		[SerializeField] private Camera mainCam;
		public Camera Camera => mainCam;

		protected void Start()
		{
			if(!mainCam)
				mainCam = GetComponent<Camera>();
			controller = planetCamera;
		}

		protected void Update()
		{
			transform.SetPositionAndRotation(controller.Position, controller.Orientation);
		}
	}
}
