using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Ragnarok
{
    public abstract class EmptyTreeView<T> : TreeView
        where T : class
    {
        private class Element : TreeViewItem
        {
            public readonly T t;

            public Element(T t, int id) : base(id)
            {
                this.t = t;
            }
        }

        private readonly List<Element> itemList;

        public System.Action<T> onSingleClicked; // 일반선택
        public System.Action<T> onContextClicked; // 우클릭
        public System.Action<T> onDoubleClicked; // 더블클릭

        public EmptyTreeView() : base(new TreeViewState())
        {
            itemList = new List<Element>();

            // 기본 설정
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            multiColumnHeader = CreateHeader();
            multiColumnHeader.ResizeToFit();

            Reload();
        }

        /// <summary>
        /// TreeItem 목록
        /// </summary>
        public void SetData(List<T> list)
        {
            itemList.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                itemList.Add(new Element(list[i], i));
            }

            SetSelection(new List<int>()); // 선택 초기화

            Reload();
        }

        protected abstract MultiColumnHeader CreateHeader();

        protected abstract void DrawColumn(Rect rect, T t, int column, ref RowGUIArgs args);

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem { id = -1, depth = -1 };
            SetupParentsAndChildrenFromDepths(root, itemList.ConvertAll(data => (TreeViewItem)data));
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            Element element = args.item as Element;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), element.t, args.GetColumn(i), ref args);
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            T data = selectedIds.Count == 0 ? null : Find(selectedIds[0]);
            onSingleClicked?.Invoke(data);
        }

        protected override void DoubleClickedItem(int id)
        {
            T data = Find(id);
            onDoubleClicked?.Invoke(data);
        }

        protected override void ContextClickedItem(int id)
        {
            T data = Find(id);
            onContextClicked?.Invoke(data);
        }

        private void CellGUI(Rect rect, T t, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref rect);
            DrawColumn(rect, t, column, ref args);
        }

        protected MultiColumnHeaderState.Column CreateColumn(string name, float width)
        {
            MultiColumnHeaderState.Column column = CreateColumn(name);

            column.width = width;
            column.minWidth = width;
            column.maxWidth = width;

            return column;
        }

        protected MultiColumnHeaderState.Column CreateColumn(string name)
        {
            return new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent(name),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Right,
                allowToggleVisibility = false,
                canSort = false,
            };
        }

        private T Find(int id)
        {
            return itemList.Find(data => data.id == id)?.t;
        }
    }
}