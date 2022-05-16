using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Ragnarok
{
    public sealed class PresetViewer
    {
        Vector2 scrollPosition;

        private Preset preset;

        public void OnGUI(Rect rect)
        {
            if (preset == null)
                return;

            using (new GUILayout.AreaScope(rect))
            {
                using (var gui = new GUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = gui.scrollPosition;

                    using (new GUILayout.VerticalScope())
                    {
                        Editor editor = Editor.CreateEditor(preset);
                        editor.DrawHeader();
                        editor.OnInspectorGUI();
                    }
                }
            }
        }

        public void SetData(Preset preset)
        {
            this.preset = preset;
            scrollPosition = Vector2.zero;
        }
    }
}