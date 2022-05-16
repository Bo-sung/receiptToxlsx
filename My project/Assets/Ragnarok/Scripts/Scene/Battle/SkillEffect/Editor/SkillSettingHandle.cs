using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public class SkillSettingHandle
    {
        SerializedProperty property;

        public void Select(SerializedProperty property)
        {
            this.property = property;
        }

        public void Draw()
        {
            using (new GUILayout.VerticalScope(EditorStyles.objectFieldThumb, GUILayout.MinWidth(400f), GUILayout.ExpandHeight(true)))
            {
                if (property == null)
                {
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    property.serializedObject.Update();
                    EditorGUILayout.PropertyField(property);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}