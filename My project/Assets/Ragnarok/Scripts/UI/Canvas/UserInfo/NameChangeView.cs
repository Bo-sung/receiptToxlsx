using UnityEngine;

namespace Ragnarok
{
    public sealed class NameChangeView : UISubCanvas<UserInfoPresenter>, IAutoInspectorFinder
    {
        [SerializeField] UIButton btnClose;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIInput inputName;
        [SerializeField] UILabelHelper labelDescription, labelCostInfo;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UICostButtonHelper btnChangeName;

        int freeNameChangeCount;
        int needCoin;
        string oldName;

        protected override void OnInit()
        {
            EventDelegate.Add(btnClose.onClick, presenter.ShowUserInfoView);
            EventDelegate.Add(btnExit.OnClick, presenter.ShowUserInfoView);
            EventDelegate.Add(inputName.onChange, RefreshButtonChangeName);
            EventDelegate.Add(btnCancel.OnClick, presenter.ShowUserInfoView);
            EventDelegate.Add(btnChangeName.OnClick, OnClickedBtnChangeName);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClose.onClick, presenter.ShowUserInfoView);
            EventDelegate.Remove(btnExit.OnClick, presenter.ShowUserInfoView);
            EventDelegate.Remove(inputName.onChange, RefreshButtonChangeName);
            EventDelegate.Remove(btnCancel.OnClick, presenter.ShowUserInfoView);
            EventDelegate.Remove(btnChangeName.OnClick, OnClickedBtnChangeName);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnHide() { }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._14019; // 캐릭터 이름 변경
            inputName.defaultText = LocalizeKey._14020.ToText(); // 변경할 닉네임을 입력하세요
            labelDescription.LocalKey = LocalizeKey._14021; // 캐릭터 이름은 최소 2글자, 최대 8글자까지 만들 수 있습니다.
            btnCancel.LocalKey = LocalizeKey._14023; // 취소
            btnChangeName.TitleLocalKey = LocalizeKey._14040; // 필요 재화
            btnChangeName.LocalKey = LocalizeKey._14024; // 이름변경
            labelCostInfo.Text = LocalizeKey._14022.ToText() // (최초 {COUNT}회는 캐릭터 이름을 무료로 변경할 수 있습니다.)
                .Replace("{COUNT}", freeNameChangeCount.ToString());
            btnChangeName.SetCostCount(needCoin);
        }

        public void Refresh()
        {
            freeNameChangeCount = presenter.GetFreeNameChangeCount();
            needCoin = presenter.GetNameChangeCatCoin();
            oldName = presenter.GetCharacterName();
            OnLocalize();
            RefreshButtonChangeName();
        }

        void RefreshButtonChangeName()
        {
            btnChangeName.IsEnabled = IsChangeAbleName();
        }

        void OnClickedBtnChangeName()
        {
            if (presenter == null)
                return;

            string errorMessage = FilterUtils.CheckCharacterName(inputName.value);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                UI.ConfirmPopup(errorMessage);
                return;
            }

            presenter.RequestChangeName(inputName.value);
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
    }
}