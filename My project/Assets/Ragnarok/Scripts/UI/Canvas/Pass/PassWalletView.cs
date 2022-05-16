using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIOnBuffPass"/>
    /// </summary>
    public class PassWalletView : UIView
    {
        [SerializeField] UIButtonHelper btnOnBuff;
        [SerializeField] UILabelValue onBuffPoint;

        [SerializeField] UILabelHelper labelPointTitle;
        [SerializeField] UILabelHelper labelMaxPoint;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabelHelper labelProgress;
        [SerializeField] UIButtonHelper btnPoint;
        [SerializeField] UILabelHelper labelDescription, labelLockDescription;
        [SerializeField] UIRewardHelper reward1, reward2;
        [SerializeField] GameObject goLock;
        [SerializeField] UIButtonHelper btnRefresh;

        public event System.Action OnSelectGetPoint;
        public event System.Action OnSelectRefreshPoint;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnOnBuff.OnClick, OnClickedBtnOnBuff);
            EventDelegate.Add(btnPoint.OnClick, OnClickedBtnPoint);
            EventDelegate.Add(btnRefresh.OnClick, OnClickedBtnRefresh);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnOnBuff.OnClick, OnClickedBtnOnBuff);
            EventDelegate.Remove(btnPoint.OnClick, OnClickedBtnPoint);
            EventDelegate.Remove(btnRefresh.OnClick, OnClickedBtnRefresh);
        }

        protected override void OnLocalize()
        {
            onBuffPoint.TitleKey = LocalizeKey._39823; // 보유 OnBuff 포인트
            labelPointTitle.LocalKey = LocalizeKey._39824; // 일일 MVP 사냥 포인트 한도
            btnPoint.LocalKey = LocalizeKey._39826; // 포인트 받기
            labelMaxPoint.LocalKey = LocalizeKey._39825; // 일일 한도 포인트
            labelDescription.Text = LocalizeKey._39827.ToText() // OnBuff 패스 적용중! 추가 포인트 {COUNT}지급!!
                .Replace(ReplaceKey.COUNT, BasisOnBuffInfo.PremiumOnBuffRewardPoints.GetInt());
            labelLockDescription.LocalKey = LocalizeKey._39833; // OnBuff 패스를 구매하면 추가 포인트를 획득할 수 있습니다.
        }

        void OnClickedBtnOnBuff()
        {
            BasisUrl.OnBuffHompage.OpenUrl();
        }

        void OnClickedBtnPoint()
        {
            OnSelectGetPoint?.Invoke();
        }

        void OnClickedBtnRefresh()
        {
            OnSelectRefreshPoint?.Invoke();
        }

        public void SetActivePass(bool isActivePass)
        {
            goLock.SetActive(!isActivePass);

            labelDescription.SetActive(isActivePass);
            labelLockDescription.SetActive(!isActivePass);
        }

        public void SetTotalPoint(int totalPoint)
        {
            onBuffPoint.Value = totalPoint.ToString("N0");
        }

        public void SetProgress(int cur, int max, bool canRequest)
        {
            progressBar.value = MathUtils.GetProgress(cur, max);
            labelProgress.Text = StringBuilderPool.Get()
                .Append(cur).Append('/').Append(max)
                .Release();

            btnPoint.IsEnabled = canRequest && (cur >= max);
        }

        public void SetRewards(RewardData freeReward, RewardData premiumReward)
        {
            reward1.SetData(freeReward);
            reward2.SetData(premiumReward);
        }

        public void SetActiveBtnRefresh(bool isActive)
        {
            btnRefresh.SetActive(isActive);
        }
    }
}