using UnityEngine;

namespace Ragnarok
{
    public sealed class FPSGUI : MonoBehaviour
    {
        [Range(1,100)]
        [SerializeField] int fontSize;

        [Range(0,1)]
        [SerializeField] float red, green, blue;

        float deltiTime = 0f;

        private void Start()
        {
            fontSize = fontSize == 0 ? 50 : fontSize;
        }

        private void Update()
        {
            deltiTime += (Time.unscaledDeltaTime - deltiTime) * 0.1f;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / fontSize;
            style.normal.textColor = new Color(red, green, blue, 1.0f);
            float msec = deltiTime * 1000f;
            float fps = 1f / deltiTime;
            string text = $"{msec:0.0} ms ({fps:0.} fps)";
            GUI.Label(rect, text, style);
        }
    }
}
