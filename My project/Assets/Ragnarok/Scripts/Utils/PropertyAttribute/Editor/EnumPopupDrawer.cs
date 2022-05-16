using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(EnumPopupAttribute))]
    public class EnumPopupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumPopupAttribute myAttribute = attribute as EnumPopupAttribute;
            label = myAttribute.GetLabel(property);

            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position, label, new GUIContent("This type has not supported."));
                return;
            }

            if (!myAttribute.isHideDisplayName)
                position = EditorGUI.PrefixLabel(position, label);

            System.Enum oldEnum, newValue;
            int oldIndex, newIndex;
            switch (myAttribute.enumPopupType)
            {
                case EnumPopupType.Default:
                    oldEnum = GetCurrentEnum(property);

                    EditorGUI.BeginChangeCheck();
                    newValue = EditorGUI.EnumPopup(position, oldEnum);
                    if (EditorGUI.EndChangeCheck())
                        property.enumValueIndex = System.Convert.ToInt32(newValue);
                    break;

                case EnumPopupType.Toolbar:
                    oldIndex = property.enumValueIndex;
                    string[] names = myAttribute.names.Length > 0 ? myAttribute.names : System.Enum.GetNames(fieldInfo.FieldType);

                    EditorGUI.BeginChangeCheck();
                    if (oldIndex >= names.Length)
                    {
                        int convertNum = names.Length - 1;
                        int newSelectedNum = GUI.Toolbar(position, convertNum, names);
                        // 선택한 index가 동일하다면 기존 index를 반환
                        newIndex = newSelectedNum == convertNum ? oldIndex : newSelectedNum;
                    }
                    else
                    {
                        newIndex = GUI.Toolbar(position, oldIndex, names);
                    }
                    if (EditorGUI.EndChangeCheck())
                        property.enumValueIndex = newIndex;
                    break;

                case EnumPopupType.EnumMask:
                    oldEnum = GetCurrentEnum(property);

                    EditorGUI.BeginChangeCheck();
                    newValue = EditorGUI.EnumMaskField(position, oldEnum);
                    if (EditorGUI.EndChangeCheck())
                        property.intValue = System.Convert.ToInt32(newValue);
                    break;
            }
        }

        private System.Enum GetCurrentEnum(SerializedProperty property)
        {
            object current = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Replace(".Array.data", ".").Split('.');
            foreach (var propertyName in propertyNames)
            {
                object parent = current;
                int indexerStart = propertyName.IndexOf('[');
                if (indexerStart == -1)
                {
                    System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
                    current = parent.GetType().GetField(propertyName, flag).GetValue(parent);
                }
                else if (parent.GetType().IsArray)
                {
                    int indexerEnd = propertyName.IndexOf(']');
                    string indexString = propertyName.Substring(indexerStart + 1, indexerEnd - indexerStart - 1);
                    int index = int.Parse(indexString);
                    System.Array array = (System.Array)parent;

                    if (index < array.Length)
                    {
                        current = array.GetValue(index);
                    }
                    else
                    {
                        current = null;
                        break;
                    }
                }
                else
                {
                    throw new System.MissingFieldException();
                }
            }

            if (current != null)
                return current as System.Enum;

            return null;
        }
    }
}