using UnityEngine;

namespace CultureMiniature
{
	[RequireComponent(typeof(Camera))]
	public class CameraAgent : MonoBehaviour
	{
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
	}
}
