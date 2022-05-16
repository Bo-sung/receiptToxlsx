using UnityEngine;

namespace Ragnarok
{
    public sealed class UICardInvenData : IUIData
    {
        public readonly ItemInfo equipmentItemInfo;
        public readonly byte index;

        public UICardInvenData(ItemInfo equipmentItemInfo, byte index)
        {
            this.equipmentItemInfo = equipmentItemInfo;
            this.index = index;
        }
    }

    public sealed class UICardInven : UICanvas, CardInvenPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        ItemInfo[] arrayInfos;
        CardInvenPresenter presenter;
        UICardInvenData data;

        protected override void OnInit()
        {
            presenter = new CardInvenPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnExit.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
            this.data = data as UICardInvenData;

            arrayInfos = presenter.GetCardItemInfos(this.data.equipmentItemInfo.ClassType, this.data.equipmentItemInfo.IsShadow);
            if (arrayInfos.Length == 0)
            {
                if (this.data.equipmentItemInfo.IsShadow)
                {
                    UI.ShowToastPopup(LocalizeKey._90289.ToText()); // 장착 가능한 쉐도우 카드가 없습니다.
                    CloseUI();
                    return;
                }

                UI.Show<UINoCard>();
                CloseUI();
                return;
            }

            UpdateView();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labTitle.LocalKey = LocalizeKey._17000; // 카드 목록
        }

        void CloseUI()
        {
            UI.Close<UICardInven>();
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UICardInfoSlot slot = go.GetComponent<UICardInfoSlot>();
            slot.SetSubData(data.equipmentItemInfo, data.index);
            slot.SetData(null, arrayInfos[index]);
        }

        public void UpdateView()
        {
            arrayInfos = presenter.GetCardItemInfos(data.equipmentItemInfo.ClassType, data.equipmentItemInfo.IsShadow);

            wrapper.Resize(arrayInfos.Length);
        }
    }
}
