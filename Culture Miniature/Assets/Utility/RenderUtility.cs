using UnityEngine;

namespace CultureMiniature
{
	public static class RenderUtility
	{
		public static void PostProcess(RenderTexture rt, Material mat)
		{
			RenderTexture target = RenderTexture.GetTemporary(rt.descriptor);
			Graphics.Blit(rt, target);
			Graphics.Blit(target, rt, mat);
			RenderTexture.ReleaseTemporary(target);
		}
	}
}
