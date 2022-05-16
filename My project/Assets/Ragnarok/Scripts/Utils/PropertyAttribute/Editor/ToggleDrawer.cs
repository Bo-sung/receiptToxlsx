using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(ToggleAttribute))]
    public class ToggleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ToggleAttribute myAttribute = attribute as ToggleAttribute;
            label = myAttribute.GetLabel(property);

            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                EditorGUI.LabelField(position, label, new GUIContent("This type has not supported."));
                return;
            }

            string[] names = null;

            bool oldValue = property.boolValue;
            bool newValue;
            switch (myAttribute.toggleType)
            {
                case ToggleType.Default:
                    EditorGUI.PropertyField(position, property, label);
                    break;

                case ToggleType.Legacy:
                    EditorGUI.BeginChangeCheck();
                    newValue = GUI.Toggle(position, oldValue, label);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.boolValue = newValue;
                    }
                    break;

                case ToggleType.OnOff:
                    position = EditorGUI.PrefixLabel(position, label);
                    string text = oldValue ? GetTextYes(myAttribute.yes) : GetTextNo(myAttribute.no);
                    EditorGUI.BeginChangeCheck();
                    newValue = GUI.Toggle(position, oldValue, text, EditorStyles.miniButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.boolValue = newValue;
                    }
                    break;

                case ToggleType.Toolbar:
                    position = EditorGUI.PrefixLabel(position, label);
                    names = GetNames(myAttribute.no, myAttribute.yes);
                    break;

                case ToggleType.NoNameToolbar:
                    names = GetNames(myAttribute.no, myAttribute.yes);
                    break;
            }

            if (names == null)
                return;

            int oldIndex = oldValue ? 1 : 0;
            int newIndex;
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

            // 같을 경우는 아무것도 하지 않는다
            if (oldIndex == newIndex)
                return;

            newValue = newIndex > 0;
            property.boolValue = newValue;
        }

        private string GetTextNo(string no)
        {
            return string.IsNullOrEmpty(no) ? "false" : no;
        }

        private string GetTextYes(string yes)
        {
            return string.IsNullOrEmpty(yes) ? "true" : yes;
        }

        private string[] GetNames(string no, string yes)
        {
            no = GetTextNo(no);
            yes = GetTextYes(yes);
            return MakeNames(no, yes);
        }

        private string[] MakeNames(params string[] args)
        {
            return args;
        }
    }
}