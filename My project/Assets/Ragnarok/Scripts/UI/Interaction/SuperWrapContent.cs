using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class SuperWrapContent<TElement, TInfo>
        where TElement : UIElement<TInfo>
        where TInfo : class
    {
        public delegate void ContentEvent(TElement ui, int index);
        public delegate void ContentPressEvent(TElement ui, int index, bool isPress);

        private readonly SuperScrollListWrapper wrapper;

        private readonly TElement[] elements;
        private readonly Dictionary<int, TElement> elementDic;

        private TInfo[] arrayInfo;

        public event ContentEvent OnClick;
        public event ContentPressEvent OnPress;
        public event UIScrollView.OnDragNotification OnDragFinished;

        public int DataSize { get; private set; }

        public SuperWrapContent(SuperScrollListWrapper ssWrapper, GameObject prefab)
        {
            wrapper = ssWrapper;

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SetClickCallback(OnItemClick);
            wrapper.SetPressCallback(OnItemPress);
            wrapper.ScrollView.onDragFinished = OnScrollDragFinished;

            wrapper.SpawnNewList(prefab, 0, 0);

            elements = wrapper.GetComponentsInChildren<TElement>(includeInactive: true);
            elementDic = new Dictionary<int, TElement>(elements.Length, IntEqualityComparer.Default);

            foreach (var item in elements)
            {
                elementDic.Add(item.gameObject.GetInstanceID(), item);
            }
        }

        void OnItemRefresh(GameObject go, int dataIndex)
        {
            TElement ui = elementDic[go.GetInstanceID()];
            ui.SetData(arrayInfo[dataIndex]);
        }

        void OnItemClick(GameObject go, int dataIndex)
        {
            OnClick?.Invoke(elementDic[go.GetInstanceID()], dataIndex);
        }

        void OnItemPress(GameObject go, int dataIndex, bool isPress)
        {
            OnPress?.Invoke(elementDic[go.GetInstanceID()], dataIndex, isPress);
        }

        void OnScrollDragFinished()
        {
            OnDragFinished?.Invoke();
        }

        public void SetData(TInfo[] infos)
        {
            arrayInfo = infos;

            DataSize = arrayInfo == null ? 0 : arrayInfo.Length;
            wrapper.Resize(DataSize);
        }

        public void SetProgress(float progress)
        {
            wrapper.SetProgress(progress);
        }

        /// <summary>
        /// index 를 향하여 이동
        /// <see cref="UILimitCenter"/>
        /// </summary>
        public void Move(int index)
        {
            if (index < 0 || index >= DataSize)
                return;

            if (!IsShouldMove())
            {
                SetProgress(0f);
                return;
            }

            Vector3 size = new Vector3(wrapper.Grid.cellWidth, wrapper.Grid.cellHeight) * index;

            if (!wrapper.ScrollView.canMoveHorizontally)
                size.x = 0f;

            if (!wrapper.ScrollView.canMoveVertically)
                size.y = 0f;

            // MonoBehaviour의 Start로 인하여 초기화 될 수 있으므로 미리 Start 시켜줌
            if (wrapper.ScrollView.horizontalScrollBar)
                wrapper.ScrollView.horizontalScrollBar.Start();

            if (wrapper.ScrollView.verticalScrollBar)
                wrapper.ScrollView.verticalScrollBar.Start();

            Transform panelTrans = wrapper.Panel.cachedTransform;
            Vector3[] corners = wrapper.Panel.worldCorners;
            Vector3 panelCenter = (corners[2] + corners[0]) * 0.5f;
            Vector3 cp = panelTrans.InverseTransformPoint(elements[0].transform.position) + size;
            Vector3 cc = panelTrans.InverseTransformPoint(panelCenter);
            Vector3 localOffset = cp - cc;

            Vector3 move = panelTrans.localPosition - panelTrans.localPosition - localOffset;
            wrapper.ScrollView.MoveRelative(move);
            Vector3 ofs = wrapper.Panel.CalculateConstrainOffset(wrapper.ScrollView.bounds.min, wrapper.ScrollView.bounds.max);
            Vector3 targetPos = ofs + panelTrans.localPosition;
            wrapper.ScrollView.MoveRelative(-move);
            SpringPanel.Begin(wrapper.Panel.cachedGameObject, targetPos, 6f);
        }

        public void RefreshAllItems()
        {
            wrapper.RefreshAllItems();
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            for (int i = 0; i < elements.Length; ++i)
            {
                yield return elements[i];
            }
        }

        private bool IsShouldMove()
        {
            if (!wrapper.ScrollView.disableDragIfFits)
                return true;

            Vector4 clip = wrapper.Panel.finalClipRegion;
            Bounds b = wrapper.ScrollView.bounds;

            float hx = (clip.z == 0f) ? Screen.width : clip.z * 0.5f;
            float hy = (clip.w == 0f) ? Screen.height : clip.w * 0.5f;

            if (wrapper.ScrollView.canMoveHorizontally)
            {
                if (b.min.x + 0.001f < clip.x - hx) return true;
                if (b.max.x - 0.001f > clip.x + hx) return true;
            }

            if (wrapper.ScrollView.canMoveVertically)
            {
                if (b.min.y + 0.001f < clip.y - hy) return true;
                if (b.max.y - 0.001f > clip.y + hy) return true;
            }

            return false;
        }
    }

    public static class SuperWrapContentExetensions
    {
        public static SuperWrapContent<T, TInfo> Initialize<T, TInfo>(this SuperScrollListWrapper wrapper, T element)
            where T : UIElement<TInfo>
            where TInfo : class
        {
            return new SuperWrapContent<T, TInfo>(wrapper, element.gameObject);
        }
    }
}