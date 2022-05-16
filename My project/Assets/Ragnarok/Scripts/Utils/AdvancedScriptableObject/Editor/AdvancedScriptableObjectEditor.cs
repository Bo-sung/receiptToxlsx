using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true)]
    public class AdvancedScriptableObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
                Save(target);
        }

        private void Save(Object obj)
        {
            EditorUtility.SetDirty(obj);
        }
    }
}