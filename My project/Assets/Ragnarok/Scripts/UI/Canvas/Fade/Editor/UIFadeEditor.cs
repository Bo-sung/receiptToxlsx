using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomEditor(typeof(UIFade), true)]
    public class UIFadeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty curve_fadeOver = serializedObject.FindProperty("curve_fadeOver");
            SerializedProperty curve_fadeOut = serializedObject.FindProperty("curve_fadeOut");

            GUILayout.Space(6f);
            NGUIEditorTools.BeginContents();
            {
                NGUIEditorTools.SetLabelWidth(110f);
                EditorGUILayout.PropertyField(curve_fadeOver, new GUIContent("가려질 때의 커브"), GUILayout.Width(170f), GUILayout.Height(62f));
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(curve_fadeOut, new GUIContent("사라질 때의 커브"), GUILayout.Width(170f), GUILayout.Height(62f));
                    if (GUILayout.Button("위와 동일하게 설정", GUILayout.Width(180f)))
                    {
                        curve_fadeOut.animationCurveValue = new AnimationCurve(curve_fadeOver.animationCurveValue.keys);
                    }
                }
                GUILayout.EndHorizontal();
            }
            NGUIEditorTools.EndContents();

            serializedObject.ApplyModifiedProperties();
        }
    }
}