using UnityEngine;

namespace CultureMiniature
{
	[RequireComponent(typeof(Camera))]
	public class PlanetCameraController : CameraController
	{
		#region Component references
		protected Planet Planet => GameManager.Instance.Planet;
		#endregion

		#region Unity life cycle
		protected void Update()
		{
			UpdateOrbit();
		}
		#endregion

		#region Interfaces
		private Vector3 position;
		private Quaternion rotation;

		public override Vector3 Position => Planet.transform.localToWorldMatrix.MultiplyPoint(position) / Planet.Radius;
		public override Quaternion Orientation => rotation * Planet.transform.localToWorldMatrix.rotation;
		#endregion

		#region Camera configs
		/// <summary>聚焦点的经度，以角度记。</summary>
		[Range(-180, +179)] public float longitude;
		/// <summary>聚焦点的纬度，以角度记。</summary>
		[Range(-90, +89)] public float latitude;
		/// <summary>相机到聚焦点的海拔高度。</summary>
		[Min(0)] public float altitude;
		/// <summary>相机方向，以角度记，北为基准，西正东负（向地面看是逆时针）。</summary>
		[Range(-180, 179)] public float direction;
		/// <summary>相机到聚焦点垂线的地平距离相对于海拔的比例。</summary>
		[Min(0)] public float horizontalDistanceRatio;

		void UpdateOrbit()
		{
			CalculateOrbit(out Vector3 normalizedLocal, out Vector3 northTangent, out Vector3 lookAtPoint, out Vector3 cameraOffset);

			position = lookAtPoint + cameraOffset;
			Vector3 upVector = horizontalDistanceRatio == 0 ? northTangent : normalizedLocal;
			rotation = Quaternion.LookRotation(-cameraOffset, upVector);
		}

		void CalculateOrbit(out Vector3 normalizedLocal, out Vector3 northTangent, out Vector3 lookAtPoint, out Vector3 cameraOffset)
		{
			// 聚焦点投影到单位球面上的位置。
			normalizedLocal = new(Mathf.Sin(longitude * Mathf.Deg2Rad), 0, Mathf.Cos(longitude * Mathf.Deg2Rad));
			normalizedLocal *= Mathf.Cos(latitude * Mathf.Deg2Rad);
			normalizedLocal.y = Mathf.Sin(latitude * Mathf.Deg2Rad);

			// 聚焦点在局部空间中的实际位置。
			lookAtPoint = normalizedLocal * Planet.Radius;

			northTangent = Vector3.ProjectOnPlane(Vector3.up, normalizedLocal).normalized;
			Vector3 westTangent = Vector3.Cross(northTangent, normalizedLocal);
			cameraOffset = Mathf.Cos(direction * Mathf.Deg2Rad) * northTangent + Mathf.Sin(direction * Mathf.Deg2Rad) * westTangent;
			cameraOffset *= horizontalDistanceRatio;
			cameraOffset += normalizedLocal;
			cameraOffset *= altitude;
		}
		#endregion
	}
}
