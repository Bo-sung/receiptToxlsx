using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEditGuildName : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButton btnClose;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIInput inputName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UICostButtonHelper btnChangeName;

        string oldName;
        int needCoin;
        Action<string> onChangeName;

        protected override void OnInit()
        {
            EventDelegate.Add(btnClose.onClick, OnBack);
            EventDelegate.Add(btnExit.OnClick, OnBack);
            EventDelegate.Add(btnCancel.OnClick, OnBack);
            EventDelegate.Add(inputName.onChange, RefreshButtonChangeName);
            EventDelegate.Add(btnChangeName.OnClick, OnClickedBtnChangeName);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClose.onClick, OnBack);
            EventDelegate.Remove(btnExit.OnClick, OnBack);
            EventDelegate.Remove(btnCancel.OnClick, OnBack);
            EventDelegate.Remove(inputName.onChange, RefreshButtonChangeName);
            EventDelegate.Remove(btnChangeName.OnClick, OnClickedBtnChangeName);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._6700; // 길드명 변경
            inputName.defaultText = LocalizeKey._6701.ToText(); // 변경할 길드명을 입력하세요.
            labelDescription.LocalKey = LocalizeKey._6702; // 길드명은 최소 2글자에서 최대 8글자로 변경할 수 있습니다.
            btnCancel.LocalKey = LocalizeKey._6704; // 취소
            btnChangeName.LocalKey = LocalizeKey._6703; // 변경
        }

        public void Set(string oldName, int needCoin, Action<string> onChangeName)
        {
            this.oldName = oldName;
            this.needCoin = needCoin;
            this.onChangeName = onChangeName;

            btnChangeName.SetCostCount(needCoin);
            RefreshButtonChangeName();
        }

        void RefreshButtonChangeName()
        {
            btnChangeName.IsEnabled = IsChangeAbleName();
        }

        private bool IsChangeAbleName()
        {
            // 코인 부족
            long coin = CoinType.CatCoin.GetCoin();
            if (coin < needCoin)
                return false;

            string newName = inputName.value;

            // 기존과 같은 이름
            if (newName.Equals(oldName))
                return false;

            // 2글자 미만 또는 8글자 초과
            int nameLength = newName.Length;
            if (nameLength < 2 || nameLength > 8)
                return false;

            return true;
        }

        void OnClickedBtnChangeName()
        {          
            string errorMessage = FilterUtils.CheckCharacterName(inputName.value);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                UI.ConfirmPopup(errorMessage);
                return;
            }

            OnBack();
            onChangeName?.Invoke(inputName.value);
            onChangeName = null;
        }
    }
}