using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(EnumIndexAttribute))]
    public class EnumIndexDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumIndexAttribute myAttribute = attribute as EnumIndexAttribute;
            label = myAttribute.GetLabel(property);

            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label, new GUIContent("This type has not supported."));
                return;
            }

            position = EditorGUI.PrefixLabel(position, label);

            System.Type type = myAttribute.type;
            string[] displayedOptions = System.Enum.GetNames(type);

            System.Array array = System.Enum.GetValues(type);
            int[] optionValues = new int[array.Length];
            for (int i = 0; i < optionValues.Length; i++)
            {
                optionValues[i] = (int)array.GetValue(i);
            }

            property.intValue = EditorGUI.IntPopup(position, property.intValue, displayedOptions, optionValues);
        }
    }
}