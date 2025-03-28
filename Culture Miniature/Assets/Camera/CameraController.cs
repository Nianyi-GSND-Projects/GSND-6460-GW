using UnityEngine;

namespace CultureMiniature
{
	public abstract class CameraController : MonoBehaviour
	{
		public abstract Vector3 Position { get; }
		public abstract Quaternion Orientation { get; }
	}
}
