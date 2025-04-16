using UnityEngine;

namespace CultureMiniature
{
	public partial class GameManager
	{
		static GUIStyle ctrlBtnStyle;

		protected void OnGUI()
		{
			return;
			if(debug)
				return;

			ctrlBtnStyle ??= new(GUI.skin.button)
			{
				fontSize = 20,
				stretchWidth = false,
			};

			GUILayout.BeginArea(new Rect(0, 0, UnityEngine.Screen.width, 80));
			GUILayout.BeginHorizontal(new GUIStyle()
			{
				padding = new RectOffset(20, 20, 20, 20),
			});

			if(GUILayout.Button("Pause", ctrlBtnStyle))
				Time.timeScale = 0;
			GUILayout.Space(10);
			if(GUILayout.Button("Resume", ctrlBtnStyle))
				Time.timeScale = 1;

			GUILayout.Space(20);

			if(GUILayout.Button("Toggle Anaglyph", ctrlBtnStyle))
				UseAnaglyph ^= true;
			GUILayout.Space(10);
			if(GUILayout.Button("Toggle Camera Rotation", ctrlBtnStyle))
				planetCamera.enabled ^= true;
			GUILayout.Space(10);
			if(GUILayout.Button("Toggle Sun Rotation", ctrlBtnStyle))
				sunRotation.enabled ^= true;
			GUILayout.Space(10);

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}
}
