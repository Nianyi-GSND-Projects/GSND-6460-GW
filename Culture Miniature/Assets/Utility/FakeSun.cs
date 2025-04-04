using UnityEngine;

namespace CultureMiniature
{
	public class FakeSun : MonoBehaviour
	{
		public Light mainLight;
		Camera mainCamera;
		public float baseDistance = 1;

		protected void Start()
		{
			mainCamera = Camera.main;
		}

		protected void Update()
		{
			Vector3 direction = -mainLight.transform.forward;
			float distance = mainCamera.farClipPlane * .5f;
			transform.position = mainCamera.transform.position + direction * distance;
			transform.LookAt(mainCamera.transform.position);
			transform.localScale = Vector3.one * (distance / baseDistance);
		}
	}
}
