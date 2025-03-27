using UnityEngine;

namespace CultureMiniature
{
	[RequireComponent(typeof(Camera))]
	public class PlanetCameraController : CameraController
	{
		#region Component references
#if UNITY_EDITOR
		new
#endif
		private Camera camera;
		protected Planet Planet => GameManager.Instance.Planet;
		#endregion

		#region Unity life cycle
		protected void Start()
		{
			camera = GetComponent<Camera>();
		}

		protected void Update()
		{
			UpdateOrbit();
		}
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
			// 聚焦点投影到单位球面上的位置。
			Vector3 normalizedLocal = new(Mathf.Sin(longitude * Mathf.Deg2Rad), 0, Mathf.Cos(longitude * Mathf.Deg2Rad));
			normalizedLocal *= Mathf.Cos(latitude * Mathf.Deg2Rad);
			normalizedLocal.y = Mathf.Sin(latitude * Mathf.Deg2Rad);

			// 聚焦点在局部空间中的实际位置。
			Vector3 lookAtPoint = normalizedLocal * (Planet.Radius + altitude);

			Vector3 northTangent = Vector3.ProjectOnPlane(Vector3.up, normalizedLocal).normalized;
			Vector3 westTangent = Vector3.Cross(northTangent, normalizedLocal);
			Vector3 cameraOffset = Mathf.Cos(-direction * Mathf.Deg2Rad) * northTangent + Mathf.Sin(-direction * Mathf.Deg2Rad) * westTangent;
			cameraOffset *= horizontalDistanceRatio;
			cameraOffset += normalizedLocal;
			cameraOffset *= altitude;

			// 赋值相机属性。
			camera.transform.position = lookAtPoint + cameraOffset;
			camera.transform.rotation = Quaternion.LookRotation(-cameraOffset, normalizedLocal);
		}
		#endregion
	}
}
