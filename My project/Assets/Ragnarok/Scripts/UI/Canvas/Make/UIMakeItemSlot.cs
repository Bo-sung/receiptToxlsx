using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMakeItemSlot : UIInfo<MakePresenter, MakeInfo>
    {
        [SerializeField] UIRewardHelper item;
        [SerializeField] UIButtonHelper button;
        [SerializeField] GameObject selectBase;
        [SerializeField] UIRewardHelper disableItem;
        [SerializeField] GameObject goDisable, goEnable;
        [SerializeField] UISprite enableBG;
        [SerializeField] GameObject equipmentMatReady;
        [SerializeField] GameObject allMatReady;
        [SerializeField] GameObject goWarning;
        [SerializeField] UILabelHelper labelWarning;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(button.OnClick, OnClickedItem);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(button.OnClick, OnClickedItem);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            item.SetData(info.Reward);
            selectBase.SetActive(info == presenter.Info);
            disableItem.SetData(info.Reward);
            equipmentMatReady.SetActive(false);
            allMatReady.SetActive(false);
            goWarning.SetActive(false);

            if (info.EnableType == EnableType.Disable)
            {
                goDisable.SetActive(true);
                goEnable.SetActive(false);
            }
            else if (info.EnableType == EnableType.Enable)
            {
                goDisable.SetActive(false);
                goEnable.SetActive(true);

                if (info.Reward.ItemData.rating > 0)
                    enableBG.spriteName = GetRatingBG(info.Reward.ItemData.rating);
                else
                    enableBG.spriteName = GetRatingBG(0);

                if (info.IsMake)
                    allMatReady.SetActive(true);
                else if (info.IsEquipmentMaterialReady)
                    equipmentMatReady.SetActive(true);

                // 제한 관련
                string warningMessage = info.GetMakeWarningMessage(isPopupMessage: false);
                NGUITools.SetActive(goWarning, !string.IsNullOrEmpty(warningMessage));

                // 제한 메시지
                if (labelWarning)
                {
                    labelWarning.Text = warningMessage;
                }
            }
        }

        private string GetRatingBG(int rating)
        {
            switch (rating)
            {
                case 1: return "Ui_Common_BG_Item_01";
                case 2: return "Ui_Common_BG_Item_02";
                case 3: return "Ui_Common_BG_Item_03";
                case 4: return "Ui_Common_BG_Item_04";
                case 5: return "Ui_Common_BG_Item_05";
            }

            return "Ui_Common_BG_Item_Default";
        }

        void OnClickedItem()
        {
            if (info.Reward == null)
                return;

            if (info.EnableType == EnableType.Disable)
            {
                item.OnClickedBtnShowInfo();
            }
            else if (info.EnableType == EnableType.Enable)
            {
                // 이미 선택된 제작 아이템 클릭시 아이템 정보팝업 표시
                if (presenter.Info == info)
                {
                    item.OnClickedBtnShowInfo();
                    return;
                }

                presenter.SetSelectMakeInfo(info);
            }
        }
    }
}
