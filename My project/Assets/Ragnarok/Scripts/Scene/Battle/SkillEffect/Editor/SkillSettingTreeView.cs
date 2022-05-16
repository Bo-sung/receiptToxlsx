using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    using Element = TreeViewItem<SkillSettingElement>;

    public sealed class SkillSettingTreeView : TreeViewWithTreeModel<SkillSettingElement>
    {
        private class Model : TreeModel<SkillSettingElement>
        {
            private readonly SkillSettingContainer skillSettings;

            public Model() : base(new List<SkillSettingElement> { new SkillSettingElement() })
            {
                skillSettings = AssetDatabase.LoadAssetAtPath<SkillSettingContainer>(SkillPreviewWindow.SKILL_SETTING_ASSET_PATH);

                SkillSetting[] settings = skillSettings.GetArray();
                for (int i = 0; i < settings.Length; i++)
                {
                    AddElement(new SkillSettingElement(settings[i]), root, i);
                }

                // 초기 세팅 끝난 후에
                modelChanged += RefreshData;
            }

            private void RefreshData()
            {
                var arrData = root.children.Cast<SkillSettingElement>().ToArray();

                SerializedObject soSkillSettings = new SerializedObject(skillSettings);
                SerializedProperty spArray = soSkillSettings.FindProperty("array");
                spArray.ClearArray();

                for (int i = 0; i < arrData.Length; i++)
                {
                    spArray.InsertArrayElementAtIndex(i);
                    spArray.GetArrayElementAtIndex(i).objectReferenceValue = arrData[i].setting;
                }

                soSkillSettings.ApplyModifiedProperties();

                // Save
                EditorUtility.SetDirty(skillSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private const int COLUMN_NAME = 0;
        private const int COLUMN_ID = 1;
        private const int COLUMN_ANI_NAME = 2;

        private readonly SearchField searchField;
        private readonly GenericMenu contextMenu;

        public System.Action<SkillSetting> onSelect;

        public SkillSettingTreeView() : base(new TreeViewState(), new Model())
        {
            searchField = new SearchField();

            contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("id 변경"), false, RenameID);
            contextMenu.AddItem(new GUIContent("ani_name 변경"), false, RenameAniName);
            contextMenu.AddItem(new GUIContent("제거"), false, Delete);

            // 기본 설정
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            multiColumnHeader = CreateHeader();
            multiColumnHeader.ResizeToFit();

            // 이벤트
            multiColumnHeader.sortingChanged += OnSortingChanged;
            searchField.downOrUpArrowKeyPressed += SetFocusAndEnsureSelectedItem;

            Reload();
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(id: -1, depth: -1);
            root.AddChild(new TreeViewItem(id: 0, depth: 0));

            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            IList<TreeViewItem> rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        public override void OnGUI(Rect rect)
        {
            const float HEADER_HEIGHT = 20f;

            // Draw Header
            Rect top = rect;
            top.height = HEADER_HEIGHT;
            searchString = searchField.OnGUI(top, searchString);

            Rect bottom = rect;
            bottom.y = HEADER_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
            bottom.height = rect.height - bottom.y;

            base.OnGUI(bottom);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            Element item = args.item as Element;

            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                CellGUI(args.GetCellRect(i), item.data, args.GetColumn(i), ref args);
            }
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void ContextClicked()
        {
            if (HasSelection())
                contextMenu.ShowAsContext(); // Context Menu 띄움
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0)
                return;

            int id = selectedIds[0];
            onSelect?.Invoke(treeModel.Find(id).setting);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            // Rename 취소
            if (!args.acceptedRename)
                return;

            // 같은 이름으로 변경
            if (string.Equals(args.originalName, args.newName))
                return;

            // itemID 와 originalName 이 같을 경우: id 변경
            if (args.itemID.ToString().Equals(args.originalName))
            {
                int newID;
                if (int.TryParse(args.newName, out newID))
                {
                    SkillSettingElement element = treeModel.Find(newID);

                    if (element == null)
                    {
                        treeModel.Find(args.itemID).setting.id = newID;
                        EditorUtility.SetDirty(treeModel.Find(args.itemID).setting);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogError($"이미 존재하는 ID 입니다: {args.newName}");
                    }
                }
                else
                {
                    Debug.LogError($"id는 문자열이 포함될 수 없습니다: {args.newName}");
                }
            }
            else
            {
                // 애니 이름 변경
                treeModel.Find(args.itemID).setting.aniName = args.newName;
                EditorUtility.SetDirty(treeModel.Find(args.itemID).setting);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                onSelect?.Invoke(treeModel.Find(args.itemID).setting);
            }
        }

        private void RenameID()
        {
            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count == 0)
                return;

            int id = selectedIds[0];
            BeginRename(new TreeViewItem(id, 0, treeModel.Find(id).setting.id.ToString()));
        }

        private void RenameAniName()
        {
            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count == 0)
                return;

            int id = selectedIds[0];
            BeginRename(new TreeViewItem(id, 0, treeModel.Find(id).setting.aniName.ToString()));
        }

        private void Delete()
        {
            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count == 0)
                return;

            int id = selectedIds[0];
            SkillSetting setting = treeModel.Find(id).setting;

            if (!EditorUtility.DisplayDialog("삭제", $"[{setting.name}]를 삭제하시겠습니까?", "삭제", "취소"))
                return;

            string path = AssetDatabase.GetAssetPath(setting);
            FileUtil.DeleteFileOrDirectory(path);

            treeModel.RemoveElements(selectedIds);
        }

        private void CellGUI(Rect rect, SkillSettingElement element, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref rect);

            switch (column)
            {
                case COLUMN_NAME:
                    if (GUI.Button(rect, element.setting.name, EditorStyles.textField))
                        EditorGUIUtility.PingObject(element.setting);
                    break;

                case COLUMN_ID:
                    DefaultGUI.Label(rect, element.setting.id.ToString(), args.selected, args.focused);
                    break;

                case COLUMN_ANI_NAME:
                    DefaultGUI.Label(rect, element.setting.aniName, args.selected, args.focused);
                    break;
            }
        }

        private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return;

            SortByMultipleColumns();
            TreeToList(root, rows);
            Repaint();
        }

        private void SortByMultipleColumns()
        {
            int[] sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<Element>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                int key = sortedColumns[i];
                bool ascending = multiColumnHeader.IsSortedAscending(key);

                switch (key)
                {
                    case COLUMN_NAME:
                        orderedQuery = orderedQuery.ThenBy(item => item.data.setting.name, ascending);
                        break;

                    case COLUMN_ID:
                        orderedQuery = orderedQuery.ThenBy(item => item.data.setting.id, ascending);
                        break;

                    case COLUMN_ANI_NAME:
                        orderedQuery = orderedQuery.ThenBy(item => item.data.setting.aniName, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        private void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new System.NullReferenceException("root");
            if (result == null)
                throw new System.NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        private IOrderedEnumerable<Element> InitialOrder(IEnumerable<Element> myTypes, int[] history)
        {
            int key = history[0];
            bool ascending = multiColumnHeader.IsSortedAscending(key);

            switch (key)
            {
                case COLUMN_NAME:
                    return myTypes.Order(item => item.data.setting.name, ascending);

                case COLUMN_ID:
                    return myTypes.Order(item => item.data.setting.id, ascending);

                case COLUMN_ANI_NAME:
                    return myTypes.Order(item => item.data.setting.aniName, ascending);
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(key)}] {nameof(key)} = {key}");
        }

        private MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[3]
            {
                CreateColumn("name"),
                CreateColumn("id"),
                CreateColumn("ani_name"),
            };

            //columns[COLUMN_NAME].allowToggleVisibility = true;
            //columns[COLUMN_ID].allowToggleVisibility = false;
            columns[COLUMN_ID].width = 16f;
            //columns[COLUMN_ANI_NAME].allowToggleVisibility = false;

            MultiColumnHeaderState state = new MultiColumnHeaderState(columns)
            {
                //visibleColumns = new int[2] { COLUMN_ID, COLUMN_ANI_NAME }
            };

            return new MultiColumnHeader(state);
        }

        private MultiColumnHeaderState.Column CreateColumn(string name)
        {
            return new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent(name),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Right,
                autoResize = true,
            };
        }
    }
}