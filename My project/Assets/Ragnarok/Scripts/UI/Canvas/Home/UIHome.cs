using Ragnarok.View;
using Ragnarok.View.Home;
using UnityEngine;

namespace Ragnarok
{
    public class UIHome : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy | UIType.Single | UIType.Reactivation;

        public enum MenuContent
        {
            /// <summary>
            /// 스킬
            /// </summary>
            Skill = 1,
            /// <summary>
            /// 도감
            /// </summary>
            Book,
            /// <summary>
            /// 던전
            /// </summary>
            Dungeon,
            /// <summary>
            /// 난전
            /// </summary>
            FreeFight,
            /// <summary>
            /// 길드
            /// </summary>
            Guild,
            /// <summary>
            /// 대전
            /// </summary>
            Pvp,
            /// <summary>
            /// 길드전
            /// </summary>
            GuildBattle,
            /// <summary>
            /// 랭킹
            /// </summary>
            Rank,
            /// <summary>
            /// 커뮤니티
            /// </summary>
            Community,
            /// <summary>
            /// 네이버라운지
            /// </summary>
            NaverLounge,
            /// <summary>
            /// 설정
            /// </summary>
            Settings,
        }

        [SerializeField] TitleView titleView;
        [SerializeField] UIGrid grid;
        [SerializeField] UIHomeButton[] homeButtons;
        [SerializeField] UIEventBannerView eventBannerView;

        public UIWidget SkillButtonWidget => GetWidget(MenuContent.Skill);

        bool isStartBattle;

        HomePresenter presenter;

        protected override void OnInit()
        {
            presenter = new HomePresenter();

            foreach (var item in homeButtons)
            {
                item.OnSelect += OnSelect;
            }

            presenter.OnUpdateGoodsZeny += UpdateGoodsZeny;
            presenter.OnUpdateGoodsCatCoin += UpdateGoodsCatCoin;
            presenter.OnUpdateJobLevel += UpdateOpenContent;
            presenter.OnUpdateJobLevel += UpdateNotice;
            presenter.OnUpdateSkillPoint += UpdateNotice;
            presenter.OnUpdateDungeonFree += UpdateNotice;
            presenter.OnUpdateGuideQuest += UpdateOpenContent;
            presenter.OnUpdateNewOpenContent += UpdateNewIcon;
            presenter.OnBookStateChanged += OnBookStateChanged;
            presenter.OnTamingMazeOpen += UpdateGuildNotice;
            presenter.OnPurchaseSuccess += UpdateGuildNotice;
            presenter.OnResetFreeItemBuyCount += UpdateGuildNotice;
            presenter.OnUpdateEndlessTowerFreeTicket += UpdateNotice;

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            foreach (var item in homeButtons)
            {
                item.OnSelect -= OnSelect;
            }

            presenter.OnUpdateGoodsZeny -= UpdateGoodsZeny;
            presenter.OnUpdateGoodsCatCoin -= UpdateGoodsCatCoin;
            presenter.OnUpdateJobLevel -= UpdateOpenContent;
            presenter.OnUpdateJobLevel -= UpdateNotice;
            presenter.OnUpdateDungeonFree -= UpdateNotice;
            presenter.OnUpdateGuideQuest -= UpdateOpenContent;
            presenter.OnUpdateNewOpenContent -= UpdateNewIcon;
            presenter.OnBookStateChanged -= OnBookStateChanged;
            presenter.OnTamingMazeOpen -= UpdateGuildNotice;
            presenter.OnPurchaseSuccess -= UpdateGuildNotice;
            presenter.OnResetFreeItemBuyCount -= UpdateGuildNotice;
            presenter.OnUpdateEndlessTowerFreeTicket -= UpdateNotice;

            presenter.RemoveEvent();
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
            eventBannerView.SetData(presenter.GetEventBannerArrayData());

            UpdateGoodsZeny();
            UpdateGoodsCatCoin();
            UpdateNotice();
            UpdateOpenContent();
            UpdateNewIcon();

            UIHomeButton btnCommunity = FindButton(MenuContent.Community);
            UIHomeButton btnNaverLounge = FindButton(MenuContent.NaverLounge);

            if (Issue.NAVER_LOUNGE)
            {
                btnCommunity.SetActive(!GameServerConfig.IsKorea());
                btnNaverLounge.SetActive(GameServerConfig.IsKorea());
            }
            else
            {
                btnCommunity.SetActive(true);
                btnNaverLounge.SetActive(false);
            }

            grid.Reposition();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._2500.ToText()); // 메뉴

            foreach (var item in homeButtons)
            {
                item.Text = GetButtonText(item.Content);
                item.SetOpenConditionText(presenter.GetOpenConditionalText(item.Content));
            }
        }

        void OnSelect(MenuContent content)
        {
            if (!presenter.IsOpenContent(content, isShowPopup: true))
                return;

            switch (content)
            {
                case MenuContent.Skill:
                    UI.Show<UISkill>();
                    break;

                case MenuContent.Guild:
                    Entity.player.Quest.RemoveNewOpenContent(ContentType.Guild);
                    if (presenter.HasGuild())
                        UI.Show<UIGuildMain>();
                    else
                        UI.Show<UIGuild>();
                    break;

                case MenuContent.Dungeon:
                    UI.Show<UIDungeon>();
                    break;

                case MenuContent.FreeFight:
                    UI.Show<UIFreeFight>();
                    break;

                case MenuContent.Pvp:
                    UI.Show<UILeague>();
                    break;

                case MenuContent.GuildBattle:
                    presenter.OnClickedBtnGuildBattle();
                    break;

                case MenuContent.Rank:
                    UI.Show<UIRank>().SetTab(tabIndex: 0);
                    break;

                case MenuContent.Settings:
                    UI.Show<UIOption>();
                    break;

                case MenuContent.Book:
                    // TODO 추후 온버프 NFT 추가 시 구현
                    //if (GameServerConfig.IsOnBuff())
                    //{
                    //    UI.Show<UIBookOnBuff>();
                    //}
                    //else
                    //{
                    //    UI.Show<UIBook>();
                    //}
                    UI.Show<UIBook>();
                    break;

                case MenuContent.Community:

                    if (GameServerConfig.IsKorea())
                    {
                        BasisUrl.NaverHomepage.OpenUrl();
                    }
                    else if (presenter.IsTaiwan())
                    {
                        BasisUrl.TaiwanFaceBookHompage.OpenUrl();
                    }
                    else
                    {
                        BasisUrl.FaceBookHomepage.OpenUrl();
                    }

                    break;

                case MenuContent.NaverLounge:
                    NaverGame.HomeBanner();
                    break;
            }
        }

        private void UpdateGoodsZeny()
        {
            long zeny = presenter.GetZeny();
            titleView.ShowZeny(zeny);
        }

        private void UpdateGoodsCatCoin()
        {
            long catCoin = presenter.GetCatCoin();
            titleView.ShowCatCoin(catCoin);
        }

        private void UpdateNotice()
        {
            foreach (var item in homeButtons)
            {
                item.SetNotice(presenter.GetHasNotice(item.Content));
            }
        }

        private void UpdateGuildNotice()
        {
            UpdateNotice(MenuContent.Guild);
        }

        private void OnBookStateChanged(BookTabType obj)
        {
            UpdateNotice();
        }

        private void UpdateOpenContent()
        {
            foreach (var item in homeButtons)
            {
                if (item == null)
                    continue;

                bool isOpenContent = presenter.IsOpenContent(item.Content, isShowPopup: false);
                if (isOpenContent)
                {
                    item.SetLockType(UIHomeButton.LockType.None);
                }
                else
                {
                    item.SetLockType(presenter.GetLockType(item.Content));
                }
            }
        }

        private void UpdateNewIcon()
        {
            foreach (var item in homeButtons)
            {
                if (item == null)
                    continue;

                item.SetActiveNew(presenter.GetHasNewIcon(item.Content));
            }
        }

        private void UpdateNotice(MenuContent content)
        {
            UIHomeButton button = FindButton(content);
            if (button == null)
                return;

            button.SetNotice(presenter.GetHasNotice(content));
        }

        private UIHomeButton FindButton(MenuContent content)
        {
            foreach (var item in homeButtons)
            {
                if (item.Content == content)
                    return item;
            }

            return null;
        }

        private string GetButtonText(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Skill:
                    return LocalizeKey._2501.ToText(); // 스킬

                case MenuContent.Guild:
                    return LocalizeKey._2503.ToText(); // 길드

                case MenuContent.Dungeon:
                    return LocalizeKey._2504.ToText(); // 던전

                case MenuContent.FreeFight:
                    return LocalizeKey._2515.ToText(); // 난전

                case MenuContent.Pvp:
                    return LocalizeKey._2506.ToText(); // 대전

                case MenuContent.GuildBattle:
                    return LocalizeKey._2518.ToText(); // 길드전

                case MenuContent.Book:
                    return LocalizeKey._40200.ToText(); // 도감

                case MenuContent.Community:
                    return LocalizeKey._2516.ToText(); // 커뮤니티

                case MenuContent.Rank:
                    return LocalizeKey._2505.ToText(); // 랭킹

                case MenuContent.Settings:
                    return LocalizeKey._2510.ToText(); // 설정

                case MenuContent.NaverLounge:
                    return LocalizeKey._2517.ToText(); // 공식 게임 라운지
            }

            Debug.LogError($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");

            return string.Empty;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
            {
                UI.Close<UIHome>();
            }
        }

        public UIWidget GetWidget(MenuContent content)
        {
            foreach (var item in homeButtons)
            {
                if (item.Content == content)
                    return item.GetComponent<UIWidget>();
            }

            return null;
        }

        public override bool Find()
        {
            base.Find();

            homeButtons = GetComponentsInChildren<UIHomeButton>();
            return true;
        }
    }
}