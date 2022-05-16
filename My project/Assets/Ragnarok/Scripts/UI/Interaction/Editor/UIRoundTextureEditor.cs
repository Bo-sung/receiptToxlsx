using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIRoundTexture), true)]
    public class UIRoundTextureEditor : UITextureInspector
    {
        protected override void DrawCustomProperties()
        {
            GUILayout.Space(6f);
            SerializedProperty sp = NGUIEditorTools.DrawProperty("RoundMode", serializedObject, "roundMode", GUILayout.MinWidth(20f));

            UIRoundTexture.RoundMode mode = (UIRoundTexture.RoundMode)sp.intValue;

            if (mode == UIRoundTexture.RoundMode.Round)
            {
                NGUIEditorTools.DrawProperty("RoundValue", serializedObject, "roundValue");
            }

            GUILayout.Space(-6f);
            base.DrawCustomProperties();
        }
    }
}