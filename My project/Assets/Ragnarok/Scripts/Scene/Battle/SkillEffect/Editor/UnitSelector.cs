using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public class UnitSelector
    {
        public delegate void SkillRangeChangeEvent(float skillRange);
        public delegate void SkillAreaChangeEvent(bool isTargetPoint, float skillArea);

        GameObject unit;
        GameObject target;

        int skillRange;
        bool isTargetPoint;
        int skillArea;

        public System.Action<GameObject> onSelectUnit;
        public System.Action<GameObject> onSelectTarget;
        public SkillRangeChangeEvent onSkillRangeChange;
        public SkillAreaChangeEvent onSkillAreaChange;

        public void Draw()
        {
            using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
            {
                GUILayout.Label("[유닛]");

                EditorGUI.BeginChangeCheck();
                GameObject goUnit = EditorGUILayout.ObjectField(unit, typeof(GameObject), false, GUILayout.ExpandWidth(false)) as GameObject;
                if (EditorGUI.EndChangeCheck())
                    SelectUnit(goUnit);

                /**********************************************************************
                 * Unicode 2026 (Ellipsis)
                 * https://en.wikipedia.org/wiki/Ellipsis#Computer_representations
                 **********************************************************************/
                if (GUILayout.Button("\u2026", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    GUI.FocusControl(string.Empty);
                    ShowSelector(SelectUnit);
                }

                EditorGUI.BeginChangeCheck();
                int skillRange = EditorGUILayout.IntField("skill_range", this.skillRange, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                    SetSkillRange(skillRange);

                GUILayout.Label("[타겟]");

                EditorGUI.BeginChangeCheck();
                GameObject goTarget = EditorGUILayout.ObjectField(target, typeof(GameObject), false, GUILayout.ExpandWidth(false)) as GameObject;
                if (EditorGUI.EndChangeCheck())
                    SelectTarget(goTarget);

                /**********************************************************************
                 * Unicode 2026 (Ellipsis)
                 * https://en.wikipedia.org/wiki/Ellipsis#Computer_representations
                 **********************************************************************/
                if (GUILayout.Button("\u2026", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    GUI.FocusControl(string.Empty);
                    ShowSelector(SelectTarget);
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Label("point_type");
                bool isTargetPoint = GUILayout.Toggle(this.isTargetPoint, this.isTargetPoint ? "타겟" : "시전자", EditorStyles.toolbar, GUILayout.Width(40f));
                int skillArea = EditorGUILayout.IntField("skill_area", this.skillArea, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                    SetSkillArea(isTargetPoint, skillArea);
            }
        }

        public void SetSkillRange(int skillRange)
        {
            this.skillRange = skillRange;
            onSkillRangeChange?.Invoke(skillRange * 0.01f);
        }

        public void SetSkillArea(bool isTargetPoint, int skillArea)
        {
            this.skillArea = skillArea;
            this.isTargetPoint = isTargetPoint;
            onSkillAreaChange?.Invoke(isTargetPoint, skillArea * 0.01f);
        }

        public bool SetUnit(GameObject go)
        {
            if (unit == go)
                return false;

            SelectUnit(go);
            return true;
        }

        public bool SetTarget(GameObject go)
        {
            if (target == go)
                return false;

            SelectTarget(go);
            return true;
        }

        private void SelectUnit(GameObject go)
        {
            unit = go;
            onSelectUnit?.Invoke(go);
        }

        private void SelectTarget(GameObject go)
        {
            if (target == go)
                return;

            target = go;
            onSelectTarget?.Invoke(go);
        }

        private void ShowSelector(System.Action<GameObject> action)
        {
            SelectorWindow window = EditorWindow.GetWindow<SelectorWindow>(title: "Selector", utility: true, focus: true);
            window.minSize = new Vector2(480f, 480f);
            window.onSelect = action;
            window.Focus();
            window.Repaint();
            window.Show();
        }

        private sealed class SelectorWindow : EditorWindow
        {
            private const string TITLE = "Select Unit";
            private readonly string[] TOOLBAR_NAME = { "캐릭터", "몬스터", };

            WindowSplitter splitter;
            PrefabTreeView characterTreeView, monsterTreeView;
            Editor editor;
            int selectedIndex;

            public System.Action<GameObject> onSelect;

            PrefabTreeView CurTreeView
            {
                get
                {
                    switch (selectedIndex)
                    {
                        case 0: return characterTreeView;
                        case 1: return monsterTreeView;
                    }

                    throw new System.ArgumentException($"[올바르지 않은 {nameof(selectedIndex)}] {nameof(selectedIndex)} = {selectedIndex}");
                }
            }

            void OnEnable()
            {
                if (splitter == null)
                    splitter = new HorizontalSplitter(Repaint);

                if (characterTreeView == null)
                {
                    EditorUtility.DisplayProgressBar("Loading UnitSelectorWindow", "Character", 0.5f);
                    characterTreeView = new PrefabTreeView(SkillPreviewWindow.CHARACTER_ASSET_PATH) { onChange = Change, onSelect = Select };
                }

                if (monsterTreeView == null)
                {
                    EditorUtility.DisplayProgressBar("Loading UnitSelectorWindow", "Monster", 1f);
                    monsterTreeView = new PrefabTreeView(SkillPreviewWindow.MONSTER_ASSET_PATH) { onChange = Change, onSelect = Select };
                }

                EditorUtility.ClearProgressBar();
            }

            void OnGUI()
            {
                splitter.OnGUI(Screen.safeArea);
                CurTreeView.OnGUI(splitter[0]);

                Rect rect = splitter[1];

                Rect head = rect;
                const float HEADER_HEIGHT = 20f;
                head.height = HEADER_HEIGHT;

                EditorGUI.BeginChangeCheck();
                selectedIndex = GUI.Toolbar(head, selectedIndex, TOOLBAR_NAME);
                if (EditorGUI.EndChangeCheck())
                {
                    Change(null);
                    CurTreeView.searchString = string.Empty;
                }

                Rect bottom = rect;
                bottom.y = HEADER_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
                bottom.height = rect.height - bottom.y;

                if (editor)
                {
                    editor.OnPreviewGUI(bottom, EditorStyles.helpBox);
                }
                else
                {
                    bottom.y = 0f;
                    EditorGUI.DropShadowLabel(bottom, "선택할 유닛을 더블클릭하세요");
                }
            }

            void Change(GameObject obj)
            {
                editor = obj == null ? null : Editor.CreateEditor(obj);
            }

            void Select(GameObject obj)
            {
                Close();
                onSelect?.Invoke(obj);
            }
        }
    }
}