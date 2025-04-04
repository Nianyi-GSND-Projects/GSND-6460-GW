using UnityEngine;

namespace CultureMiniature
{
	public class RotateConstantly : MonoBehaviour
	{
		public Vector3 axis;
		public float speed;

		public void FixedUpdate()
		{
			float dt = Time.fixedDeltaTime;
			transform.rotation = Quaternion.AngleAxis(dt * speed, axis) * transform.rotation;
		}
	}
}
