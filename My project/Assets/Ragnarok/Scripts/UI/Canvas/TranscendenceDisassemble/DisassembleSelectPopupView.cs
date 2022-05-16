using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UITranscendenceDisassemble"/>
    /// </summary>
    public class DisassembleSelectPopupView : UIView
    {
        [SerializeField] SelectPopupView selectPopupView;
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UIGrid grid;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper labelNotice;

        private ItemInfo itemInfo;

        public event System.Action<long> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            selectPopupView.OnExit += Hide;
            selectPopupView.OnCancel += Hide;
            selectPopupView.OnConfirm += InvokeOnSelect;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            selectPopupView.OnExit -= Hide;
            selectPopupView.OnCancel -= Hide;
            selectPopupView.OnConfirm -= InvokeOnSelect;
        }

        protected override void OnLocalize()
        {
            selectPopupView.MainTitleLocalKey = LocalizeKey._8402; // 초월 분해
            selectPopupView.CancelLocalKey = LocalizeKey._8407; // 취소
            selectPopupView.ConfirmLocalKey = LocalizeKey._8408; // 분해
            labelDesc.LocalKey = LocalizeKey._8405; // 초월된 장비를 분해하시겠습니까?
            labelNotice.LocalKey = LocalizeKey._8406; // 분해한 초월 장비는 다시 복구할 수 없습니다.\n카드가 장착되어 있는 장비는 분해 시 카드가 자동으로 회수됩니다.
        }

        public void SetData(ItemInfo itemInfo)
        {
            this.itemInfo = itemInfo;
            equipmentProfile.SetData(itemInfo);
            labelItemName.Text = LocalizeKey._8404.ToText() // [DC83FF][{VALUE}초월][-] [4C4A4D]{NAME}[-]
                .Replace(ReplaceKey.VALUE, itemInfo.ItemTranscend)
                .Replace(ReplaceKey.NAME, itemInfo.Name);
        }

        public void SetRewards(RewardData[] inputs)
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(inputs[i]);
            }
            grid.Reposition();
        }

        private void InvokeOnSelect()
        {
            OnSelect?.Invoke(itemInfo.ItemNo);
        }
    }
}