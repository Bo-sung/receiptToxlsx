using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRank : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UILabelHelper btnTitle;
        [SerializeField] UITabHelper tab;
        [SerializeField] RankView rankView;
        [SerializeField] UIRankElement myRankElement;

        RankPresenter presenter;

        protected override void OnInit()
        {
            presenter = new RankPresenter();

            rankView.OnDragFinish += presenter.RequestNextPage;
            rankView.OnSelect += presenter.RequestOtherCharacterInfo;
            presenter.OnUpdateRankList += UpdateRankList;

            presenter.AddEvent();

            EventDelegate.Add(btnClose.OnClick, OnBack);
            EventDelegate.Add(tab[0].OnChange, ShowRankTab0);
            EventDelegate.Add(tab[1].OnChange, ShowRankTab1);
            EventDelegate.Add(tab[2].OnChange, ShowRankTab2);
            EventDelegate.Add(tab[3].OnChange, ShowRankTab3);
            EventDelegate.Add(tab[4].OnChange, ShowRankTab4);
            EventDelegate.Add(tab[5].OnChange, ShowRankTab5);
            EventDelegate.Add(tab[6].OnChange, ShowRankTab6);
            EventDelegate.Add(tab[7].OnChange, ShowRankTab7);
        }

        protected override void OnClose()
        {
            rankView.OnDragFinish -= presenter.RequestNextPage;
            rankView.OnSelect -= presenter.RequestOtherCharacterInfo;
            presenter.OnUpdateRankList -= UpdateRankList;

            presenter.RemoveEvent();

            EventDelegate.Remove(btnClose.OnClick, OnBack);
            EventDelegate.Remove(tab[0].OnChange, ShowRankTab0);
            EventDelegate.Remove(tab[1].OnChange, ShowRankTab1);
            EventDelegate.Remove(tab[2].OnChange, ShowRankTab2);
            EventDelegate.Remove(tab[3].OnChange, ShowRankTab3);
            EventDelegate.Remove(tab[4].OnChange, ShowRankTab4);
            EventDelegate.Remove(tab[5].OnChange, ShowRankTab5);
            EventDelegate.Remove(tab[6].OnChange, ShowRankTab6);
            EventDelegate.Remove(tab[7].OnChange, ShowRankTab7);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnTitle.LocalKey = LocalizeKey._32000; // 랭킹
            tab[0].LocalKey = LocalizeKey._32002; // 직업 레벨
            tab[1].LocalKey = LocalizeKey._32015; // 사냥 필드
            tab[2].LocalKey = LocalizeKey._32019; // 전투력
            tab[3].LocalKey = LocalizeKey._32022; // MVP 처치
            tab[4].LocalKey = LocalizeKey._32023; // 카드 강화
            tab[5].LocalKey = LocalizeKey._32024; // 제작
            tab[6].LocalKey = LocalizeKey._32025; // 거래소 구매
            tab[7].LocalKey = LocalizeKey._32026; // 거래소 판매
        }

        public void SetTab(int tabIndex)
        {
            tab[tabIndex].Set(true);
        }

        public void UpdateRankList((RankType rankType, int page) info)
        {
            // 랭킹 리스트
            rankView.SetRankType(info.rankType);
            UIRankElement.IInput[] arrayInfo = presenter.GetArrayInfo(info.rankType);
            rankView.SetData(arrayInfo);

            // 내 랭킹
            myRankElement.SetData(presenter.GetMyInfo(info.rankType));
        }

        private void ShowRankTab0()
        {
            if (!UIToggle.current.value)
                return;

            rankView.SetProgress(0);
            presenter.RequestRankList(RankType.All);
        }

        private void ShowRankTab1()
        {
            if (!UIToggle.current.value)
                return;

            rankView.SetProgress(0);
            presenter.RequestRankList(RankType.StageClear);
        }

        private void ShowRankTab2()
        {
            if (!UIToggle.current.value)
                return;

            rankView.SetProgress(0);
            presenter.RequestRankList(RankType.BattleScore);
        }

        private void ShowRankTab3()
        {
            if (!UIToggle.current.value)
                return;

            rankView.SetProgress(0);
            presenter.RequestRankList(RankType.KillMvp);
        }

        private void ShowRankTab4()
        {
            if (!UIToggle.current.value)
                return;

            rankView.SetProgress(0);
            presenter.RequestRankList(RankType.CardUp);
        }

        private void ShowRankTab5()
        {
            if (!UIToggle.current.value)
                return;

            rankView.SetProgress(0);
            presenter.RequestRankList(RankType.ItemMake);
        }

        private void ShowRankTab6()
        {
            if (!UIToggle.current.value)
                return;

            rankView.SetProgress(0);
            presenter.RequestRankList(RankType.ItemBuy);
        }

        private void ShowRankTab7()
        {
            if (!UIToggle.current.value)
                return;

            rankView.SetProgress(0);
            presenter.RequestRankList(RankType.ItemSell);
        }
    }
}
