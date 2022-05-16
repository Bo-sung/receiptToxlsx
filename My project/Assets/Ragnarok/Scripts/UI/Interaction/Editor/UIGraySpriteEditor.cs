using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIGraySprite), true)]
    public class UIGraySpriteEditor : UISpriteInspector
    {
        protected override void DrawCustomProperties()
        {
            GUILayout.Space(6f);
            NGUIEditorTools.DrawProperty("Mode", serializedObject, "mode", GUILayout.MinWidth(20f));

            GUILayout.Space(-6f);
            base.DrawCustomProperties();
        }
    }
}