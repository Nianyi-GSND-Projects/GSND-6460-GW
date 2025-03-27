using UnityEngine;

namespace CultureMiniature
{
	public static class DebugUtility
	{
		public class Timer
		{
			public string name;
			public float begin, end;
			public System.Action onEnd;

			public Timer(string name)
			{
				this.name = name;
				onEnd += Log;
			}

			private void Log()
			{
				float duration = end - begin;
				string message = $"{name}: Finished in {duration} seconds.";
				if(duration <= timerWarningLimit)
					Debug.Log(message);
				else
					Debug.LogWarning(message);
			}

			public void Start()
			{
				begin = Time.realtimeSinceStartup;
			}
			public void Stop()
			{
				end = Time.realtimeSinceStartup;
				onEnd?.Invoke();
			}
		}

		public static float timerWarningLimit = 1.0f;
		private static Timer staticTimer;
		public static void StartTiming(string name)
		{
			if(staticTimer != null)
				staticTimer.Stop();
			staticTimer = new(name);
			staticTimer.Start();
		}

		public static void StopTiming()
		{
			if(staticTimer == null)
				return;
			staticTimer.Stop();
			staticTimer = null;
		}
	}
}
