using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIWorldBoss : UICanvas<WorldBossPresenter>
        , WorldBossPresenter.IView
        , WorldBossListView.IListener
        , WorldBossInfoView.IListener
        , WorldBossRewardView.IListener
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] WorldBossListView WorldBossList;
        [SerializeField] WorldBossInfoView worldBossInfo;
        [SerializeField] WorldBossRewardView worldBossReward;
        [SerializeField] UIButtonHelper btnHelp;

        bool isStartBattle;

        protected override void OnInit()
        {
            presenter = new WorldBossPresenter(this);

            WorldBossList.Initialize(this);
            worldBossInfo.Initialize(this);
            worldBossReward.Initialize(this);

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.AddEvent();

            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            if (presenter != null)
                presenter = null;

            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            BattleManager.OnStart += OnStartBattle;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            isStartBattle = false;
            BattleManager.OnStart -= OnStartBattle;
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
            presenter.StartUpdateWorldBossInfos();
        }

        protected override void OnHide()
        {
            presenter.KillCoroutines();
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._7037.ToText()); // 무한의 공간
        }

        protected override void OnBack()
        {
            UIDungeon.viewType = UIDungeon.ViewType.None;
            UI.Show<UIDungeon>();
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
            {
                UI.Close<UIWorldBoss>();
            }
        }

        public void Refresh()
        {
            WorldBossList.SetData(presenter.GetWorldBossDungeonElements());
            worldBossInfo.SetData(presenter.GetSelectElement());
            worldBossReward.SetData(presenter.GetSelectElement());
        }

        private void OnClickedBtnHelp()
        {
            int info_id = DungeonInfoType.WorldBoss.GetDungeonInfoId();
            UI.Show<UIDungeonInfoPopup>().Show(info_id);
        }

        void WorldBossPresenter.IView.UpdateZeny(long zeny)
        {
            titleView.ShowZeny(zeny);
        }

        void WorldBossPresenter.IView.UpdateCatCoin(long catCoin)
        {
            titleView.ShowCatCoin(catCoin);
        }

        void WorldBossListView.IListener.OnSelect(WorldBossDungeonElement element)
        {
            presenter.SetSelectWorldBoss(element);
            Refresh();
        }

        void WorldBossListView.IListener.OnAlarm(WorldBossDungeonElement element)
        {
            presenter.SetAlarmWorldBoss(element);
        }

        void WorldBossListView.IListener.OnStartWorldBoss()
        {
            presenter.StartWorldBoss();
        }

        void WorldBossPresenter.IView.CloseUI()
        {
            UI.Close<UIWorldBoss>();
        }

        void WorldBossPresenter.IView.TryStartBattle()
        {
            isStartBattle = true;
        }
    }
}
