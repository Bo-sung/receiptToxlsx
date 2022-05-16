using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(RenameAttribute))]
    public class RenameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RenameAttribute myAttribute = attribute as RenameAttribute;
            label = myAttribute.GetLabel(property);
            EditorGUI.PropertyField(position, property, label);
        }
    }
}