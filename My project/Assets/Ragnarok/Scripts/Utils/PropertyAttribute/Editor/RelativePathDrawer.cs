﻿using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(RelativePathAttribute))]
    public class RelativePathDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RelativePathAttribute myAttribute = attribute as RelativePathAttribute;
            label = myAttribute.GetLabel(property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label, new GUIContent("This type has not supported."));
                return;
            }

            if (!myAttribute.isHideDisplayName)
                position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginProperty(position, label, property);
            {
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

                    string title = myAttribute.browsePanelTitle;
                    string oldPath = PathUtils.GetPath(property.stringValue, PathUtils.PathType.Absolute);
                    string newPath = null;

                    switch (myAttribute.pathType)
                    {
                        case PathType.Folder:
                            if (string.IsNullOrEmpty(title))
                                title = "Select Folder";

                            newPath = EditorUtility.OpenFolderPanel(title, oldPath, string.Empty);
                            break;

                        case PathType.File:
                            if (string.IsNullOrEmpty(title))
                                title = "Select File";

                            newPath = EditorUtility.OpenFilePanel(title, oldPath, myAttribute.extension);
                            break;
                    }

                    if (!string.IsNullOrEmpty(newPath) && !newPath.Equals(oldPath))
                    {
                        property.stringValue = PathUtils.GetGUID(newPath, PathUtils.PathType.Absolute);
                    }
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
            }
            EditorGUI.EndProperty();
        }
    }
}