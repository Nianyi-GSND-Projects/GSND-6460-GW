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
		[SerializeField] private MainCamera mainCamera;
		[SerializeField] private Planet planet;
		public Planet Planet => planet;
		#endregion

		#region Unity life cycle
		protected void Start()
		{
			StartCoroutine(nameof(Main));
		}
		#endregion

		IEnumerator Main()
		{
			yield return new WaitForEndOfFrame();

			float standardInterval = 0.5f;
			yield return new WaitForSeconds(standardInterval);
			planet.CreateMesh();
			Debug.Log("Created");
			for(int i = 0; i < planet.debugSubdivisionLevel; ++i)
			{
				yield return new WaitForSeconds(standardInterval);
				planet.SubdivideMesh();
				Debug.Log($"Subbed #{i}");
			}
			yield return new WaitForSeconds(standardInterval);
			planet.DualizeMesh();
			Debug.Log("Dualized");
			yield return new WaitForSeconds(standardInterval);
			planet.ColorizeMesh();
			Debug.Log("Colorized");
			planet.FinalizeMesh();
			Debug.Log("Finalized");
		}
	}
}
