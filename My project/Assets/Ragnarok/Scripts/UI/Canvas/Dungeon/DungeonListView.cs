using UnityEngine;
using Ragnarok.View;

namespace Ragnarok
{
    public class DungeonListView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        public event System.Action<DungeonType> OnSelect;

        DungeonElement[] elements;

        protected override void Awake()
        {
            base.Awake();
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SetClickCallback(OnItemClick);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        public override void Show()
        {
            base.Show();
            wrapper.SetProgress(0);
        }

        protected override void OnLocalize()
        {
        }

        void OnItemClick(GameObject go, int index)
        {
            DungeonType dungeonType = elements[index].DungeonType;
            if (!elements[index].IsOpenedDungeon())
                return;

            OnSelect?.Invoke(dungeonType);
        }

        public void SetData(DungeonElement[] elements)
        {
            this.elements = elements;
            Refresh();
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIDungeonInfoSlot ui = go.GetComponent<UIDungeonInfoSlot>();
            bool activeNotice = elements[index].IsOpenedDungeon() && // 컨텐츠 해금 후
                (elements[index].GetFreeCount() > 0 || elements[index].PossibleFreeReward()); // 무료입장 가능하거나 무료보상을 받을 수 있을경우
            ui.SetData(elements[index], activeNotice);
        }

        void UpdateNotice()
        {

        }

        public void Refresh()
        {
            if (elements == null)
                return;

            wrapper.Resize(elements.Length);
        }
    }
}