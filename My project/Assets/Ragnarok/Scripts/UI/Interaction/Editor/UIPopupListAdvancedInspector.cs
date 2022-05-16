using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomEditor(typeof(UIPopupListAdvanced), true)]
    public class UIPopupListAdvancedInspector : UIPopupListInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            NGUIEditorTools.DrawProperty("Label Category", serializedObject, "labelCategory");

            base.OnInspectorGUI();
        }
    }
}