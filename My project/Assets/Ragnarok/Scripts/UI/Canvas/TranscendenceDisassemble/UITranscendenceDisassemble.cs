using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UITranscendenceDisassemble : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] DisassembleListView disassembleListView;
        [SerializeField] DisassembleSelectPopupView disassembleSelectPopupView;

        TranscendenceDisassemblePresenter presenter;

        protected override void OnInit()
        {
            presenter = new TranscendenceDisassemblePresenter();

            popupView.OnExit += CloseUI;
            popupView.OnConfirm += CloseUI;
            disassembleListView.OnSelect += OnSelectItem;
            disassembleSelectPopupView.OnSelect += presenter.RequestDisassemble;

            presenter.OnDisassemble += CloseUI;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnExit -= CloseUI;
            popupView.OnConfirm -= CloseUI;
            disassembleListView.OnSelect -= OnSelectItem;
            disassembleSelectPopupView.OnSelect -= presenter.RequestDisassemble;

            presenter.OnDisassemble -= CloseUI;
        }

        protected override void OnShow(IUIData data = null)
        {
            disassembleSelectPopupView.Hide();
            disassembleListView.SetData(presenter.GetInputs());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._8400; // 초월 분해 장비 선택
            popupView.ConfirmLocalKey = LocalizeKey._8401; // 닫기
        }

        protected override void OnBack()
        {
            if (disassembleSelectPopupView.IsShow)
            {
                disassembleSelectPopupView.Hide();
                return;
            }
            base.OnBack();
        }

        private void CloseUI()
        {
            UI.Close<UITranscendenceDisassemble>();
        }

        private void OnSelectItem(long itemNo)
        {
            ItemInfo item = presenter.GetItemInfo(itemNo);

            if (item == null)
                return;

            if (item.IsLock)
            {
                UI.ShowToastPopup(LocalizeKey._8409.ToText()); // 잠금 된 장비는 분해할 수 없습니다.
                return;
            }

            disassembleSelectPopupView.SetData(item);

            int type = 0;
            switch (item.SlotType)
            {
                case ItemEquipmentSlotType.Weapon:
                    type = DisassembleType.TranscendenceWeapon.ToIntValue();
                    break;

                case ItemEquipmentSlotType.HeadGear:
                case ItemEquipmentSlotType.Garment:
                case ItemEquipmentSlotType.Armor:
                case ItemEquipmentSlotType.Accessory1:
                case ItemEquipmentSlotType.Accessory2:
                    type = DisassembleType.TranscendenceArmor.ToIntValue();
                    break;                
            }
            RewardData[] rewards = presenter.GetRewards(type, item.ItemTranscend);

            if (rewards != null)
                disassembleSelectPopupView.SetRewards(rewards);

            disassembleSelectPopupView.Show();
        }
    }
}