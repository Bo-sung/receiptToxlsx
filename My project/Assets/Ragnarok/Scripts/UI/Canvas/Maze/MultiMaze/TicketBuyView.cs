using UnityEngine;

namespace Ragnarok.View
{
    public class TicketBuyView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIRewardHelper rewardTicket;
        [SerializeField] UILabelHelper labelMessage;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonWithIconValue btnEnter;
        [SerializeField] UILabelHelper labelNotice;

        public event System.Action OnSelectEnter;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.OnClick, Hide);
            EventDelegate.Add(btnCancel.OnClick, Hide);
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.OnClick, Hide);
            EventDelegate.Remove(btnCancel.OnClick, Hide);
            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._2206; // 구매
            labelMessage.LocalKey = LocalizeKey._2207; // 입장권을 추가로 구매하여 입장하시겠습니까?
            labelDescription.LocalKey = LocalizeKey._2208; // (입장권은 구매할 때마다 가격이 상승하지만,\n자정에 구입 가격이 초기화됩니다.
            btnCancel.LocalKey = LocalizeKey._2209; // 취소
            btnEnter.LocalKey = LocalizeKey._2210; // 입장
            labelNotice.LocalKey = LocalizeKey._2211; // 입장권 보유 수는 자정에 초기화되며, 클리어 시에만 소모됩니다.
        }

        public void Set(RewardType rewardType, int catCoin)
        {
            rewardTicket.SetData(new RewardData(rewardType, 1, 0));
            btnEnter.SetIconName(CoinType.CatCoin.IconName());
            btnEnter.SetLabelValue(catCoin.ToString("N0"));
        }

        void OnClickedBtnEnter()
        {
            OnSelectEnter?.Invoke();
        }
    }
}