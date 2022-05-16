using MEC;
using Ragnarok.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIDuelPlayerList : UICanvas
    {
        public class Input : IUIData
        {
            public ChapterDuelState duelState;
            public int alphabetIndex;
        }

        public class EventInput : IUIData
        {
            public DuelServerPacket packet;
        }

        public class ArenaInput : IUIData
        {
            public int arenaPoint;
            public int winCount;
            public int loseCount;
        }

        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        public enum EventType
        {
            OnClickBattle,
            OnClickCharge,
            OnClickRefresh,
            OnClickAgent
        }

        [SerializeField] GameObject prefab;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UITextureHelper jobProfile;
        [SerializeField] UITextureHelper jobProfileDeco;
        [SerializeField] UITextureHelper alphabetJobDeco;
        [SerializeField] UIDuelAlphabetCollection alphabetTemplates;
        [SerializeField] GameObject alphabetRoot;
        [SerializeField] UILabelHelper nameLabel;
        [SerializeField] UILabelHelper battleScoreLabel;
        [SerializeField] UILabelHelper recordLabel;
        [SerializeField] UILabelHelper ticketLabel;
        [SerializeField] UIButtonHelper chargeButton;
        [SerializeField] UIButtonHelper refreshButton;
        [SerializeField] UILabelHelper noPlayerListNotice;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnAgent;
        [SerializeField] UIDuelCombatAngetProfileSlot[] combatAgentSlots;
        [SerializeField] UIButton closeButton;
        [SerializeField] UIDuelAlphabetCollection alphabetCube;
        [SerializeField] GameObject goDuelInfo;
        [SerializeField] UIDuelPlayerArenaInfo arenaInfo;

        private DuelPlayerListPresenter presenter;
        private bool isStartBattle;

        private UIDuel.State state;
        private UIDuelPlayerListSlot.IInput[] infos;
        private UIDuelAlphabet curAlphabet;

        protected override void OnInit()
        {
            presenter = new DuelPlayerListPresenter(this);

            presenter.OnUpdateNewAgent += UpdateAgentNotice;
            presenter.OnUpdateEquippedAgent += UpdateAgentNotice;
            presenter.OnResetWaitingTime += UpdateWaitingTime;
            presenter.AddEvent();
            EventDelegate.Add(chargeButton.OnClick, OnClickChargeButton);
            EventDelegate.Add(refreshButton.OnClick, OnClickRefreshButton);
            EventDelegate.Add(btnAgent.OnClick, OnClickedBtnAgent);
            EventDelegate.Add(closeButton.onClick, CloseUI);

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
        }

        protected override void OnClose()
        {
            presenter.OnUpdateNewAgent -= UpdateAgentNotice;
            presenter.OnUpdateEquippedAgent -= UpdateAgentNotice;
            presenter.OnResetWaitingTime -= UpdateWaitingTime;
            presenter.RemoveEvent();
            EventDelegate.Remove(chargeButton.OnClick, OnClickChargeButton);
            EventDelegate.Remove(refreshButton.OnClick, OnClickRefreshButton);
            EventDelegate.Remove(btnAgent.OnClick, OnClickedBtnAgent);
            EventDelegate.Remove(closeButton.onClick, CloseUI);
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

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
            {
                UI.Close<UIDuelPlayerList>();
            }
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data == null)
            {
                state = default;
                return;
            }

            wrapper.Resize(0);
            noPlayerListNotice.gameObject.SetActive(false);
            UpdateAgentNotice();
            UpdateWaitingTime();

            if (data is Input input)
            {
                state = UIDuel.State.Chapter;
                presenter.OnShowChapter(input.duelState, input.alphabetIndex);
            }
            else if (data is EventInput eventInput)
            {
                state = UIDuel.State.Event;
                presenter.OnShowEvent(eventInput.packet.id);
            }
            else if (data is ArenaInput arenaInput)
            {
                state = UIDuel.State.Arena;
                presenter.OnShowArena(arenaInput.arenaPoint);
                SetPlayerInfo(arenaInput.winCount, arenaInput.loseCount);
            }
            else
            {
                state = default;
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._47903; // 듀얼
            chargeButton.LocalKey = LocalizeKey._47904; // 충전
            refreshButton.LocalKey = LocalizeKey._47900; // 목록 변경
            noPlayerListNotice.LocalKey = LocalizeKey._47902; // 목록 변경
            btnAgent.LocalKey = LocalizeKey._47032; // 동료 장착
        }

        public void SetAlphabet(Job job, int color, char alphabet)
        {
            NGUITools.SetActive(goDuelInfo, true);
            arenaInfo.Hide();

            NGUITools.SetActive(alphabetJobDeco.cachedGameObject, true);
            DestroyChapterAlphabet();
            alphabetCube.Hide();

            alphabetJobDeco.Set(job.GetJobIcon());

            curAlphabet = Instantiate(alphabetTemplates.GetTemplate(color));
            curAlphabet.transform.parent = alphabetRoot.transform;
            curAlphabet.transform.localPosition = Vector3.zero;
            curAlphabet.transform.localScale = Vector3.one;
            curAlphabet.gameObject.SetActive(true);
            curAlphabet.SetData(0, alphabet, null);
        }

        public void UseAlphabet(int serverId, int alphabetIndex)
        {
            NGUITools.SetActive(goDuelInfo, true);
            arenaInfo.Hide();

            NGUITools.SetActive(alphabetJobDeco.cachedGameObject, false);
            DestroyChapterAlphabet();
            alphabetCube.Show();

            alphabetCube.Use(serverId, alphabetIndex);
        }

        public void SetArena(int arenaPoint)
        {
            NGUITools.SetActive(goDuelInfo, false);
            arenaInfo.Show();
            arenaInfo.SetData(presenter.GetArenaNameId(arenaPoint), arenaPoint);
        }

        private void DestroyChapterAlphabet()
        {
            if (curAlphabet == null)
                return;

            Destroy(curAlphabet.gameObject);
            curAlphabet = null;
        }

        public void SetPlayerInfo(int winCount, int defeatCount)
        {
            Job job = Entity.player.Character.Job;
            Gender gender = Entity.player.Character.Gender;
            string name = Entity.player.Character.Name;
            jobProfile.Set(Entity.player.GetProfileName());
            jobProfileDeco.Set(job.GetJobIcon());
            battleScoreLabel.Text = LocalizeKey._10204.ToText()
                .Replace(ReplaceKey.VALUE, Entity.player.GetTotalAttackPower().ToString("n0"));

            switch (state)
            {
                case UIDuel.State.Chapter:
                    nameLabel.Text = $"Lv.{Entity.player.Character.JobLevel} {name} [c][BEBEBE]({Entity.player.Character.CidHex})[-][/c]";
                    recordLabel.Text = LocalizeKey._47812.ToText() // {WIN}승 / {LOSE}패
                        .Replace(ReplaceKey.WIN, winCount)
                        .Replace(ReplaceKey.LOSE, defeatCount);
                    break;

                case UIDuel.State.Event:
                    nameLabel.Text = $"Lv.{Entity.player.Character.JobLevel} {name} [c][BEBEBE]({Entity.player.Character.CidHex})[-][/c]";
                    recordLabel.Text = string.Empty;
                    break;

                case UIDuel.State.Arena:
                    nameLabel.Text = $"Lv.{Entity.player.Character.JobLevel} {name} [c][BEBEBE]({Entity.player.Character.CidHex})[-][/c]";
                    recordLabel.Text = LocalizeKey._47812.ToText() // {WIN}승 / {LOSE}패
                        .Replace(ReplaceKey.WIN, winCount)
                        .Replace(ReplaceKey.LOSE, defeatCount);
                    break;

                default:
                    throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {state}");
            }
        }

        public void SetDualPoint(int count)
        {
            int useCount = BasisType.ENTER_DUEL_POINT.GetInt();
            ticketLabel.Text = string.Concat(count / useCount, " / ", BasisType.DUEL_POINT_DROP_MAX.GetInt() / useCount);
        }

        public void ShowPlayers(UIDuelPlayerListSlot.IInput[] infos)
        {
            this.infos = infos;
            wrapper.Resize(infos.Length);
            noPlayerListNotice.gameObject.SetActive(infos.Length == 0);
        }

        public void SetCombatAgents(CombatAgent[] combatAgents, int validSlotCount)
        {
            for (int i = 0; i < combatAgentSlots.Length; ++i)
            {
                combatAgentSlots[i].SetData(i < combatAgents.Length ? combatAgents[i] : null, i < validSlotCount);
            }
        }

        /// <summary>
        /// 동료 알림 업데이트
        /// </summary>
        private void UpdateAgentNotice()
        {
            bool isNotice = presenter.CanEquipAgent();
            btnAgent.SetNotice(isNotice);
        }

        private void UpdateWaitingTime()
        {
            var remainTime = presenter.GetDuelListWaitingTime();

            if (remainTime.ToRemainTime() > 0)
            {
                Timing.RunCoroutineSingleton(YieldDuelListWaitingTime(remainTime).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                refreshButton.IsEnabled = true;
                refreshButton.LocalKey = LocalizeKey._47900; // 목록 변경
            }
        }

        private IEnumerator<float> YieldDuelListWaitingTime(RemainTime remainTime)
        {
            refreshButton.IsEnabled = false;

            while (remainTime.ToRemainTime() > 0f)
            {
                var secs = TimeSpan.FromMilliseconds(remainTime.ToRemainTime());

                refreshButton.Text = LocalizeKey._47926.ToText().Replace(ReplaceKey.TIME, secs.TotalSeconds.ToIntValue()); // 쿨타임({TIME}s)
                yield return Timing.WaitForSeconds(1f);
            }

            refreshButton.IsEnabled = true;
            refreshButton.LocalKey = LocalizeKey._47900; // 목록 변경
        }

        private void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<UIDuelPlayerListSlot>();
            slot.SetData(infos[index], OnClickBattle);
        }

        private void OnClickedBtnAgent()
        {
            presenter.ViewEventHandler(EventType.OnClickAgent, null);
        }

        private void OnClickBattle(UIDuelPlayerListSlot.IInput info)
        {
            isStartBattle = true;
            presenter.ViewEventHandler(EventType.OnClickBattle, info);
        }

        private void OnClickChargeButton()
        {
            presenter.ViewEventHandler(EventType.OnClickCharge, null);
        }

        private void OnClickRefreshButton()
        {
            presenter.ViewEventHandler(EventType.OnClickRefresh, null);
        }

        public void CloseUI()
        {
            UI.Close<UIDuelPlayerList>();
            UI.Show<UIDuel>(new UIDuel.Input() { needRequestDuelInfo = false, duelState = state, });
        }

        protected override void OnBack()
        {
            CloseUI();
        }
    }
}