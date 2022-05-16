using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UITradeStoreInvenSelectData : IUIData
    {
        public bool exceptPrivateExist; // 개인상점을 위한 아이템 셀렉 팝업인지. -> 이미 개인상점에 등록된 아이템은 보이지 않게 한다.
    }

    public class UITradeStoreInvenSelect : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UITabHelper tabHelper;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        TradeStoreInvenSelectPresenter presenter;
        UITradeStoreInvenSelectData data;
        List<ItemInfo> showItemList;
        int curTabIndex;

        protected override void OnInit()
        {
            presenter = new TradeStoreInvenSelectPresenter();
            presenter.AddEvent();

            EventDelegate.Add(btnClose.OnClick, OnClickBtnClose);
            EventDelegate.Add(tabHelper[0].OnChange, OnClickBtnEquipmentTab);
            EventDelegate.Add(tabHelper[1].OnChange, OnClickBtnCardTab);
            EventDelegate.Add(tabHelper[2].OnChange, OnClickBtnPartsTab);

            curTabIndex = 0;

            wrapper.SetRefreshCallback(OnElementRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnClose.OnClick, OnClickBtnClose);
            EventDelegate.Remove(tabHelper[0].OnChange, OnClickBtnEquipmentTab);
            EventDelegate.Remove(tabHelper[1].OnChange, OnClickBtnCardTab);
            EventDelegate.Remove(tabHelper[2].OnChange, OnClickBtnPartsTab);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._34100; // 장비 목록
            tabHelper[0].LocalKey = LocalizeKey._34101; // 장비
            tabHelper[1].LocalKey = LocalizeKey._34102; // 카드
            tabHelper[2].LocalKey = LocalizeKey._34103; // 재료
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is UITradeStoreInvenSelectData)
            {
                this.data = data as UITradeStoreInvenSelectData;
                Refresh(isNewList: true);
                return;
            }
        }

        void Refresh(bool isNewList = false)
        {
            if (isNewList)
            {
                showItemList = GetInventoryItemList();

                showItemList.Sort((a, b) =>
                {
                    if (a.IsShadow != b.IsShadow)
                        return a.IsShadow ? 1 : -1;

                    if (a.RoPoint != b.RoPoint)
                    {
                        if (!a.CanTrade)
                            return 1;
                        if (!b.CanTrade)
                            return -1;
                    }

                    return a.ItemId - b.ItemId;
                });

                wrapper.Resize(showItemList.Count);
                wrapper.SetProgress(0);
            }
        }

        void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<TradeStoreInvenSelectSlot>();
            slot.SetData(presenter, showItemList[index]);
        }

        void SetTab(int idx)
        {
            if (!UIToggle.current.value)
                return;
            curTabIndex = idx;
            tabHelper[curTabIndex].Value = true;
            Refresh(isNewList: true);
        }

        void OnClickBtnEquipmentTab()
        {
            SetTab(0);
        }

        void OnClickBtnCardTab()
        {
            SetTab(1);
        }

        void OnClickBtnPartsTab()
        {
            SetTab(2);
        }

        void OnClickBtnClose()
        {
            CloseUI();
        }

        void CloseUI()
        {
            UI.Close<UITradeStoreInvenSelect>();
        }

        List<ItemInfo> GetInventoryItemList()
        {
            switch (curTabIndex)
            {
                case 0: return presenter.GetInventoryItemList(typeof(EquipmentItemInfo), data.exceptPrivateExist);
                case 1: return presenter.GetInventoryItemList(typeof(CardItemInfo), data.exceptPrivateExist);
                case 2: return presenter.GetInventoryItemList(typeof(PartsItemInfo), data.exceptPrivateExist);
            }

            return null;
        }
    }
}