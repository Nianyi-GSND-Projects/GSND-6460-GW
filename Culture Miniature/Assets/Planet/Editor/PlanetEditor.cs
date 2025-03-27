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
				Planet.CreateMesh();
			GUI.enabled &= Planet.HasPm;
			if(GUILayout.Button("Subdivide"))
				Planet.SubdivideMesh();
			if(GUILayout.Button("Dualize"))
				Planet.DualizeMesh();
			if(GUILayout.Button("Colorize"))
				Planet.ColorizeMesh();
			if(GUILayout.Button("Finalize"))
				Planet.FinalizeMesh();
			EditorGUILayout.EndHorizontal();
			GUI.enabled = oldEnabled;
		}
	}
}
