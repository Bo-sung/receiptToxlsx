using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(GUIDFieldAttribute))]
    public class GUIDFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIDFieldAttribute myAttribute = attribute as GUIDFieldAttribute;
            label = myAttribute.GetLabel(property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label, new GUIContent("This type has not supported."));
                return;
            }

            if (!myAttribute.isHideDisplayName)
                position = EditorGUI.PrefixLabel(position, label);

            string guid = property.stringValue;

            EditorGUI.BeginProperty(position, label, property);
            {
                switch (myAttribute.guidFieldType)
                {
                    case GUIDFieldType.ObjectField:
                        if (myAttribute.width > 0)
                            position.width = myAttribute.width;

                        if (myAttribute.height > 0)
                            position.height = myAttribute.height;

                        Object obj = PathUtils.GetObject(guid, myAttribute.objectType);

                        EditorGUI.BeginChangeCheck();
                        obj = EditorGUI.ObjectField(position, obj, myAttribute.objectType, false);
                        if (EditorGUI.EndChangeCheck())
                            property.stringValue = PathUtils.GetGUID(obj);
                        break;

                    case GUIDFieldType.Browse:
                        const float BROWSE_BUTTON_WIDTH = 18f;
                        float fieldWidth = position.width - BROWSE_BUTTON_WIDTH - EditorGUIUtility.singleLineHeight;

                        // Draw Empty Box
                        position.width = fieldWidth + BROWSE_BUTTON_WIDTH;
                        GUI.Box(position, string.Empty, "ToolbarSeachTextField");

                        position.width = BROWSE_BUTTON_WIDTH;
                        /**********************************************************************
                         * Unicode 2026 (Ellipsis)
                         * https://en.wikipedia.org/wiki/Ellipsis#Computer_representations
                         **********************************************************************/

                        if (GUI.Button(position, "\u2026", EditorStyles.miniButton))
                        {
                            GUI.FocusControl(string.Empty);

                            string oldPath = PathUtils.GetPath(guid, PathUtils.PathType.Absolute);
                            string newPath = EditorUtility.OpenFilePanel(myAttribute.browsePanelTitle, oldPath, myAttribute.extension);

                            if (!string.IsNullOrEmpty(newPath) && !newPath.Equals(oldPath))
                                property.stringValue = PathUtils.GetGUID(newPath, PathUtils.PathType.Absolute);
                        }

                        position.x += position.width;
                        position.width = fieldWidth;
                        string path = PathUtils.GetPath(property.stringValue, PathUtils.PathType.Relative);
                        if (GUI.Button(position, path, EditorStyles.miniLabel))
                        {
                            Object pingObject = PathUtils.GetObject(property.stringValue, typeof(Object));
                            EditorGUIUtility.PingObject(pingObject);
                        }

                        position.x += position.width;
                        position.width = EditorGUIUtility.singleLineHeight;
                        if (GUI.Button(position, string.Empty, "ToolbarSeachCancelButton")) // "WinBtnClose"
                            property.stringValue = string.Empty;
                        break;
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GUIDFieldAttribute myAttribute = attribute as GUIDFieldAttribute;
            return myAttribute.height > 0 ? myAttribute.height : base.GetPropertyHeight(property, label);
        }
    }
}