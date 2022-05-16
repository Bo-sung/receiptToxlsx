using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public abstract class ContainerTreeView<TContainer, TValue> : TreeViewWithTreeModel<ContainerTreeView<TContainer, TValue>.Element>
        where TContainer : StringAssetContainer<TValue>
        where TValue : Object
    {
        public sealed class Element : TreeElement
        {
            public readonly TValue obj;

            /// <summary>
            /// Root
            /// </summary>
            public Element()
                : base(id: -1, depth: -1, name: string.Empty)
            {

            }

            public Element(TValue obj, int id)
                : base(id: id, depth: 0, name: obj == null ? string.Empty : obj.name)
            {
                this.obj = obj;
            }
        }

        private class Model : TreeModel<Element>
        {
            public Model(string assetName) : base(new List<Element> { new Element() })
            {
                TContainer container = AssetDatabase.LoadAssetAtPath<TContainer>(assetName);

                if (container == null)
                {
                    Debug.LogError(assetName);
                    return;
                }

                TValue[] prefabs = container.GetArray();
                for (int i = 0; i < prefabs.Length; i++)
                {
                    AddElement(new Element(prefabs[i], i), root, i);
                }
            }
        }

        private readonly SearchField searchField;

        public System.Action<TValue> onChange;
        public System.Action<TValue> onSelect;

        public ContainerTreeView(string assetName) : base(new TreeViewState(), new Model(assetName))
        {
            searchField = new SearchField();

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

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0)
                return;

            int id = selectedIds[0];
            TreeViewItem<Element> item = FindItem(id, rootItem) as TreeViewItem<Element>;
            onChange?.Invoke(item.data.obj);
        }

        protected override void DoubleClickedItem(int id)
        {
            TreeViewItem<Element> item = FindItem(id, rootItem) as TreeViewItem<Element>;
            onSelect?.Invoke(item.data.obj);
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

            var myTypes = rootItem.children.Cast<TreeViewItem<Element>>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                int key = sortedColumns[i];
                bool ascending = multiColumnHeader.IsSortedAscending(key);
                orderedQuery = orderedQuery.ThenBy(item => item.data.name, ascending);
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

        private IOrderedEnumerable<TreeViewItem<Element>> InitialOrder(IEnumerable<TreeViewItem<Element>> myTypes, int[] history)
        {
            int key = history[0];
            bool ascending = multiColumnHeader.IsSortedAscending(key);
            return myTypes.Order(item => item.data.name, ascending);
        }

        private MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[1]
            {
                CreateColumn("name"),
            };

            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
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
                allowToggleVisibility = false,
            };
        }
    }

    public sealed class PrefabTreeView : ContainerTreeView<PrefabContainer, GameObject>
    {
        public PrefabTreeView(string assetName) : base(assetName) { }
    }

    public sealed class SoundTreeView : ContainerTreeView<SoundContainer, AudioClip>
    {
        public SoundTreeView() : base(SkillPreviewWindow.EFFECT_SOUND_ASSET_PATH) { }
    }

    public sealed class ProjectileTreeView : ContainerTreeView<ProjectileSettingContainer, ProjectileSetting>
    {
        public ProjectileTreeView() : base(SkillPreviewWindow.PROJECTILE_SETTING_ASSET_PATH) { }
    }
}