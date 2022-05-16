using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIResizeGrid), true)]
    public class UIResizeGridEditor : UIWidgetContainerEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty sp = NGUIEditorTools.DrawProperty("Arrangement", serializedObject, "arrangement");

            NGUIEditorTools.DrawProperty("  Max Cell Width", serializedObject, "maxCellWidth");
            NGUIEditorTools.DrawProperty("  Max Cell Height", serializedObject, "maxCellHeight");

            GUI.enabled = false;
            NGUIEditorTools.DrawProperty("    Cell Width", serializedObject, "cellWidth");
            NGUIEditorTools.DrawProperty("    Cell Height", serializedObject, "cellHeight");
            GUI.enabled = true;

            if (sp.intValue < 2)
            {
                bool columns = (sp.hasMultipleDifferentValues || (UIGrid.Arrangement)sp.intValue == UIGrid.Arrangement.Horizontal);

                GUILayout.BeginHorizontal();
                {
                    sp = NGUIEditorTools.DrawProperty(columns ? "  Column Limit" : "  Row Limit", serializedObject, "maxPerLine");
                    if (sp.intValue < 0) sp.intValue = 0;
                    if (sp.intValue == 0) GUILayout.Label("Unlimited");
                }
                GUILayout.EndHorizontal();

                UIGrid.Sorting sort = (UIGrid.Sorting)NGUIEditorTools.DrawProperty("Sorting", serializedObject, "sorting").intValue;

                if (sp.intValue != 0 && (sort == UIGrid.Sorting.Horizontal || sort == UIGrid.Sorting.Vertical))
                {
                    EditorGUILayout.HelpBox("Horizontal and Vertical sortinig only works if the number of rows/columns remains at 0.", MessageType.Warning);
                }
            }

            NGUIEditorTools.DrawProperty("Pivot", serializedObject, "pivot");
            NGUIEditorTools.DrawProperty("Smooth Tween", serializedObject, "animateSmoothly");
            NGUIEditorTools.DrawProperty("Constrain to Panel", serializedObject, "keepWithinPanel");
            serializedObject.ApplyModifiedProperties();
        }
    }
}