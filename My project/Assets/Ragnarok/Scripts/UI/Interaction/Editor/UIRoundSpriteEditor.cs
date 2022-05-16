using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIRoundSprite), true)]
    public class UIRoundSpriteEditor : UISpriteInspector
    {
        protected override void DrawCustomProperties()
        {
            GUILayout.Space(6f);
            SerializedProperty sp = NGUIEditorTools.DrawProperty("Mode", serializedObject, "mode", GUILayout.MinWidth(20f));

            UIRoundSprite.SpriteMode mode = (UIRoundSprite.SpriteMode)sp.intValue;

            if(mode == UIRoundSprite.SpriteMode.Round)
            {
                NGUIEditorTools.DrawProperty("Round", serializedObject, "mRound");
            }

            GUILayout.Space(-6f);
            base.DrawCustomProperties();
        }
    } 
}
