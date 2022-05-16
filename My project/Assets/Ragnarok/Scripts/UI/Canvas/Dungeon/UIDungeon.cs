using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIDungeon : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        public enum ViewType
        {
            None = 0,
            Zeny = 1,
            EXP = 2,
            WorldBoss = 3,
            Defence = 4,
            CentralLab = 5,
            EndlessTower = 6,
        }

        public static ViewType viewType = ViewType.None;

        [SerializeField] TitleView titleView;
        [SerializeField] DungeonListView dungeonList;
        [SerializeField] DungeonReadyView dungeonReady;

        DungeonPresenter presenter;

        bool isStartBattle;

        protected override void OnInit()
        {
            presenter = new DungeonPresenter();

            dungeonReady.Initialize(presenter);

            dungeonList.OnSelect += OnSelectListViewSlot;
            dungeonReady.OnSelectFastClearBattle += OnSelectFastClearBattle;
            dungeonReady.OnSelectStartBattle += OnSelectStartBattle;
            presenter.OnUpdateGoodsZeny += OnUpdateGoodsZeny;
            presenter.OnUpdateGoodsCatCoin += OnUpdateGoodsCatCoin;
            presenter.OnUpdateView += UpdateView;

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            dungeonList.OnSelect -= OnSelectListViewSlot;
            dungeonReady.OnSelectFastClearBattle -= OnSelectFastClearBattle;
            dungeonReady.OnSelectStartBattle -= OnSelectStartBattle;
            presenter.OnUpdateGoodsZeny -= OnUpdateGoodsZeny;
            presenter.OnUpdateGoodsCatCoin -= OnUpdateGoodsCatCoin;
            presenter.OnUpdateView -= UpdateView;

            presenter.RemoveEvent();

            if (presenter != null)
                presenter = null;
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
            presenter.RemoveNewOpenContent_Dungeon(); // 신규 컨텐츠 플래그 제거

            OnUpdateGoodsZeny();
            OnUpdateGoodsCatCoin();

            UpdateView();
            dungeonList.Show();
            dungeonReady.Hide();

            if (viewType == ViewType.Zeny)
                OnSelectListViewSlot(DungeonType.ZenyDungeon);
            else if (viewType == ViewType.EXP)
                OnSelectListViewSlot(DungeonType.ExpDungeon);
            else if (viewType == ViewType.WorldBoss)
                OnSelectListViewSlot(DungeonType.WorldBoss);
            else if (viewType == ViewType.Defence)
                OnSelectListViewSlot(DungeonType.Defence);
            else if (viewType == ViewType.CentralLab)
                OnSelectListViewSlot(DungeonType.CentralLab);
            else if (viewType == ViewType.EndlessTower)
                OnSelectListViewSlot(DungeonType.EnlessTower);

            viewType = ViewType.None;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._7012.ToText()); // 던전
        }

        protected override void OnBack()
        {
            if (dungeonReady.IsShow)
            {
                UpdateView();
                dungeonReady.Hide();
                dungeonList.Show();
                OnLocalize(); // 타이틀명 초기화
                return;
            }

            UI.Close<UIDungeon>();
        }

        void OnUpdateGoodsZeny()
        {
            titleView.ShowZeny(presenter.GetZeny());
        }

        void OnUpdateGoodsCatCoin()
        {
            titleView.ShowCatCoin(presenter.GetCatCoin());
        }

        void UpdateView()
        {
            dungeonList.SetData(presenter.GetDungeonElements());

            if (dungeonReady.IsShow)
                dungeonReady.Refresh();
        }

        void OnSelectListViewSlot(DungeonType dungeonType)
        {
            // 던전 UI 나가기 애니메이션 중에는 던전 리스트 클릭 막기
            if (IsPlayingAniOut())
                return;

            AniInFinish();

            if (dungeonType == DungeonType.WorldBoss)
            {
                UI.Show<UIWorldBoss>();
                return;
            }

            if (dungeonType == DungeonType.CentralLab)
            {
                // 중앙실험실 가능 여부
                if (!presenter.IsCheckCentralLab())
                    return;

                UI.Show<UICentralLab>();
                return;
            }

            if (dungeonType == DungeonType.EnlessTower)
            {
                UI.Show<UIEndlessTower>();
                return;
            }

            switch (dungeonType) // 타이틀명 변경
            {
                case DungeonType.ZenyDungeon:
                    titleView.ShowTitle(LocalizeKey._10611.ToText()); // 제니 던전
                    break;

                case DungeonType.ExpDungeon:
                    titleView.ShowTitle(LocalizeKey._10612.ToText()); // 경험치 던전
                    break;

                case DungeonType.Defence:
                    titleView.ShowTitle(LocalizeKey._28028.ToText()); // 비공정 습격
                    break;
            }

            dungeonReady.SetData(presenter.GetDungeonDetailElements(dungeonType));
            dungeonList.Hide();
            dungeonReady.Show();
        }

        void OnSelectFastClearBattle(DungeonType dungeonType, int id)
        {
            presenter.RequestFastClearDungeon(dungeonType, id, isFree: false);
        }

        void OnSelectStartBattle(DungeonType dungeonType, int id)
        {
            bool isCleared = presenter.IsCleared(dungeonType, id);
            bool canEnterDungeon = presenter.CanEnterDungeon(dungeonType);

            // 무료 소탕
            if(isCleared && canEnterDungeon)
            {
                presenter.RequestFastClearDungeon(dungeonType, id, isFree: true);
                return;
            }

            isStartBattle = true;
            presenter.RequestStartDungeon(dungeonType, id);
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
            {
                UI.Close<UIDungeon>();
            }
        }
    }
}