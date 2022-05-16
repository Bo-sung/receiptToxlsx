using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEquipmentInvenData : IUIData
    {
        public readonly ItemEquipmentSlotType slotType;

        public UIEquipmentInvenData(ItemEquipmentSlotType slotType)
        {
            this.slotType = slotType;
        }
    }

    public sealed class UIEquipmentInven : UICanvas<EquipmentInvenPresenter>, EquipmentInvenPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labNoData;

        ItemInfo[] arrayInfos;
        UIEquipmentInvenData data;

        long strongestEquipmentNo; // 가장 강한 장비

        protected override void OnInit()
        {
            presenter = new EquipmentInvenPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data != null)
            {
                this.data = data as UIEquipmentInvenData;
                EquipmentItemInfo strongestEquipment = presenter.GetStrongestEquipment(this.data.slotType);
                strongestEquipmentNo = strongestEquipment == null ? 0L : strongestEquipment.ItemNo;
            }
            Refresh();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labTitle.LocalKey = LocalizeKey._15000; // 장비 목록
            labNoData.LocalKey = LocalizeKey._15002; // 해당 타입의 장비가 없습니다.
        }

        void OnClickedBtnExit()
        {
            UI.Close<UIEquipmentInven>();
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIEquipmentSlot slot = go.GetComponent<UIEquipmentSlot>();
            slot.SetData(arrayInfos[index]);

            if (arrayInfos[index].IsEquipped || arrayInfos[index].IsShadow)
            {
                slot.SetPowerUpIcon(false);
            }
            else
            {
                slot.SetPowerUpIcon(arrayInfos[index].ItemNo == strongestEquipmentNo);
            }
        }

        public void Refresh()
        {
            arrayInfos = presenter.GetEquipmentArray(data.slotType);
            wrapper.Resize(arrayInfos.Length);
            labNoData.gameObject.SetActive(arrayInfos.Length == 0);
        }
    }
}