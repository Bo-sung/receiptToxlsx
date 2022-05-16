using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICostumeInven : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labNoData;

        ItemInfo[] arrayInfos;
        ItemEquipmentSlotType slotType;

        CostumeInvenPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CostumeInvenPresenter();

            presenter.OnUpdateCostume += OnUpdateCostume;

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.OnUpdateCostume -= OnUpdateCostume;
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labTitle.LocalKey = LocalizeKey._15100; // 코스튬 목록
            labNoData.LocalKey = LocalizeKey._15101; // 해당 타입의 코스튬이 없습니다.
        }

        void OnClickedBtnExit()
        {
            UI.Close<UICostumeInven>();
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UICostumeInfoSlot slot = go.GetComponent<UICostumeInfoSlot>();
            slot.Set(new UICostumeInfoSlot.Info(arrayInfos[index], presenter));
        }

        public void Set(ItemEquipmentSlotType slotType)
        {
            this.slotType = slotType;
            arrayInfos = presenter.GetCostumeArray(slotType);
            wrapper.Resize(arrayInfos.Length);
            labNoData.gameObject.SetActive(arrayInfos.Length == 0);
        }

        private void OnUpdateCostume()
        {
            Set(slotType);
        }
    }
}