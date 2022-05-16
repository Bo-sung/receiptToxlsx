using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Ragnarok
{
    [CustomEditor(typeof(SpecialRouletteConfig))]
    public sealed class SpecialRouletteConfigEditor : Editor
    {
        SpecialRouletteConfig config;

        private ReorderableList list;

        void OnEnable()
        {
            config = target as SpecialRouletteConfig;

            if (list == null)
            {
                list = new ReorderableList(serializedObject, serializedObject.FindProperty("configs")) { elementHeight = 40f };

                list.drawHeaderCallback += OnDrawHeaderCallback;
                list.drawElementCallback += OnDrawElementCallback;
                list.onAddCallback += OnAddCallback;
                list.onRemoveCallback += OnRemoveCallback;
                list.onReorderCallback += OnReorderCallback;
            }
        }

        void OnDisable()
        {
            if (list != null)
            {
                list.drawHeaderCallback -= OnDrawHeaderCallback;
                list.drawElementCallback -= OnDrawElementCallback;
                list.onAddCallback -= OnAddCallback;
                list.onRemoveCallback -= OnRemoveCallback;
                list.onReorderCallback -= OnReorderCallback;
            }
        }

        public override void OnInspectorGUI()
        {
            SerializedProperty script = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(script);

            GUILayout.Space(4f);

            list.displayRemove = list.count > 0;
            list.DoLayoutList();

            // Check Events
            Event evt = Event.current;
            switch (evt.type)
            {
                // Delete frames with supr
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.Delete && list.displayRemove && list.HasKeyboardControl() && list.index != -1)
                    {
                        OnRemoveCallback(list);
                    }
                    break;
            }
        }

        void OnDrawHeaderCallback(Rect rect)
        {
            GUI.Label(rect, "Configs");
        }

        void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty config = serializedObject.FindProperty("configs").GetArrayElementAtIndex(index);
            SerializedProperty canvasName = config.FindPropertyRelative("canvasName");
            SerializedProperty titleKey = config.FindPropertyRelative("input.titleKey");
            SerializedProperty descriptionKey = config.FindPropertyRelative("input.descriptionKey");

            const float PADDING = 10f;
            rect.x += PADDING;
            rect.width -= PADDING;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, canvasName, new GUIContent($"[{index}] Canvas Name"));

            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float xMax = rect.xMax;
            rect.width = EditorGUIUtility.labelWidth;
            GUI.Label(rect, "Local Key");
            rect.x = xMax - EditorGUIUtility.fieldWidth - 4f - EditorGUIUtility.fieldWidth;
            rect.width = EditorGUIUtility.fieldWidth;
            titleKey.intValue = EditorGUI.IntField(rect, titleKey.intValue);
            rect.x = xMax - EditorGUIUtility.fieldWidth;
            descriptionKey.intValue = EditorGUI.IntField(rect, descriptionKey.intValue);

            if (serializedObject.ApplyModifiedProperties())
            {
                Save();
            }
        }

        void OnAddCallback(ReorderableList list)
        {
            SerializedProperty configs = serializedObject.FindProperty("configs");
            configs.InsertArrayElementAtIndex(configs.arraySize);

            if (serializedObject.ApplyModifiedProperties())
            {
                Save();
            }
        }

        void OnRemoveCallback(ReorderableList list)
        {
            SerializedProperty configs = serializedObject.FindProperty("configs");
            configs.DeleteArrayElementAtIndex(list.index);

            if (serializedObject.ApplyModifiedProperties())
            {
                Save();
            }

            list.ReleaseKeyboardFocus();
        }

        void OnReorderCallback(ReorderableList list)
        {
            Save();
        }

        private void Save()
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
    }
}