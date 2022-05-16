using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIMaskSprite), true)]
    public class UIMaskSpriteEditor : UISpriteInspector
    {
        protected override void DrawCustomProperties()
        {
            GUILayout.Space(6f);
            SerializedProperty sp = NGUIEditorTools.DrawProperty("Mode", serializedObject, "mMode", GUILayout.MinWidth(20f));

            UIMaskSprite.SpriteMode mode = (UIMaskSprite.SpriteMode)sp.intValue;

            if (mode == UIMaskSprite.SpriteMode.AlphaMask)
            {
                NGUIEditorTools.DrawProperty("MaskTexture", serializedObject, "mMaskTexture");                
            }

            GUILayout.Space(-6f);
            base.DrawCustomProperties();
        }
    } 
}
