using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class CharacterShareChargePopupView : UIView, IInspectorFinder
    {
        /// <summary>
        /// <see cref="ShopInfo"/>
        /// </summary>
        public interface IInitializer
        {
            int Cost { get; }
            string IconName { get; }
        }

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UIButtonCharacterShareTicket[] tickets;
        [SerializeField] UILabelHelper labelFreeCharge;
        [SerializeField] UIButtonHelper btnFreeCharge;
        [SerializeField] UILabelHelper labelNotice01, labelNotice02;
        [SerializeField] UIButtonHelper btnConfirm;

        public event System.Action OnSelectFreeCharge;
        public event System.Action<ShareTicketType> OnSelectUse;
        public event System.Action<ShareTicketType> OnSelectBuy;

        private int freeDailyTotalHours; // 무료 충전 시간
        private int maxShareTotalHours; // 최대 충전 시간

        protected override void Awake()
        {
            base.Awake();

            foreach (var item in tickets)
            {
                item.OnSelectShareTicketUse += OnSelectShareTicketUse;
                item.OnSelectShareTicketBuy += OnSelectShareTicketBuy;
            }

            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnFreeCharge.OnClick, OnClickedBtnFreeCharge);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in tickets)
            {
                item.OnSelectShareTicketUse -= OnSelectShareTicketUse;
                item.OnSelectShareTicketBuy -= OnSelectShareTicketBuy;
            }

            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnFreeCharge.OnClick, OnClickedBtnFreeCharge);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._10217; // 이용시간 충전
            labelFreeCharge.Text = LocalizeKey._10221.ToText() // 무료 {HOURS}시간 충전
                .Replace(ReplaceKey.HOURS, freeDailyTotalHours);
            btnFreeCharge.LocalKey = LocalizeKey._10222; // 사용
            labelNotice01.LocalKey = LocalizeKey._10223; // 무료충전은 매일 자정에 초기화 됩니다.
            labelNotice02.Text = LocalizeKey._10224.ToText() // 이용시간은 [c][84a2ec]최대 {HOURS}시간[-][/c]까지 누적 가능합니다
                .Replace(ReplaceKey.HOURS, maxShareTotalHours);
            btnConfirm.LocalKey = LocalizeKey._10225; // 확인
        }

        void OnSelectShareTicketUse(ShareTicketType ticketType)
        {
            OnSelectUse?.Invoke(ticketType);
        }

        void OnSelectShareTicketBuy(ShareTicketType ticketType)
        {
            OnSelectBuy?.Invoke(ticketType);
        }

        void OnClickedBtnFreeCharge()
        {
            OnSelectFreeCharge?.Invoke();
        }

        void OnClickedBtnConfirm()
        {
            Hide();
        }

        public void SetFreeChargeTicket(bool hasFreeTicket)
        {
            btnFreeCharge.IsEnabled = hasFreeTicket;
            btnFreeCharge.SetNotice(hasFreeTicket);
        }

        public void Initialize(int freeDailyTotalHours, int maxShareTotalHours)
        {
            this.freeDailyTotalHours = freeDailyTotalHours;
            this.maxShareTotalHours = maxShareTotalHours;
        }

        public void Initialize(ShareTicketType ticketType, IInitializer initializer)
        {
            UIButtonCharacterShareTicket button = GetButton(ticketType);
            if (button == null)
                return;

            button.SetIcon(initializer.IconName);
            button.SetCost(initializer.Cost);
        }

        public void SetTicketCount(ShareTicketType ticketType, int count)
        {
            UIButtonCharacterShareTicket button = GetButton(ticketType);
            if (button == null)
                return;

            button.SetCount(count);
        }

        private UIButtonCharacterShareTicket GetButton(ShareTicketType ticketType)
        {
            foreach (var item in tickets)
            {
                if (item.GetTicketType() == ticketType)
                    return item;
            }

            return null;
        }

        bool IInspectorFinder.Find()
        {
            tickets = GetComponentsInChildren<UIButtonCharacterShareTicket>();
            return true;
        }
    }
}