using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomEditor(typeof(UISafeAreaResizeViewport))]
    public class UISafeAreaResizeViewportEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SerializedProperty userCustom = serializedObject.FindProperty("useCustom");
            SerializedProperty customArea = serializedObject.FindProperty("customArea");

            EditorGUILayout.PropertyField(userCustom);
            EditorGUILayout.PropertyField(customArea);

            if (GUILayout.Button("Reset Safety Area"))
            {
                customArea.rectValue = Screen.safeArea;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}