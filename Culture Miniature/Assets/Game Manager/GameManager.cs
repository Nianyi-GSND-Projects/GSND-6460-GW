using UnityEngine;
using System.Collections;

namespace CultureMiniature
{
	public class GameManager : MonoBehaviour
	{
		#region Singleton
		private static GameManager instance;
		public static GameManager Instance => instance;
		protected void Awake()
		{
			instance = this;
		}
		#endregion

		#region Component references
		[Header("Componenr references")]
		[SerializeField] private MainCamera mainCamera;
		[SerializeField] private Planet planet;
		public Planet Planet => planet;
		#endregion

		#region Unity life cycle
		protected void Start()
		{
			StartCoroutine(nameof(Main));
		}

		protected void Update()
		{
			UpdatePlanetFocus();
		}
		#endregion

		#region Fields
		[Header("Fields")]

		[Header("Planet creation")]
		[SerializeField][Range(-90, 90)] private float pcLatitude = 15;
		[SerializeField][Min(0)] private float pcRotationSpeed = 45;
		[SerializeField][Min(0)] private float pcRelativeRadius = 4;
		#endregion

		#region Life cycle
		IEnumerator Main()
		{
			yield return new WaitForEndOfFrame();

			StartCoroutine(nameof(PCRotation));

			float standardInterval = 1f;
			planet.CreateMesh();
			for(int i = 0; i < planet.debugSubdivisionLevel; ++i)
			{
				yield return new WaitForSeconds(standardInterval);
				planet.SubdivideMesh();
			}
			planet.FinalizeMesh();

			yield return new WaitForSeconds(standardInterval);
			planet.CreateHeightMap();
		}

		/// <summary>星球创建时的旋转动画控制。</summary>
		IEnumerator PCRotation()
		{
			var pc = mainCamera.planetCamera;

			// Set up configs.
			pc.horizontalDistanceRatio = 0;
			pc.longitude = 0;
			pc.latitude = pcLatitude;
			pc.altitude = planet.Radius * (pcRelativeRadius - 1);
			pc.direction = 0;

			// Roll the animation.
			for(float previous = Time.time, now; ; previous = now)
			{
				yield return new WaitForEndOfFrame();
				float dt = (now = Time.time) - previous;
				pc.longitude += dt * pcRotationSpeed;
			}
		}

		void UpdatePlanetFocus()
		{
			var mousePosition = Input.mousePosition;
			if(!float.IsNormal(mousePosition.sqrMagnitude))
				return;
			Ray ray = mainCamera.Camera.ScreenPointToRay(mousePosition);
			if(!Physics.Raycast(ray, out var hit, float.PositiveInfinity, Planet.LayerMask))
			{
				Planet.UseFocus = false;
				return;
			}
			Planet.UseFocus = true;
			Planet.FocusPosition = Planet.transform.worldToLocalMatrix.MultiplyPoint(hit.point) * Planet.Radius;
		}
		#endregion
	}
}
