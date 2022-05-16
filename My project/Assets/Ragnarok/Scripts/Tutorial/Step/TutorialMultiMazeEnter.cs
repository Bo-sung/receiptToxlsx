using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialMultiMazeEnter : TutorialStep
    {
        public TutorialMultiMazeEnter() : base(TutorialType.MultiMazeEnter)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Maze, false))
                return false;

            if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.MultiMazeLobby)
                return false;

            // 메인퀘스트가 멀티미로 도전 퀘스트일 경우에만 가능 (서비스하는 지역의 경우에는 튜토리얼을 다시 진행하는 것을 막기 위함)
            if (!Entity.player.Quest.IsCurrentQuest(QuestType.MULTI_MAZE_ENTER_COUNT))
                return false;

            return true;
        }

        protected override bool HasSkip()
        {
            return false;
        }

        protected override Npc GetNpc()
        {
            return Npc.HOLORUCHI;
        }

        protected override IEnumerator<float> Run()
        {
            HideTutorial();

            yield return WaitUntilHideDailyCheck();
            yield return WaitUntilHideCanvas<UIQuestReward>();
            yield return WaitUntilHideCanvas<UICharacterShareWaiting>();

            ShowTutorial();

            // 프론테라 미궁이 아직 불안정한 것 같군..\n이번에는 다같이 도전 해보자구!
            yield return Show(LocalizeKey._26152.ToText(), UIWidget.Pivot.Bottom);

            HideBlind();

            MultiMazeLobbyEntry multiMazeLobbyEntry = BattleManager.Instance.GetCurrentEntry() as MultiMazeLobbyEntry;
            multiMazeLobbyEntry.GoToCurrentNpc(1);

            yield return Timing.WaitUntilTrue(multiMazeLobbyEntry.IsMoveFinishedToNpc);

            ShowBlind();

            // NPC 근처로 가면 입장 버튼을 볼 수 있는 거 기억하지?\n쿡쿡쿡. 입장 버튼을 눌러줘.
            yield return Show(LocalizeKey._26153.ToText(), UIWidget.Pivot.Bottom, multiMazeLobbyEntry.GetMazeLobbyFieldWidget(), IsVisibleCanvas<UIMazeSelect>);

            UIMazeSelect mazeSelect = UI.GetUI<UIMazeSelect>();

            // 이번 도전은 혼자서는 처치할 수 없을 거 같아!\n다같이 보스를 처치하자구! 쿡쿡쿡
            yield return Show(LocalizeKey._26154.ToText(), UIWidget.Pivot.Bottom, mazeSelect.GetBtnMultiWidget(), mazeSelect.IsSelectMultiMaze);

            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}