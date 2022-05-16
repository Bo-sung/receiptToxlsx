using UnityEngine;

namespace Ragnarok.View.League
{
    public class LeagueEntryPopupView : UIView, IAutoInspectorFinder
    {
        public interface IInput
        {
            int GetTicketSize();
        }

        [SerializeField] UIEventTrigger unselect;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UIPressButton btnMinus, btnPlus;
        [SerializeField] UIInput inputCount;
        [SerializeField] UILabelValue ticketCount, freeCount;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnCancel, btnConfirm;

        private int size;
        private int maxSize;

        public System.Action<int> OnConfirm;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(unselect.onClick, Hide);
            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Add(inputCount.onChange, RefreshInputCount);
            EventDelegate.Add(inputCount.onSubmit, RefreshInputCount);
            EventDelegate.Add(btnCancel.OnClick, Hide);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(unselect.onClick, Hide);
            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Remove(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Remove(inputCount.onChange, RefreshInputCount);
            EventDelegate.Remove(inputCount.onSubmit, RefreshInputCount);
            EventDelegate.Remove(btnCancel.OnClick, Hide);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.Text = LocalizeKey._47017.ToText(); // 대전 횟수 설정
            ticketCount.Title = LocalizeKey._47011.ToText(); // 입장권
            freeCount.Title = LocalizeKey._47014.ToText(); // 무료 입장
            labelNotice.Text = LocalizeKey._47018.ToText(); // 무료 잔여 횟수가 남아있다면, 무료 횟수가 먼저 차감됩니다.
            btnCancel.Text = LocalizeKey._47019.ToText(); // 취소
            btnConfirm.Text = LocalizeKey._47020.ToText(); // 시작
        }

        public void Show(int leagueTicketCount, int leagueFreeCount, int leagueFreeMaxCount)
        {
            size = 1;
            maxSize = leagueTicketCount + leagueFreeCount;

            ticketCount.Value = leagueTicketCount.ToString();
            freeCount.Value = StringBuilderPool.Get()
                .Append(leagueFreeCount.ToString()).Append("/").Append(leagueFreeMaxCount.ToString())
                .Release();

            Show();
        }

        public override void Show()
        {
            base.Show();

            Refresh();
        }

        void OnClickedBtnConfirm()
        {
            OnConfirm?.Invoke(size);
        }

        void OnClickedBtnMinus()
        {
            --size;
            Refresh();
        }

        void OnClickedBtnPlus()
        {
            ++size;
            Refresh();
        }

        void RefreshInputCount()
        {
            int inputValue = int.Parse(inputCount.value);
            size = Mathf.Clamp(inputValue, 1, maxSize);
            Refresh();
        }

        private void Refresh()
        {
            inputCount.value = size.ToString();
            btnMinus.isEnabled = size > 1;
            btnPlus.isEnabled = size < maxSize;
        }
    }
}