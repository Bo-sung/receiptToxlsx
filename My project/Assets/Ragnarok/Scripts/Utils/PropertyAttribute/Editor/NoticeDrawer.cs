using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(NoticeAttribute))]
    public class NoticeDrawer : PropertyDrawer
    {
        private const float INDENTING = 4f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            NoticeAttribute myAttribute = attribute as NoticeAttribute;
            label = myAttribute.GetLabel(property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label, new GUIContent("This type has not supported."));
                return;
            }

            Rect rectBox = position;
            rectBox.height = GetPropertyHeight(property, label);
            //GUI.Box(rectBox, string.Empty, "ObjectFieldThumb");
            GUI.Box(rectBox, string.Empty, "ProgressBarBack");

            EditorGUI.BeginProperty(position, label, property);
            {
                position.x += INDENTING;
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.width -= INDENTING;
                position.width -= INDENTING;

                Rect labelPosition = EditorGUI.IndentedRect(position);

                bool isEnabled = GUI.enabled;
                GUI.enabled = true;
                {
                    EditorGUI.HandlePrefixLabel(position, labelPosition, label);
                }
                GUI.enabled = isEnabled;

                position.y += EditorGUIUtility.singleLineHeight;
                position.y += EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.BeginChangeCheck();
                string stringValue = EditorGUI.TextField(position, property.stringValue, EditorStyles.miniTextField);
                if (EditorGUI.EndChangeCheck())
                    property.stringValue = stringValue;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}