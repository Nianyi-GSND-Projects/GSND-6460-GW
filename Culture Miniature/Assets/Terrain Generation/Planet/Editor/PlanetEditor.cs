using UnityEngine;
using UnityEditor;

namespace CultureMiniature
{
	[CustomEditor(typeof(Planet))]
	public class PlanetEditor : Editor
	{
		Planet Planet => target as Planet;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			DrawEditModeGui();
		}

		void DrawEditModeGui()
		{
			EditorGUILayout.Space();
			GUILayout.Label("Planet model", EditorStyles.boldLabel);

			EditorGUILayout.Space(2);
			GUILayout.Label($"Subdivision level: {Planet.SubdivisionLevel}");

			EditorGUILayout.Space(2);
			bool oldEnabled = GUI.enabled;
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = Application.isPlaying;
			if(GUILayout.Button("Create"))
				Planet.CreatePlanetMesh();
			GUI.enabled &= Planet.HasPm;
			if(GUILayout.Button("Subdivide"))
				Planet.SubdividePlanetMesh();
			if(GUILayout.Button("Dualize"))
				Planet.DualizePlanetMesh();
			if(GUILayout.Button("Colorize"))
				Planet.ColorizePlanetMesh();
			if(GUILayout.Button("Finalize"))
				Planet.FinalizePlanetMesh();
			EditorGUILayout.EndHorizontal();
			GUI.enabled = oldEnabled;
		}
	}
}
