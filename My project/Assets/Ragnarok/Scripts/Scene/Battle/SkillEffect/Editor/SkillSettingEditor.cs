using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CustomEditor(typeof(SkillSetting))]
    public sealed class SkillSettingEditor : Editor
    {
        private const float THICKNESS = 2f;

        Vector2 scrollPosition;
        SkillSetting setting;

        private string selectedPropertyPath;
        private int selectedIndex;

        public float maxTime = -1;
        public System.Action<SerializedProperty> onSelect;

        void OnEnable()
        {
            setting = target as SkillSetting;
        }

        void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            if (maxTime == -1f)
            {
                base.OnInspectorGUI();
                return;
            }

            if (maxTime == 0f)
                return;

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("자동정렬", EditorStyles.miniButton))
                    Sort();
            }

            using (var gui = new GUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = gui.scrollPosition;

                DrawTimeline("[대미지]");
                DrawTimeline(nameof(SkillSetting.hitTime), -1, setting.hitTime, 0);

                DrawTimeline("[이펙트]", nameof(SkillSetting.arrVfx));
                for (int i = 0; i < setting.arrVfx.Length; i++)
                {
                    DrawTimeline(nameof(SkillSetting.arrVfx), i, setting.arrVfx[i].time, setting.arrVfx[i].duration);
                }

                DrawTimeline("[사운드]", nameof(SkillSetting.arrSound));
                for (int i = 0; i < setting.arrSound.Length; i++)
                {
                    DrawTimeline(nameof(SkillSetting.arrSound), i, setting.arrSound[i].time, setting.arrSound[i].duration);
                }

                DrawTimeline("[발사체]", nameof(SkillSetting.arrProjectile));
                for (int i = 0; i < setting.arrProjectile.Length; i++)
                {
                    DrawTimeline(nameof(SkillSetting.arrProjectile), i, setting.arrProjectile[i].time, setting.arrProjectile[i].duration);
                }
            }
        }

        private void DrawTimeline(string title, string arrayPropertyPath = null)
        {
            GUILayout.Space(2f);
            GUILayout.Label(GUIContent.none, "IN Title");
            GUILayout.Space(-12f);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title);
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(arrayPropertyPath)))
                {
                    if (GUILayout.Button("추가", EditorStyles.miniButton))
                        Add(new System.Tuple<string, int>(arrayPropertyPath, serializedObject.FindProperty(arrayPropertyPath).arraySize));
                }
            }
        }

        private void DrawTimeline(string propertyPath, int index, int time, int duration)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            Rect rectBar = rect;
            rectBar.xMin += 30f;
            rectBar.width += 3f;

            bool isSelected = string.Equals(selectedPropertyPath, propertyPath) && (selectedIndex == index);
            EditorGUI.DrawRect(rectBar, isSelected ? new Color(0.1f, 0.1f, 0.2f) : Color.black);

            GUI.backgroundColor = Color.clear;
            if (GUI.Button(rectBar, GUIContent.none))
            {
                if (Event.current.button == 0)
                {
                    selectedPropertyPath = propertyPath;
                    selectedIndex = index;

                    SerializedProperty selectedProperty = serializedObject.FindProperty(propertyPath);

                    if (selectedProperty.isArray)
                        selectedProperty = selectedProperty.GetArrayElementAtIndex(index);

                    onSelect?.Invoke(selectedProperty);
                }
                else
                {
                    if (index >= 0)
                    {
                        NGUIContextMenu.AddItem("추가", false, Add, new System.Tuple<string, int>(propertyPath, index));
                        NGUIContextMenu.AddItem("삭제", false, Remove, new System.Tuple<string, int>(propertyPath, index));
                        NGUIContextMenu.Show();
                    }
                }
            }
            GUI.backgroundColor = Color.white;

            Rect rectPlay = rect;
            rectPlay.width = 28f;
            GUI.Label(rectPlay, time.ToString(), EditorStyles.miniLabel);

            float xMin = Mathf.Lerp(rectBar.x, rectBar.xMax, time * 0.01f / maxTime);
            float xMax = Mathf.Lerp(rectBar.x, rectBar.xMax, (time + duration) * 0.01f / maxTime);
            float halfThickness = THICKNESS * 0.5f;
            EditorGUI.DrawRect(Rect.MinMaxRect(xMin - halfThickness, rectBar.yMin, xMax + halfThickness, rectBar.yMax), Color.white);
        }

        private void Sort()
        {
            
        }

        private void Add(object obj)
        {
            var tuple = obj as System.Tuple<string, int>;
            SerializedProperty sp = serializedObject.FindProperty(tuple.Item1);
            sp.InsertArrayElementAtIndex(tuple.Item2);
            serializedObject.ApplyModifiedProperties();
        }

        private void Remove(object obj)
        {
            var tuple = obj as System.Tuple<string, int>;

            //if (!EditorUtility.DisplayDialog("삭제", $"[{tuple.Item2}]를 삭제하시겠습니까?", "삭제", "취소"))
            //    return;

            SerializedProperty sp = serializedObject.FindProperty(tuple.Item1);
            sp.DeleteArrayElementAtIndex(tuple.Item2);

            serializedObject.ApplyModifiedProperties();
        }
    }
}