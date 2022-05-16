using UnityEngine;

namespace Ragnarok
{
    public sealed class UIFeedView : UICanvas<FeedViewPresenter>, FeedViewPresenter.IView
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelFeedTitle; // 먹이
        [SerializeField] UILabelHelper labelFeedCount; // 보유량 : {COUNT}

        protected override void OnInit()
        {
            presenter = new FeedViewPresenter(this);
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void SetItem(int itemId)
        {
            presenter.SetItem(itemId);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelFeedTitle.LocalKey = LocalizeKey._33300; // 먹이
        }

        void FeedViewPresenter.IView.SetRewardData(RewardData rewardData)
        {
            rewardHelper.SetData(rewardData);
            rewardHelper.IsEnabled = rewardData.Count > 0;
        }

        void FeedViewPresenter.IView.SetItemCount(int itemCount)
        {
            labelFeedCount.Text = LocalizeKey._33116.ToText().Replace(ReplaceKey.COUNT, itemCount); // 보유량 : {COUNT}
        }
    }
}