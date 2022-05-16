using UnityEngine;

namespace Ragnarok.View.EquipmentView
{
    public class EquipmentInfoBattleOptionView : UIView
    {
        [SerializeField] UILabelValue atk, matk, def, mdef; // 장비 기본 옵션
        [SerializeField] BattleOptionListView equipOptionView; // 장비 추가 옵션
        [SerializeField] UILabelHelper labelCardOptionTitle; // 카드옵션
        [SerializeField] UITable table; // 장비 추가옵션, 카드 옵션 정렬
        [SerializeField] CardSlotListView cardSlotListView; // 카드 슬롯 정보
        [SerializeField] CardOptionView cardOptionView; // 카드 옵션 정보

        public event UIEquipmentInfo.SelectCardSlotEvent OnSelect;

        protected override void Awake()
        {
            base.Awake();
            cardSlotListView.OnSelect += OnSelectCardSlot;
            cardOptionView.OnSelect += OnSelectCardSlot;
            table.onReposition += OnReposition;
        }        

        protected override void OnDestroy()
        {
            base.OnDestroy();
            cardSlotListView.OnSelect -= OnSelectCardSlot;
            cardOptionView.OnSelect -= OnSelectCardSlot;
            table.onReposition -= OnReposition;
        }

        protected override void OnLocalize()
        {
            labelCardOptionTitle.LocalKey = LocalizeKey._16006; // 카드 옵션
        }

        public void SetData(ItemInfo info, bool isEditable, string iconName, string itemName)
        {
            ItemInfo.Status status = info.GetStatus();
            SetValue(atk, LocalizeKey._56000.ToText(), status.atk); // 물리 공격력
            SetValue(matk, LocalizeKey._56001.ToText(), status.matk); // 마법 공격력
            SetValue(def, LocalizeKey._56002.ToText(), status.def); // 물리 방어력
            SetValue(mdef, LocalizeKey._56003.ToText(), status.mdef); // 마법 방어력

            equipOptionView.SetData(info); // 장비 추가 옵션
            cardSlotListView.SetData(info, isEditable, iconName); // 카드 슬롯 정보
            cardOptionView.SetData(info, itemName); // 카드 옵션 리스트
        }        

        private void SetValue(UILabelValue labelValue, string title, int value)
        {
            bool isValid = value > 0;

            labelValue.Title = title;

            if (!isValid)
            {
                labelValue.Value = "-";
                return;
            }

            labelValue.Value = $"{value}";
        }

        private void OnSelectCardSlot(byte slotIndex, CardSlotEvent cardSlotEvent)
        {
            OnSelect?.Invoke(slotIndex, cardSlotEvent);
        }

        public void ShowSlotView(bool isActive)
        {
            cardSlotListView.SetActive(isActive);            
        }

        public void ResetPosition()
        {
            table.repositionNow = true;
        }

        void OnReposition()
        {            
            cardOptionView.ResetPosition();
        }

        public void ToggleSlotView()
        {
            bool isActive = !cardSlotListView.IsShow;
            ShowSlotView(isActive);
        }
    }
}