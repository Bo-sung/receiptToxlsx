using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEventRank : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] RankView rankView;
        [SerializeField] UIRankElement myRankElement;

        RankPresenter presenter;

        private RankType rankType;

        protected override void OnInit()
        {
            presenter = new RankPresenter();

            EventDelegate.Add(btnClose.OnClick, OnBack);

            rankView.OnDragFinish += presenter.RequestNextPage;
            rankView.OnSelect += presenter.RequestOtherCharacterInfo;
            presenter.OnUpdateRankList += UpdateRankList;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            rankView.OnDragFinish -= presenter.RequestNextPage;
            rankView.OnSelect -= presenter.RequestOtherCharacterInfo;
            presenter.OnUpdateRankList -= UpdateRankList;

            EventDelegate.Remove(btnClose.OnClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = GetTitleLocalKey();
        }

        public void SetRankType(RankType type)
        {
            rankType = type;

            OnLocalize();
            presenter.RequestRankList(rankType);
        }

        private void UpdateRankList((RankType rankType, int page) info)
        {
            // 랭킹 리스트
            rankView.SetRankType(info.rankType);
            UIRankElement.IInput[] arrayInfo = presenter.GetArrayInfo(info.rankType);
            rankView.SetData(arrayInfo);

            // 내 랭킹
            myRankElement.SetData(presenter.GetMyInfo(info.rankType));
        }

        private int GetTitleLocalKey()
        {
            switch (rankType)
            {
                case RankType.RockPaperScissors:
                    return LocalizeKey._32032; // 이벤트 랭킹
            }

            return LocalizeKey._32000; // 랭킹
        }
    }
}