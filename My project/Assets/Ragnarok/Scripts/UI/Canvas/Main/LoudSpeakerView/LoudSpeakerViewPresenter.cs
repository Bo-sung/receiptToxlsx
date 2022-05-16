using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UILoudSpeakerView"/>
    /// </summary>
    public class LoudSpeakerViewPresenter : ViewPresenter
    {
        private readonly string TAG = nameof(LoudSpeakerViewPresenter);

        public interface IView
        {
            void SetMode(UILoudSpeakerView.Mode mode);
            void Show(int stage, string nickname, string message, int cid, int uid, float remainTime);
            void Hide();
        }

        /******************** Models ********************/
        private readonly ChatModel chatModel;
        private readonly LoudSpeakerMessageQueue queue;
        private readonly UserModel userModel;
        private readonly BattleManager battleManager;
        private readonly DungeonModel dungeonModel;
        private readonly MultiMazeDataManager multiMazeDataRepo;
        private readonly QuestModel questModel;

        private readonly IView view;
        private bool isPlaying;

        public LoudSpeakerViewPresenter(IView view)
        {
            this.view = view;

            isPlaying = false;

            chatModel = Entity.player.ChatModel;
            queue = chatModel.LoudSpeakerMessageManager;
            userModel = Entity.player.User;
            battleManager = BattleManager.Instance;
            dungeonModel = Entity.player.Dungeon;
            multiMazeDataRepo = MultiMazeDataManager.Instance;
            questModel = Entity.player.Quest;
        }

        public override void AddEvent()
        {
            chatModel.OnResponseLoudSpeaker += OnResponseLoudSpeaker;
        }

        public override void RemoveEvent()
        {
            chatModel.OnResponseLoudSpeaker -= OnResponseLoudSpeaker;
        }

        public void Begin()
        {
            if (isPlaying)
                return;

            if (queue.Count == 0)
                return;

            PlayCurrentQueue();
        }

        public void Stop()
        {
            isPlaying = false;
            Timing.KillCoroutines(TAG);
        }

        /// <summary>
        /// 귓속말 상대 추가하고 귓속말챗 열기
        /// </summary>
        public void OpenWhipserUI()
        {
            if (queue.Count == 0)
            {
#if UNITY_EDITOR
                Debug.LogError("[OpenWhipserUI] 확성기 메시지가 없다.");
#endif
                return;
            }

            var curInfo = queue.Peek();
            chatModel.SetWhisperInfo(new WhisperInfo(curInfo.CID, curInfo.UID, curInfo.Nickname));

            UI.ShortCut<UIChat>()
                .Show(ChatMode.Whisper, whisperCid: curInfo.CID);
        }

        /// <summary>
        /// 메시지 하나 스킵
        /// </summary>
        public void SkipMessage()
        {
            queue.Skip();
        }

        public void ShowOtherCharacterInfo()
        {
            if (queue.Count == 0)
            {
#if UNITY_EDITOR
                Debug.LogError("[ShowOtherCharacterInfo] 확성기 메시지가 없다.");
#endif
                return;
            }

            var curInfo = queue.Peek();
            userModel.RequestOtherCharacterInfo(curInfo.UID, curInfo.CID).WrapNetworkErrors();
        }

        private void OnResponseLoudSpeaker(ChatModel.ISimpleChatInput input)
        {
            if (isPlaying)
                return;

            PlayCurrentQueue();
        }

        /// <summary>
        /// 현재 확성기 메시지를 재생
        /// </summary>
        private void PlayCurrentQueue()
        {
            if (queue.Count == 0)
            {
#if UNITY_EDITOR
                Debug.LogError("[PlayCurrentQueue] 확성기 메시지가 없다.");
#endif
                return;
            }

            isPlaying = true;

            queue.Play();

            ChatModel.ISimpleChatInput input = queue.Peek();

            UILoudSpeakerView.Mode mode;
            int stage;
            if (input.IsGMMsg)
            {
                mode = UILoudSpeakerView.Mode.GM;
                stage = 0;
            }
            else if (input is MultiMatchChatInfo multiMatchChatInfo)
            {
                mode = UILoudSpeakerView.Mode.Maze;
                stage = multiMatchChatInfo.GetStage();
            }
            else
            {
                mode = UILoudSpeakerView.Mode.Default;
                stage = 0;
            }
            view.SetMode(mode);
            view.Show(stage, input.Nickname, input.Message, input.CID, input.UID, queue.GetRemainTime());
            Timing.RunCoroutine(YieldWaitForDone(), TAG);
        }

        private IEnumerator<float> YieldWaitForDone()
        {
            yield return Timing.WaitUntilTrue(queue.IsDone); // 재생중인 메시지 끝날 때까지 대기

            isPlaying = false;

            if (!queue.Next())
            {
                view.Hide();
                yield break;
            }

            PlayCurrentQueue(); // 큐가 남아있다면 다음 메시지 재생
        }

        public void EnterMultiMaze(int stage)
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            if (!IsOpenDungeon(stage))
                return;

            StartMultiMaze(stage);
        }

        private bool IsOpenDungeon(int stage)
        {
            BattleMode curBattleMode = battleManager.Mode;

            // 스테이지, 로비, 타임패트롤 에서만 이동 가능
            if (curBattleMode != BattleMode.Stage && curBattleMode != BattleMode.Lobby && curBattleMode != BattleMode.TimePatrol)
            {
                string message = LocalizeKey._46400.ToText(); // 현재 위치에서 이동이 불가능합니다.
                UI.ShowToastPopup(message);
                return false;
            }

            if (!IsOpenScenarioId(stage))
            {
                string message = LocalizeKey._90234.ToText(); // 조건이 맞지 않아 해당 던전을 입장할 수 없습니다.
                UI.ShowToastPopup(message);
                return false;
            }

            return true;
        }

        private bool IsOpenScenarioId(int stage)
        {
            // 미로 컨텐츠 오픈하지 않음
            if (!questModel.IsOpenContent(ContentType.Maze, false))
                return false;

            MultiMazeData data = multiMazeDataRepo.Get(stage);
            if (data == null)
                return false;

            // 오픈 조건에 해당하는 시나리오 id가 없을 때 && 이벤트 던전의 경우 => 무조건 오픈
            int openScenarioId = data.open_scenario_id;
            if (openScenarioId == 0 && data.IsEvent())
                return questModel.IsOpenContent(ContentType.Maze, false);

            return dungeonModel.IsCleardScenarioMazeId(openScenarioId);
        }

        private async void StartMultiMaze(int stage)
        {
            string message = LocalizeKey._90235.ToText(); // 미궁섬으로 이동하시겠습니까?
            if (!await UI.SelectPopup(message))
                return;

            battleManager.StartBattle(BattleMode.MultiMazeLobby, stage);
        }
    }
}