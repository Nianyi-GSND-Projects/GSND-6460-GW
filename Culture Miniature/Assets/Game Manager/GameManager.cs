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
		[SerializeField] private PlanetCameraController planetCamera;
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
			StartCoroutine(Planet.GenerationCoroutine());
		}

		/// <summary>星球创建时的旋转动画控制。</summary>
		IEnumerator PCRotation()
		{
			// Set up configs.
			planetCamera.horizontalDistanceRatio = 0;
			planetCamera.longitude = 0;
			planetCamera.latitude = pcLatitude;
			planetCamera.altitude = planet.Radius * (pcRelativeRadius - 1);
			planetCamera.direction = 0;

			// Roll the animation.
			for(float previous = Time.time, now; ; previous = now)
			{
				yield return new WaitForEndOfFrame();
				float dt = (now = Time.time) - previous;
				planetCamera.longitude += dt * pcRotationSpeed;
			}
		}

		void UpdatePlanetFocus()
		{
			var mousePosition = Input.mousePosition;
			if(!float.IsNormal(mousePosition.sqrMagnitude))
				return;
			Ray ray = planetCamera.Camera.ScreenPointToRay(mousePosition);
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
