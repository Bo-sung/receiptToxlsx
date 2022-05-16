using UnityEngine;

namespace Ragnarok
{
    public static class CameraExtensions
    {
        /// <summary>
        /// NGUIEditorExtensions.cs 참조
        /// </summary>
        public static RenderTexture RenderTexture(this Camera cam, int width, int height)
        {
            RenderTexture rt = cam.targetTexture;

            // Set up the render texture for the camera
            if (rt == null)
            {
                rt = new RenderTexture(width, height, 1)
                {
                    hideFlags = HideFlags.HideAndDontSave,
#if UNITY_5_5_OR_NEWER
                    autoGenerateMips = false,
#else
			        generateMips = false,
#endif
                    format = RenderTextureFormat.ARGB32,
                    filterMode = FilterMode.Trilinear,
                    anisoLevel = 4,
                };
            }
            cam.targetTexture = rt;
            if (rt != null) cam.Render();
            return rt;
        }
    }
}