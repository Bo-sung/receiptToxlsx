using MEC;
using Ragnarok.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIDuel : UICanvas
    {
        public class Input : IUIData
        {
            public bool needRequestDuelInfo;
            public State duelState;
        }

        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        public enum EventType
        {
            OnClickChapter,
            OnClickAlphabet,
            OnClickCharge,
            OnClickRecieveReward,
            OnClickAgent,
            OnSysRequestShowChapter,
            OnClickEvent,
            OnClickArena,
        }

        public enum State
        {
            /// <summary>
            /// 일반 챕터
            /// </summary>
            Chapter = 1,
            /// <summary>
            /// 이벤트 서버 대항전
            /// </summary>
            Event,
            /// <summary>
            /// 아레나
            /// </summary>
            Arena,
        }

        [SerializeField] TitleView titleView;
        [SerializeField] UIDuelAlphabetCollection alphabetTemplates;
        [SerializeField] GameObject[] wordRoots;
        [SerializeField] GameObject[] myJobIcons;
        [SerializeField] GameObject[] myJobOutlines;
        [SerializeField] UIRewardHelper[] reward;
        [SerializeField] GameObject[] completeMarks;
        [SerializeField] UILabel[] rewardOrderLabels;
        [SerializeField] UILabelHelper alphabetCountPanelLabel;
        [SerializeField] UILabelHelper alphabetCountLabel;
        [SerializeField] UIButtonHelper getRewardButton;
        [SerializeField] UISprite rewardButtonSprite;
        [SerializeField] UILabel rewardButtonLabel;
        [SerializeField] UIDuelEventSlot btnEventDuel;
        [SerializeField] UIDuelEventSlot btnArenaDuel;

        [SerializeField] UIScrollView chapterScroll;
        [SerializeField] GameObject chapterScrollContentsRoot;
        [SerializeField] UIDuelChapterListSlot chapterPrefab;

        [SerializeField] SuperScrollListWrapper historyScroll;
        [SerializeField] GameObject historySlotPrefab;
        [SerializeField] UILabelHelper noHistoryNotice;

        [SerializeField] UILabelHelper noticeLabel;

        [SerializeField] GameObject alphabetPanelRoot;
        [SerializeField] GameObject completePanelRoot;

        [SerializeField] UILabelHelper labelMainNotice;
        [SerializeField] UILabelHelper labelAttackWin;
        [SerializeField] UILabelHelper labelDefenseWin;
        [SerializeField] UILabelHelper labelAttackDefeat;
        [SerializeField] UILabelHelper labelDefenseDefeat;
        [SerializeField] UIButtonHelper btnAgent;

        [SerializeField] UIWidget duelWordPanelWidget;
        [SerializeField] UIWidget combatAgentButtonWidget;

        [SerializeField] GameObject duelDefault;
        [SerializeField] UILabelValue duelLock;

        [SerializeField] UIDuelCombatAngetProfileSlot[] combatAgentSlots;
        [SerializeField] UIButton closeButton;
        [SerializeField] GameObject rewardFX;

        [SerializeField] GameObject normalView;
        [SerializeField] DuelEventView duelEventView;
        [SerializeField] DuelArenaView duelArenaView;

        [SerializeField] UIButtonHelper btnHelp;

        [SerializeField] GameObject lockGridObject;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabel labelWaitingTime;
        [SerializeField] GameObject lockEventGridObject;
        [SerializeField] UILabelHelper labelEventDesc;
        [SerializeField] UILabel labelEventWaitingTime;
        [SerializeField] GameObject lockArenaGridObject;
        [SerializeField] UILabelHelper labelArenaDesc;
        [SerializeField] UILabel labelArenaWaitingTime;

        [SerializeField] UIScrollBar scrollBar;

        private Color32 normal = new Color32(63, 136, 188, 255);
        private Color32 disabled = new Color32(125, 125, 125, 255);

        private DuelPresenter presenter;
        private UIDuelChapterListSlot[] chapterSlots;
        private List<CharDuelHistory> showingHistoryList;

        public UIWidget DuelWordPanelWidget { get { return duelWordPanelWidget; } }
        public UIWidget CombatAgentButtonWidget { get { return combatAgentButtonWidget; } }

        protected override void OnInit()
        {
            showingHistoryList = new List<CharDuelHistory>();

            presenter = new DuelPresenter(this);
            presenter.OnUpdateNewAgent += UpdateAgentNotice;
            presenter.OnUpdateEquippedAgent += UpdateAgentNotice;
            presenter.OnUpdateZeny += UpdateZeny;
            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.OnResetWaitingTime += UpdateWaitingTime;
            presenter.OnResetEventWaitingTime += UpdateWaitingEventTime;
            presenter.OnUpdateCharacterInfo += UpdateCharacterInfo;
            presenter.OnUpdateCharacterGender += UpdateCharacterInfo;
            presenter.OnResetArenaWaitingTime += UpdateWaitingArenaTime;
            presenter.AddEvent();
            EventDelegate.Add(getRewardButton.OnClick, OnClickGetReward);
            EventDelegate.Add(btnAgent.OnClick, OnClickedBtnAgent);
            EventDelegate.Add(closeButton.onClick, OnClickClose);
            EventDelegate.Add(btnEventDuel.OnClick, ShowEventView);
            EventDelegate.Add(btnArenaDuel.OnClick, ShowArenaView);
            EventDelegate.Add(btnHelp.OnClick, OnClickBtnHelp);

            historyScroll.SpawnNewList(historySlotPrefab, 0, 0);
            historyScroll.SetRefreshCallback(OnHistoryScrollRefresh);

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.InitializeRewardData();

            duelEventView.OnSelect += OnSelectServer;
            duelEventView.OnReward += presenter.RequestEventReward;
            duelEventView.OnRefresh += RefreshEventDuel;
            duelEventView.OnSelectGift += ShowEventDuelRewardUI;
            duelEventView.OnSelectRanking += ShowEventDuelRankingUI;

            duelArenaView.OnSelectUserInfo += presenter.ShowOtherUserInfo;
            duelArenaView.OnSelectEntry += OnSelectArena;
            duelArenaView.OnSelectBuyFlag += presenter.RequestArenaPointBuy;
            duelArenaView.OnReward += presenter.RequestArenaReward;
            duelArenaView.OnSelectRanking += ShowArenaDuelRankingUI;
            duelArenaView.OnRefresh += RefreshArenaDuel;
        }

        protected override void OnClose()
        {
            duelArenaView.OnSelectUserInfo -= presenter.ShowOtherUserInfo;
            duelArenaView.OnSelectEntry -= OnSelectArena;
            duelArenaView.OnSelectBuyFlag -= presenter.RequestArenaPointBuy;
            duelArenaView.OnReward -= presenter.RequestArenaReward;
            duelArenaView.OnSelectRanking -= ShowArenaDuelRankingUI;
            duelArenaView.OnRefresh -= RefreshArenaDuel;

            duelEventView.OnSelect -= OnSelectServer;
            duelEventView.OnReward -= presenter.RequestEventReward;
            duelEventView.OnRefresh -= RefreshEventDuel;
            duelEventView.OnSelectGift -= ShowEventDuelRewardUI;
            duelEventView.OnSelectRanking -= ShowEventDuelRankingUI;

            presenter.OnUpdateNewAgent -= UpdateAgentNotice;
            presenter.OnUpdateEquippedAgent -= UpdateAgentNotice;
            presenter.OnUpdateZeny -= UpdateZeny;
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.OnResetWaitingTime -= UpdateWaitingTime;
            presenter.OnResetEventWaitingTime -= UpdateWaitingEventTime;
            presenter.OnUpdateCharacterInfo -= UpdateCharacterInfo;
            presenter.OnUpdateCharacterGender -= UpdateCharacterInfo;
            presenter.OnResetArenaWaitingTime -= UpdateWaitingArenaTime;
            presenter.RemoveEvent();
            EventDelegate.Remove(getRewardButton.OnClick, OnClickGetReward);
            EventDelegate.Remove(btnAgent.OnClick, OnClickedBtnAgent);
            EventDelegate.Remove(closeButton.onClick, OnClickClose);
            EventDelegate.Remove(btnEventDuel.OnClick, ShowEventView);
            EventDelegate.Remove(btnArenaDuel.OnClick, ShowArenaView);
            EventDelegate.Remove(btnHelp.OnClick, OnClickBtnHelp);
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._47828.ToText()); // 듀얼
            getRewardButton.LocalKey = LocalizeKey._47830; // 받기
            labelMainNotice.LocalKey = LocalizeKey._47801; // 이 챕터의 모든 보상을 획득하셨어요.\n축하드립니다!
            noHistoryNotice.LocalKey = LocalizeKey._47802; // 듀얼 내역이 없습니다.
            labelAttackWin.LocalKey = LocalizeKey._47803; // 공격 성공
            labelDefenseWin.LocalKey = LocalizeKey._47804; // 공격 실패
            labelAttackDefeat.LocalKey = LocalizeKey._47805; // 방어 성공
            labelDefenseDefeat.LocalKey = LocalizeKey._47806; // 방어 실패
            btnAgent.LocalKey = LocalizeKey._47032; // 동료 장착
            duelLock.TitleKey = LocalizeKey._47832; // 듀얼 잠금 해제
            duelLock.Value = presenter.OpenContentText();
            alphabetCountPanelLabel.Text = LocalizeKey._47833.ToText(); // 듀얼 조각
            rewardOrderLabels[0].text = LocalizeKey._47834.ToText().Replace(ReplaceKey.VALUE, 1);
            rewardOrderLabels[1].text = LocalizeKey._47834.ToText().Replace(ReplaceKey.VALUE, 2);
            btnEventDuel.LocalKey = LocalizeKey._47829; // 이벤트
            btnArenaDuel.LocalKey = LocalizeKey._47866; // 아레나
            labelDesc.LocalKey = LocalizeKey._47849; // 대기 시간
            labelEventDesc.LocalKey = LocalizeKey._47849; // 대기 시간
            labelArenaDesc.LocalKey = LocalizeKey._47849; // 대기 시간
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_Duel(); // 신규 컨텐츠 플래그 제거

            NGUITools.SetActive(normalView, false);
            duelEventView.Hide();
            duelArenaView.Hide();

            Input input = data as Input;
            alphabetPanelRoot.SetActive(false);
            completePanelRoot.SetActive(false);

            if (input == null)
            {
                presenter.OnShow(needRequestDuelInfo: true, duelState: presenter.GetRecentlyState());
            }
            else
            {
                presenter.OnShow(input.needRequestDuelInfo, input.duelState);
            }

            UpdateAgentNotice();
            UpdateOpenContent();
            UpdateWaitingTime();
            UpdateWaitingEventTime();
            UpdateCharacterInfo();
            UpdateWaitingArenaTime();
        }

        protected override void OnHide()
        {
        }

        private void OnClickBtnHelp()
        {
            int info_id = DungeonInfoType.Duel.GetDungeonInfoId();
            UI.Show<UIDungeonInfoPopup>().Show(info_id);
        }

        public void UpdateNoticeLabel()
        {
            Job jobGroup = presenter.GetJobGroup();
            string jobGroupName = presenter.GetJobGroupName();
            noticeLabel.Text = LocalizeKey._47811.ToText() // [c][{COLOR}]{NAME} 계열[-][/c] 듀얼 조각은 멀티 미로에서 획득할 수 있습니다.
                .Replace(ReplaceKey.COLOR, GetHexCode(jobGroup))
                .Replace(ReplaceKey.NAME, jobGroupName);
        }

        public void RequestShowChapter(int chapter)
        {
            presenter.ViewEventHandler(EventType.OnSysRequestShowChapter, chapter);
        }

        void OnSelectServer(DuelServerPacket packet)
        {
            UI.Show<UIDuelPlayerList>(new UIDuelPlayerList.EventInput { packet = packet });
        }

        void OnSelectArena()
        {
            UI.Show<UIDuelPlayerList>(new UIDuelPlayerList.ArenaInput
            {
                arenaPoint = presenter.GetArenaPoint(),
                winCount = presenter.GetArenaWinCount(),
                loseCount = presenter.GetArenaLoseCount(),
            });
        }

        public void SetChapters(AdventureData[] adventureDatas, int lastClearedChpater)
        {
            if (chapterSlots == null)
            {
                chapterSlots = new UIDuelChapterListSlot[adventureDatas.Length];
                for (int i = 0; i < adventureDatas.Length; ++i)
                {
                    chapterSlots[i] = NGUITools.AddChild(chapterScrollContentsRoot, chapterPrefab.gameObject).GetComponent<UIDuelChapterListSlot>();
                    chapterSlots[i].transform.localPosition = new Vector3(143 * (i + 1), 0, 0);
                }

                chapterScroll.UpdatePosition();
                chapterScroll.SetDragAmount(0, 0, false);
            }

            // 챕터 슬롯 정보
            for (int i = 0; i < chapterSlots.Length; ++i)
            {
                chapterSlots[i].SetChapter(adventureDatas[i], OnClickChapter);
                chapterSlots[i].SetIsOpened(adventureDatas[i].chapter <= lastClearedChpater);
                chapterSlots[i].SetSelection(false);
            }
        }

        public void ShowDefaultView()
        {
            NGUITools.SetActive(normalView, true);
            duelEventView.Hide();
            duelArenaView.Hide();
            btnEventDuel.SetSelection(false);
            btnArenaDuel.SetSelection(false);
        }

        public void ShowEventView()
        {
            // 이미 켜져있을 경우
            if (duelEventView.IsShow)
                return;

            presenter.ViewEventHandler(EventType.OnClickEvent, null);

            NGUITools.SetActive(normalView, false);
            duelEventView.Show();
            duelEventView.Initialize(presenter.GetEventRewards(), presenter.GetEventConditionValues());

            btnEventDuel.SetSelection(true);
            btnArenaDuel.SetSelection(false);

            for (int i = 0; i < chapterSlots.Length; ++i)
            {
                chapterSlots[i].SetSelection(false);
            }
        }

        public void ShowArenaView()
        {
            // 이미 켜져있을 경우
            if (duelArenaView.IsShow)
                return;
            if (!presenter.CanEnterArena(isShowMessage: true))
                return;

            bool isShowIndicator = true;
            presenter.ViewEventHandler(EventType.OnClickArena, isShowIndicator);

            NGUITools.SetActive(normalView, false);
            duelArenaView.Show();
            duelArenaView.Initialize(presenter.GetArenaRewards(), presenter.GetArenaConditionValues(), presenter.arenaFlagCatCoin);

            btnEventDuel.SetSelection(false);
            btnArenaDuel.SetSelection(true);

            for (int i = 0; i < chapterSlots.Length; ++i)
            {
                chapterSlots[i].SetSelection(false);
            }
        }

        public void SetEventView(int seasonSeq, long remainTime, long nextRemainTime, int myRank, int winCount, RewardData[] serverRewards)
        {
            duelEventView.SetData(seasonSeq, remainTime, nextRemainTime, myRank, winCount, serverRewards);
        }

        public void SetEventViewRewardStep(int rewardStep)
        {
            duelEventView.SetRewardStep(rewardStep);
        }

        public void ShowEventViewServers(DuelServerPacket[] packets)
        {
            duelEventView.ShowServers(packets);
        }

        public void ShowEventViewCalculateNotice()
        {
            duelEventView.ShowCalculateNotice();
        }

        public void ShowEventViewRank(UIDuelBuffReward.IInput perfectRank, UIDuelBuffReward.IInput normalRank)
        {
            duelEventView.ShowRank(perfectRank, normalRank);
        }

        public void SetArenaView(long remainTime, UIDuelArenaHistoryElement.IInput[] histories)
        {
            duelArenaView.SetData(remainTime, histories);
        }

        public void SetArenaViewPoint(int arenaPoint)
        {
            duelArenaView.SetArena(arenaPoint, presenter.FindArena(arenaPoint), presenter.FindNextArena(arenaPoint));
        }

        public void SetArenaViewRewardStep(int rewardStep)
        {
            duelArenaView.SetRewardStep(rewardStep);
        }

        public void ShowArenaViewCalculateNotice(long remainTime)
        {
            duelArenaView.ShowCalculateNotice(remainTime);
        }

        public void SetDuelState(ChapterDuelState duelState, bool isOnShow)
        {
            int selectSlotIdx = 0;

            for (int i = 0; i < chapterSlots.Length; ++i)
            {
                if (chapterSlots[i].Chapter == duelState.Chapter)
                {
                    selectSlotIdx = chapterSlots[i].Chapter; // 이벤트가 제일 앞(0번)에 있기 때문에 1번부터 사용 함.

                    chapterSlots[i].SetSelection(true);
                }
                else
                {
                    chapterSlots[i].SetSelection(false);
                }
            }

            if (isOnShow) // 슬롯리스트 위치 초기화
            {
                if (duelEventView.IsShow) // 이벤트 듀얼일경우
                    selectSlotIdx = 0;
                else if (duelArenaView.IsShow)
                    selectSlotIdx = 0;

                int maxIdx = chapterSlots.Length - 1;
                int idx = selectSlotIdx;
                float progress = (float)idx / maxIdx;

                scrollBar.value = progress;
                //UICenterOnSelectSlot.InitCenter(selectChapterSlot.gameObject, isOnShow);
            }

            int maxAlphabetCount = 0;
            int curAlphabetCount = 0;

            for (int i = 0; i < wordRoots.Length; ++i)
            {
                UIDuelAlphabet alphabet = wordRoots[i].GetComponentInChildren<UIDuelAlphabet>(true);

                if (alphabet != null)
                {
                    Destroy(alphabet.gameObject);
                    alphabet = null;
                }

                if (duelState.IsValidAlphabet(i))
                {
                    ++maxAlphabetCount;
                    if (duelState.IsOwningAlphabet(i))
                    {
                        alphabet = Instantiate(alphabetTemplates.GetTemplate(duelState.CurDuelRewardData.color_index));
                        ++curAlphabetCount;
                    }
                    else
                    {
                        alphabet = Instantiate(alphabetTemplates.GetTemplate(0));
                    }
                }

                if (alphabet != null)
                {
                    alphabet.transform.parent = wordRoots[i].transform;
                    alphabet.transform.localPosition = Vector3.zero;
                    alphabet.transform.localScale = Vector3.one;
                    alphabet.gameObject.SetActive(true);
                    alphabet.SetData(i, duelState.DuelWord[i], OnClickAlphabet);
                }
            }

            int index = 0;
            foreach (var each in duelState.RewardDatas)
                reward[index++].SetData(new RewardData(each.reward_type, each.reward_value, each.reward_count));
            for (int i = 0; i < completeMarks.Length; ++i)
                completeMarks[i].SetActive(i < duelState.RewardedCount);

            bool canReceiveReward = (duelState.RewardedCount < 2 && curAlphabetCount == maxAlphabetCount);
            rewardButtonSprite.spriteName = canReceiveReward ? "Ui_Common_Btn_04" : "Ui_Common_Btn_03";
            rewardButtonLabel.effectColor = canReceiveReward ? normal : disabled;
            alphabetCountLabel.Text = duelState.RewardedCount == 2 ? LocalizeKey._47335.ToText() : string.Concat(curAlphabetCount, "/", maxAlphabetCount);
            NGUITools.SetActive(rewardFX, canReceiveReward);
        }

        public void SetCombatAgents(CombatAgent[] combatAgents, int validSlotCount)
        {
            for (int i = 0; i < combatAgentSlots.Length; ++i)
            {
                combatAgentSlots[i].SetLabelLock(i); // 잠금라벨 셋팅

                if (i < combatAgents.Length)
                    combatAgentSlots[i].SetData(combatAgents[i], i < validSlotCount);
                else
                    combatAgentSlots[i].SetData(null, i < validSlotCount);
            }
        }

        public void SetMyJobIcon(Job jobGroup)
        {
            for (int i = 0; i < myJobIcons.Length; ++i)
            {
                myJobOutlines[i].SetActive(false);
                myJobIcons[i].SetActive(false);
            }

            if (jobGroup == Job.Swordman)
            {
                myJobOutlines[0].SetActive(true);
                myJobIcons[0].SetActive(true);
            }
            else if (jobGroup == Job.Magician)
            {
                myJobOutlines[1].SetActive(true);
                myJobIcons[1].SetActive(true);
            }
            else if (jobGroup == Job.Thief)
            {
                myJobOutlines[2].SetActive(true);
                myJobIcons[2].SetActive(true);
            }
            else if (jobGroup == Job.Archer)
            {
                myJobOutlines[3].SetActive(true);
                myJobIcons[3].SetActive(true);
            }
        }

        public void SetDuelHistory(IEnumerable<CharDuelHistory> duelHistories)
        {
            showingHistoryList.Clear();
            showingHistoryList.AddRange(duelHistories);
            historyScroll.Resize(showingHistoryList.Count);
            noHistoryNotice.gameObject.SetActive(showingHistoryList.Count == 0);
        }

        public void SetActiveCompletion(bool value)
        {
            completePanelRoot.SetActive(value);
            alphabetPanelRoot.SetActive(!value);
        }

        private void OnHistoryScrollRefresh(GameObject slot, int index)
        {
            slot.GetComponent<UIDuelHistorySlot>().SetData(showingHistoryList[index]);
        }

        /// <summary>
        /// 동료 알림 업데이트
        /// </summary>
        private void UpdateAgentNotice()
        {
            bool isNotice = presenter.CanEquipAgent();
            btnAgent.SetNotice(isNotice);
        }

        private void UpdateZeny(long totalZeny)
        {
            titleView.ShowZeny(totalZeny);
        }

        private void UpdateCatCoin(long totalCatCoin)
        {
            titleView.ShowCatCoin(totalCatCoin);
        }

        private void UpdateOpenContent()
        {
            // 컨텐츠 오픈 체크
            bool isOpenContent = presenter.IsOpenContent();
            duelDefault.SetActive(isOpenContent);
            duelLock.SetActive(!isOpenContent);
            //듀얼 아레나 오픈 여부 체크
            btnArenaDuel.SetActive(presenter.ArenaIsAvalable());
        }

        private void UpdateWaitingTime()
        {
            var remainTime = presenter.GetDuelListWaitingTime(State.Chapter);

            if (remainTime.ToRemainTime() > 0)
            {
                Timing.RunCoroutineSingleton(YieldDuelListWaitingTime(remainTime).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                lockGridObject.SetActive(false);
            }
        }

        private IEnumerator<float> YieldDuelListWaitingTime(RemainTime remainTime)
        {
            lockGridObject.SetActive(true);

            while (remainTime.ToRemainTime() > 0f)
            {
                var secs = TimeSpan.FromMilliseconds(remainTime.ToRemainTime());

                labelWaitingTime.text = secs.TotalSeconds.ToIntValue().ToString();
                yield return Timing.WaitForSeconds(1f);
            }

            lockGridObject.SetActive(false);
        }

        private void UpdateWaitingEventTime()
        {
            var remainTime = presenter.GetDuelListWaitingTime(State.Event);

            if (remainTime.ToRemainTime() > 0)
            {
                Timing.RunCoroutineSingleton(YieldEventDuelListWaitingTime(remainTime).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                lockEventGridObject.SetActive(false);
            }
        }

        private IEnumerator<float> YieldEventDuelListWaitingTime(RemainTime remainTime)
        {
            lockEventGridObject.SetActive(true);

            while (remainTime.ToRemainTime() > 0f)
            {
                var secs = TimeSpan.FromMilliseconds(remainTime.ToRemainTime());

                labelEventWaitingTime.text = secs.TotalSeconds.ToIntValue().ToString();
                yield return Timing.WaitForSeconds(1f);
            }

            lockEventGridObject.SetActive(false);
        }

        private void UpdateWaitingArenaTime()
        {
            var remainTime = presenter.GetDuelListWaitingTime(State.Arena);

            if (remainTime.ToRemainTime() > 0)
            {
                Timing.RunCoroutineSingleton(YieldArenaDuelListWaitingTime(remainTime).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                lockArenaGridObject.SetActive(false);
            }
        }

        private IEnumerator<float> YieldArenaDuelListWaitingTime(RemainTime remainTime)
        {
            lockArenaGridObject.SetActive(true);

            while (remainTime.ToRemainTime() > 0f)
            {
                var secs = TimeSpan.FromMilliseconds(remainTime.ToRemainTime());

                labelArenaWaitingTime.text = secs.TotalSeconds.ToIntValue().ToString();
                yield return Timing.WaitForSeconds(1f);
            }

            lockArenaGridObject.SetActive(false);
        }

        private void OnClickedBtnAgent()
        {
            presenter.ViewEventHandler(EventType.OnClickAgent, null);
        }

        private void OnClickChapter(int chapter)
        {
            presenter.ViewEventHandler(EventType.OnClickChapter, chapter);
        }

        private void OnClickGetReward()
        {
            presenter.ViewEventHandler(EventType.OnClickRecieveReward, null);
        }

        private void OnClickAlphabet(int index)
        {
            presenter.ViewEventHandler(EventType.OnClickAlphabet, index);
        }

        private void OnClickClose()
        {
            UI.Close<UIDuel>();
        }

        private string GetHexCode(Job job)
        {
            if (job == Job.Swordman)
                return "ee4646";

            if (job == Job.Magician)
                return "6287ff";

            if (job == Job.Thief)
                return "d076ff";

            if (job == Job.Archer)
                return "29bb4b";

            return "ffffff";
        }

        private void UpdateCharacterInfo()
        {
            duelEventView.SetCharacterData(presenter.GetJob(), presenter.GetGender(), presenter.GetProfileName(), presenter.GetName(), presenter.GetHexCid());
        }

        private void RefreshEventDuel()
        {
            if (!duelEventView.IsShow)
                return;

            presenter.ViewEventHandler(EventType.OnClickEvent, null);
        }

        private void RefreshArenaDuel()
        {
            if (!duelArenaView.IsShow)
                return;

            bool isShowIndicator = false;
            presenter.ViewEventHandler(EventType.OnClickArena, isShowIndicator);
        }

        void ShowEventDuelRewardUI()
        {
            UI.Show<UIEventDuelReward>();
        }

        void ShowEventDuelRankingUI()
        {
            UI.Show<UIEventDuelRanking>();
        }

        void ShowArenaDuelRankingUI()
        {
            UI.Show<UIDuelArenaRank>();
        }
    }
}