using UnityEngine;

namespace Ragnarok
{
    public class RenameAttribute : PropertyAttribute
    {
        public string displayName;
        public string tooltip;

#if UNITY_EDITOR
        private GUIContent label;
        public GUIContent GetLabel(UnityEditor.SerializedProperty property)
        {
            return label ?? (label = MakeLabel(property));
        }

        private GUIContent MakeLabel(UnityEditor.SerializedProperty property)
        {
            if (string.IsNullOrEmpty(displayName))
                displayName = property.displayName;

            if (string.IsNullOrEmpty(tooltip))
                tooltip = property.tooltip;

            return new GUIContent(displayName, tooltip);
        }
#endif
    }
}