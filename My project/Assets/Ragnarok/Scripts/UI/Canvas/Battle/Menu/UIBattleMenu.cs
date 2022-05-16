using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleMenu : UICanvas, TutorialMazeEnter.ISelectImpl, TutorialMazeExit.IExitImpl
    {
        private const string TAG = nameof(UIBattleMenu);
        private const string TAG_PACKAGE = nameof(UIBattleMenu) + "Package";

        protected override UIType uiType => UIType.Hide | UIType.Fixed;
        public override int layer => Layer.UI_ExceptForCharZoom;

        public enum MenuContent
        {
            /// <summary>
            /// 나가기
            /// </summary>
            Exit = 1,
            /// <summary>
            /// 노점
            /// </summary>
            Trade,
            /// <summary>
            /// 카드 미로
            /// </summary>
            Maze,
            /// <summary>
            /// 듀얼
            /// </summary>
            Duel,
            /// <summary>
            /// 파견
            /// </summary>
            Explore,
            /// <summary>
            /// 버프
            /// </summary>
            Buff,
            /// <summary>
            /// 보스전투
            /// </summary>
            Boss,
            /// <summary>
            /// 룰렛
            /// </summary>
            Roulette,
            /// <summary>
            /// 직업 레벨 패키지
            /// </summary>
            JobLevel,
            /// <summary>
            /// 첫 결제
            /// </summary>
            FirstPayment,
            /// <summary>
            /// 큐펫
            /// </summary>
            Cupet,
            /// <summary>
            /// 길드 아지트
            /// </summary>
            GuildAgit,
            /// <summary>
            /// 고객보상
            /// </summary>
            CustomerReward,
            /// <summary>
            /// 광장
            /// </summary>
            Square,
            /// <summary>
            /// NPC로 이동
            /// </summary>
            NpcMove,
        }

        [SerializeField] UIGrid grid;
        [SerializeField] UIBattleMenuButton btnExit;
        [SerializeField] UIBattleMenuButton btnTrade;
        [SerializeField] UIBattleMenuButton btnMaze;
        [SerializeField] UIDuelButton btnDuel;
        [SerializeField] UIBattleMenuButton btnExplore;
        [SerializeField] UIBattleMenuButton btnBuff;
        [SerializeField] UIBattleMenuButton btnBoss;
        [SerializeField] GameObject fxBuff;
        [SerializeField] UIBattleMenuButton btnRoulette;
        [SerializeField] UILabel labelRouletteCount;
        [SerializeField] UIGrid gridUpperRight;
        [SerializeField] UIBattleMenuButton btnJobLevel;
        [SerializeField] UIBattleMenuButton btnFirstPayment;
        [SerializeField] UIBattleMenuButton btnCupet;
        [SerializeField] UIBattleMenuButton btnGuildAgit;
        [SerializeField] UIBattleMenuButton btnCustomerReward;
        [SerializeField] UIBattleMenuButton btnTradeTown;
        [SerializeField] UIBattleMenuButton btnNpcMove;
        [SerializeField] UIGraySprite sprNpcMove;

        public event Action OnExit;
        public event Action OnTempMaze;
        public event Action OnBoss;
        public event Action OnNpcMove;

        BattleMenuPresenter presenter;

        public UIWidget DuelButtonWidget => btnDuel.GetComponent<UIWidget>();
        public UIWidget TradeButtonWidget => btnTrade.GetComponent<UIWidget>();

        private MenuContent[] currentContents;

        public Vector3 GetDuelPos()
        {
            return btnDuel.transform.position;
        }

        public Vector3 GetRoulettePos()
        {
            return btnRoulette.transform.position;
        }

        protected override void OnInit()
        {
            presenter = new BattleMenuPresenter();

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnTrade.OnClick, OnClickedBtnTrade);
            EventDelegate.Add(btnMaze.OnClick, OnClickedBtnMaze);
            EventDelegate.Add(btnDuel.OnClick, OnClickedBtnDual);
            EventDelegate.Add(btnExplore.OnClick, OnClickedExplore);
            EventDelegate.Add(btnBuff.OnClick, OnClickedBuff);
            EventDelegate.Add(btnBoss.OnClick, OnClickedBtnBoss);
            EventDelegate.Add(btnRoulette.OnClick, OnClickedBtnRoulette);
            EventDelegate.Add(btnJobLevel.OnClick, OnClickedBtnJobLevel);
            EventDelegate.Add(btnFirstPayment.OnClick, OnClickedBtnFirstPayment);
            EventDelegate.Add(btnCupet.OnClick, OnClickedBtnCupet);
            EventDelegate.Add(btnGuildAgit.OnClick, OnClickedBtnGuildAgit);
            EventDelegate.Add(btnCustomerReward.OnClick, OnClickedBtnCustomerReward);
            EventDelegate.Add(btnTradeTown.OnClick, OnClickedBtnSquare);
            EventDelegate.Add(btnNpcMove.OnClick, OnClickedBtnNpcMove);
            btnDuel.OnDoubleSelect += OnDoubleSelectBtnMaze;

            presenter.OnUpdateDuelPoint += UpdateDuelPoint;
            presenter.OnUpdateAlarm += UpdateDuelNotice;
            presenter.OnUpdateBuff += UpdateBuffEffect;
            presenter.OnUpdateNewOpenContent += UpdateNewIcon;
            presenter.OnUpdateItem += UpdateRouletteNotice;
            presenter.OnUpdateExploreState += UpdateExploreNotice;
            presenter.OnUpdatePackageJobLevel += UpdateJobLevelRemainTime;
            presenter.OnUpdateShopMail += UpdateOpenContent;
            presenter.OnUpdateKafra += UpdateSquareNotice;
            presenter.OnUpdateNabiho += UpdateSquareNotice;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(btnTrade.OnClick, OnClickedBtnTrade);
            EventDelegate.Remove(btnMaze.OnClick, OnClickedBtnMaze);
            EventDelegate.Remove(btnDuel.OnClick, OnClickedBtnDual);
            EventDelegate.Remove(btnExplore.OnClick, OnClickedExplore);
            EventDelegate.Remove(btnBuff.OnClick, OnClickedBuff);
            EventDelegate.Remove(btnBoss.OnClick, OnClickedBtnBoss);
            EventDelegate.Remove(btnRoulette.OnClick, OnClickedBtnRoulette);
            EventDelegate.Remove(btnJobLevel.OnClick, OnClickedBtnJobLevel);
            EventDelegate.Remove(btnFirstPayment.OnClick, OnClickedBtnFirstPayment);
            EventDelegate.Remove(btnCupet.OnClick, OnClickedBtnCupet);
            EventDelegate.Remove(btnGuildAgit.OnClick, OnClickedBtnGuildAgit);
            EventDelegate.Remove(btnCustomerReward.OnClick, OnClickedBtnCustomerReward);
            EventDelegate.Remove(btnTradeTown.OnClick, OnClickedBtnSquare);
            EventDelegate.Remove(btnNpcMove.OnClick, OnClickedBtnNpcMove);
            btnDuel.OnDoubleSelect -= OnDoubleSelectBtnMaze;

            presenter.OnUpdateDuelPoint -= UpdateDuelPoint;
            presenter.OnUpdateAlarm -= UpdateDuelNotice;
            presenter.OnUpdateBuff -= UpdateBuffEffect;
            presenter.OnUpdateNewOpenContent -= UpdateNewIcon;
            presenter.OnUpdateItem -= UpdateRouletteNotice;
            presenter.OnUpdateExploreState -= UpdateExploreNotice;
            presenter.OnUpdatePackageJobLevel -= UpdateJobLevelRemainTime;
            presenter.OnUpdateShopMail -= UpdateOpenContent;
            presenter.OnUpdateKafra -= UpdateSquareNotice;
            presenter.OnUpdateNabiho -= UpdateSquareNotice;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            SetMode(null); // 초기화
            //UpdateOpenContent();
            //UpdateNotice();
            UpdateBuffEffect();
            UpdateNewIcon();
            UpdateExploreNotice();
            UpdateSquareNotice();

            Timing.RunCoroutine(NoticeChecker(), TAG);
        }

        protected override void OnHide()
        {
            Timing.KillCoroutines(TAG);
        }

        protected override void OnLocalize()
        {
            // 모든 버튼 Hide
            foreach (MenuContent item in Enum.GetValues(typeof(MenuContent)))
            {
                UIBattleMenuButton button = GetButton(item);
                if (button == null)
                    continue;

                button.Text = GetButtonText(item);
            }
        }

        private IEnumerator<float> NoticeChecker()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(10);
                UpdateExploreNotice();
            }
        }

        void OnClickedExplore()
        {
            SelectContent(MenuContent.Explore);
        }

        void OnClickedBuff()
        {
            SelectContent(MenuContent.Buff);
        }

        void OnClickedBtnBoss()
        {
            SelectContent(MenuContent.Boss);
        }

        void OnClickedBtnRoulette()
        {
            SelectContent(MenuContent.Roulette);
        }

        void OnClickedBtnExit()
        {
            SelectContent(MenuContent.Exit);
        }

        void OnClickedBtnMaze()
        {
            SelectContent(MenuContent.Maze);
        }

        void OnClickedBtnTrade()
        {
            SelectContent(MenuContent.Trade);
        }

        void OnClickedBtnDual()
        {
            SelectContent(MenuContent.Duel);
        }

        void OnClickedBtnJobLevel()
        {
            SelectContent(MenuContent.JobLevel);
        }

        void OnClickedBtnFirstPayment()
        {
            SelectContent(MenuContent.FirstPayment);
        }

        void OnDoubleSelectBtnMaze()
        {
            OnTempMaze?.Invoke();
        }

        void OnClickedBtnCupet()
        {
            SelectContent(MenuContent.Cupet);
        }

        void OnClickedBtnGuildAgit()
        {
            SelectContent(MenuContent.GuildAgit);
        }

        void OnClickedBtnCustomerReward()
        {
            SelectContent(MenuContent.CustomerReward);
        }

        void OnClickedBtnSquare()
        {
            SelectContent(MenuContent.Square);
        }

        void OnClickedBtnNpcMove()
        {
            SelectContent(MenuContent.NpcMove);
        }

        private void SelectContent(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Exit:
                    OnExit?.Invoke();
                    break;

                case MenuContent.Trade:
                    if (!BasisOpenContetsType.TradeShop.IsOpend())
                    {
                        string message = LocalizeKey._90045.ToText(); // 업데이트 예정입니다.
                        UI.ShowToastPopup(message);
                        return;
                    }

                    UI.Show<UIPrivateStore>().ShowMine();
                    break;

                case MenuContent.Maze:
                    UI.Show<UIAdventureMazeSelect>();
                    break;

                case MenuContent.Duel:
                    presenter.RemoveAlarm(AlarmType.Duel);
                    UI.Show<UIDuel>();
                    break;

                case MenuContent.Explore:
                    UI.Show<UIAgent>(new UIAgent.Input()
                    {
                        viewAgentType = AgentType.ExploreAgent
                    });
                    break;

                case MenuContent.Buff:
                    UI.Show<UIBuffInfo>();
                    break;

                case MenuContent.Boss:
                    OnBoss?.Invoke();
                    break;

                case MenuContent.Roulette:
                    UI.Show<UIRouletteEvent>();
                    break;

                case MenuContent.JobLevel:
                    UI.Show<UIPackageJobLevel>();
                    break;

                case MenuContent.FirstPayment:
                    UI.Show<UIPackageFirstPayment>();
                    break;

                case MenuContent.Cupet:
                    presenter.ShowGuildCupet();
                    break;

                case MenuContent.GuildAgit:
                    UI.ShowToastPopup(LocalizeKey._90045.ToText()); // 업데이트 예정입니다.
                    break;

                case MenuContent.CustomerReward:
                    UI.Show<UICustomerReward>();
                    break;

                case MenuContent.Square:
                    UI.Show<UISquare>();
                    break;

                case MenuContent.NpcMove:
                    OnNpcMove?.Invoke();
                    break;
            }
        }

        public void SetMode(params MenuContent[] args)
        {
            currentContents = args;

            HideButtons(); // 모든 버튼 Hide

            // 특정 버튼 Show
            int size = currentContents == null ? 0 : currentContents.Length;
            for (int i = 0; i < size; i++)
            {
                MenuContent content = currentContents[i];

                UIBattleMenuButton button = GetButton(content);
                if (!button)
                    continue;

                button.SetActive(true);
                button.SetAsLastSibling(); // 가장 마지막 Child로 세팅
            }

            grid.Reposition();

            UpdateOpenContent();
            UpdateNotice();
            UpdateJobLevelRemainTime();
        }

        public void UpdateBossButtonSprite(int questCoinCount)
        {
            btnBoss.SetIconName(GetBossIconName(questCoinCount));
        }

        public UIWidget GetDuelTarget()
        {
            return btnDuel.GetMainWidget();
        }

        private void HideButtons()
        {
            foreach (MenuContent item in Enum.GetValues(typeof(MenuContent)))
            {
                UIBattleMenuButton button = GetButton(item);

                if (!button)
                    continue;

                button.SetActive(false);
            }
        }

        private void UpdateOpenContent()
        {
            foreach (MenuContent item in Enum.GetValues(typeof(MenuContent)))
            {
                UpdateOpenContent(item);
            }
            GridUpperRightReposition();
        }

        private void UpdateNotice()
        {
            foreach (MenuContent item in Enum.GetValues(typeof(MenuContent)))
            {
                UpdateNotice(item);
            }
        }

        private void UpdateNewIcon()
        {
            foreach (MenuContent item in Enum.GetValues(typeof(MenuContent)))
            {
                UIBattleMenuButton button = GetButton(item);

                if (!button)
                    continue;

                button.SetActiveNew(presenter.GetHasNewIcon(item));
            }
        }

        /// <summary>
        /// 적용중인 버프 이펙트 업데이트
        /// </summary>
        private void UpdateBuffEffect()
        {
            fxBuff.SetActive(presenter.IsUseBuff());
        }

        private void UpdateDuelNotice()
        {
            UpdateNotice(MenuContent.Duel);
        }

        private void UpdateRouletteNotice()
        {
            UpdateNotice(MenuContent.Roulette);
        }

        private void UpdateExploreNotice()
        {
            UpdateNotice(MenuContent.Explore);
        }

        private void UpdateSquareNotice()
        {
            UpdateNotice(MenuContent.Square);
        }

        public void UpdateOpenContent(MenuContent content)
        {
            // 현재 Menu 모드에 포함 여부 체크
            if (!IsContainCurrentContent(content))
                return;

            UIBattleMenuButton button = GetButton(content);
            if (button == null)
                return;

            button.SetActive(presenter.IsOpenContent(content, isShowPopup: false));

            if (content == MenuContent.Duel)
            {
                UpdateDuelPoint();
            }
        }

        public void GridUpperRightReposition()
        {
            gridUpperRight.Reposition();
        }

        private bool IsContainCurrentContent(MenuContent content)
        {
            if (currentContents == null)
                return false;

            foreach (var item in currentContents)
            {
                if (item == content)
                    return true;
            }

            return false;
        }

        private void UpdateNotice(MenuContent content)
        {
            UIBattleMenuButton button = GetButton(content);

            if (!button)
                return;

            button.SetNotice(presenter.GetHasNotice(content));
        }

        private void UpdateDuelPoint()
        {
            int cur = presenter.GetCurDuelPoint();
            int max = presenter.GetMaxDuelPoint();
            bool canDuel = presenter.CanDuel();
            btnDuel.SetProgress(cur, max);
            btnDuel.SetActiveAnimation(canDuel);
        }

        private void UpdateJobLevelRemainTime()
        {
            if (!IsContainCurrentContent(MenuContent.JobLevel))
                return;

            Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), TAG_PACKAGE, SingletonBehavior.Overwrite);
            GridUpperRightReposition();
        }

        private IEnumerator<float> YieldRemainTime()
        {
            while (true)
            {
                float time = presenter.GetJobLevelPackageRemainTime();
                if (time <= 0)
                    break;

                UpdateLimitTime(time);
                yield return Timing.WaitForSeconds(0.5f);
            }

            UpdateLimitTime(0);
            GridUpperRightReposition();
        }

        private void UpdateLimitTime(float time)
        {
            // 5분 미만으로 떨여졌을 때, 토스트 팝업으로 알림
            const int SHOW_JOB_LEVEL_PACKAGE_POPUP_MINUTES = 5;

            if (time <= 0)
            {
                btnJobLevel.SetActive(false);
                return;
            }

            btnJobLevel.SetActive(true);
            // UI 표시에 1분을 추가해서 보여준다.
            TimeSpan span = TimeSpan.FromMilliseconds(time + 60000);
            int totalDays = (int)span.TotalDays;
            bool isDay = totalDays > 0;

            if (isDay)
            {
                btnJobLevel.Text = LocalizeKey._8041.ToText().Replace(ReplaceKey.TIME, totalDays); // D-{TIME}
            }
            else
            {
                btnJobLevel.Text = span.ToString(@"hh\:mm");
            }

            if (span.TotalMinutes <= SHOW_JOB_LEVEL_PACKAGE_POPUP_MINUTES)
                ShowJobLevelPackageNotice();
        }

        private void ShowJobLevelPackageNotice()
        {
            // 이미 팝업 보여줬음
            if (!presenter.GetIsNeedShowJobLevelPopup())
                return;

            // Fade 화면이 가리고 있음
            UIFade uiFade = UI.GetUI<UIFade>();
            if (uiFade != null && uiFade.IsVisible)
                return;

            presenter.SetIsNeedShowJobLevelPopup(); // 팝업 플래그 변환
            UI.ShowToastPopup(LocalizeKey._2111.ToText()); // 잠시 후, 레벨 달성 패키지의 구매가 불가능해집니다.
        }

        private UIBattleMenuButton GetButton(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Exit:
                    return btnExit;

                case MenuContent.Trade:
                    return btnTrade;

                case MenuContent.Maze:
                    return btnMaze;

                case MenuContent.Duel:
                    return btnDuel;

                case MenuContent.Explore:
                    return btnExplore;

                case MenuContent.Buff:
                    return btnBuff;

                case MenuContent.Boss:
                    return btnBoss;

                case MenuContent.Roulette:
                    return btnRoulette;

                case MenuContent.JobLevel:
                    return btnJobLevel;

                case MenuContent.FirstPayment:
                    return btnFirstPayment;

                case MenuContent.Cupet:
                    return btnCupet;

                case MenuContent.GuildAgit:
                    return btnGuildAgit;

                case MenuContent.CustomerReward:
                    return btnCustomerReward;

                case MenuContent.Square:
                    return btnTradeTown;

                case MenuContent.NpcMove:
                    return btnNpcMove;
            }

            return null;
        }

        private string GetButtonText(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Exit:
                    return LocalizeKey._2100.ToText(); // 나가기

                case MenuContent.Trade:
                    return LocalizeKey._2101.ToText(); // 노점

                case MenuContent.Maze:
                    return LocalizeKey._2104.ToText(); // 카드 미로

                case MenuContent.Duel:
                    return LocalizeKey._2103.ToText(); // 듀얼

                case MenuContent.Explore:
                    return LocalizeKey._2105.ToText(); // 파견

                case MenuContent.Buff:
                    return LocalizeKey._3039.ToText(); // 버프

                case MenuContent.Boss:
                    return LocalizeKey._2106.ToText(); // 바로 도전

                case MenuContent.Roulette:
                    return LocalizeKey._2107.ToText(); // 룰렛

                case MenuContent.FirstPayment:
                    return LocalizeKey._2110.ToText(); // 첫 결제

                case MenuContent.Cupet:
                    return LocalizeKey._33115.ToText(); // 큐펫

                case MenuContent.GuildAgit:
                    return LocalizeKey._2112.ToText(); // 아지트

                case MenuContent.CustomerReward:
                    return LocalizeKey._2113.ToText(); // 고객보상

                case MenuContent.Square:
                    return LocalizeKey._10800.ToText(); // 광장

                case MenuContent.NpcMove:
                    return LocalizeKey._2114.ToText(); // 관리자
            }

            return string.Empty;
        }

        private string GetBossIconName(int questCoinCount)
        {
            // 보스 도전 가능
            if (questCoinCount >= Constants.Battle.MATCH_MULTI_NEED_TO_BOSS_BATTLE)
                return "Ui_Common_Icon_Top_Specialmaze_2";

            return "Ui_Common_Icon_Top_Specialmaze_1";
        }

        public void SetNpcMoveIcon(bool isGrayScale)
        {
            sprNpcMove.Mode = isGrayScale ? UIGraySprite.SpriteMode.Grayscale : UIGraySprite.SpriteMode.None;
        }

        public UISprite GetMenuIcon(MenuContent content)
        {
            UIBattleMenuButton button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return null;
            }
#endif

            return button.GetIcon();
        }

        #region Tutorial

        UIWidget TutorialMazeEnter.ISelectImpl.GetBtnMazeWidget()
        {
            return GetButton(MenuContent.Maze).GetComponent<UIWidget>();
        }

        UIWidget TutorialMazeExit.IExitImpl.GetBtnExitWidget()
        {
            return GetButton(MenuContent.Exit).GetComponent<UIWidget>();
        }

        #endregion
    }
}