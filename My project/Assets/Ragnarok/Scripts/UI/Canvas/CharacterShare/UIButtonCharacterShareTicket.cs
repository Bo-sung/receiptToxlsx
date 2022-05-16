using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class UIButtonCharacterShareTicket : UIView, IInspectorFinder
    {
        [SerializeField] ShareTicketType ticketType;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabel labelShareTime;
        [SerializeField] UILabel labelCount;
        [SerializeField] UIButtonHelper btnUse, btnBuy;
        [SerializeField] UILabel labelCost;

        public event System.Action<ShareTicketType> OnSelectShareTicketUse;
        public event System.Action<ShareTicketType> OnSelectShareTicketBuy;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnUse.OnClick, OnClickedBtnUse);
            EventDelegate.Add(btnBuy.OnClick, OnClickedBtnBuy);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnUse.OnClick, OnClickedBtnUse);
            EventDelegate.Remove(btnBuy.OnClick, OnClickedBtnBuy);
        }

        void OnClickedBtnUse()
        {
            OnSelectShareTicketUse?.Invoke(ticketType);
        }

        void OnClickedBtnBuy()
        {
            OnSelectShareTicketBuy?.Invoke(ticketType);
        }

        protected override void OnLocalize()
        {
            var timeSpan = ticketType.ToTimeSpan();
            if (timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
            {
                // 정확한 시간일 경우
                labelShareTime.text = LocalizeKey._10240.ToText() // {HOURS}시간
                    .Replace(ReplaceKey.HOURS, timeSpan.TotalHours.ToString());
            }
            else
            {
                // 딱 나누어 떨어지지 않는 시간의 경우에는 Total 분으로 보여줌
                labelShareTime.text = LocalizeKey._10218.ToText() // {MINUTES}분
                    .Replace(ReplaceKey.MINUTES, timeSpan.TotalMinutes.ToString());
            }

            btnUse.LocalKey = LocalizeKey._10219; // 사용
            btnBuy.LocalKey = LocalizeKey._10220; // 구매
        }

        public ShareTicketType GetTicketType()
        {
            return ticketType;
        }

        public void SetIcon(string iconName)
        {
            icon.Set(iconName, false);
        }

        public void SetCost(int cost)
        {
            labelCost.text = cost.ToString();
        }

        public void SetCount(int count)
        {
            bool hasItem = count > 0;
            btnUse.SetActive(hasItem);
            btnBuy.SetActive(!hasItem);
            labelCount.text = count.ToString();
        }

        bool IInspectorFinder.Find()
        {
            ticketType = (transform.GetSiblingIndex() + 1).ToEnum<ShareTicketType>();
            return true;
        }
    }
}