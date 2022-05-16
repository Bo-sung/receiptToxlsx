using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialSingleMazeEnter : TutorialStep
    {
        public TutorialSingleMazeEnter() : base(TutorialType.SingleMazeEnter)
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

            // 메인퀘스트가 시나리오미로 클리어 퀘스트일 경우에만 가능 (서비스하는 지역의 경우에는 튜토리얼을 다시 진행하는 것을 막기 위함)
            if (!Entity.player.Quest.IsCurrentQuest(QuestType.SCENARIO_MAZE_ID_CLEAR_COUNT))
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

            UITutorialGuide tutorialGuide = UI.Show<UITutorialGuide>();
            tutorialGuide.SetContentType(ContentType.AchieveMultiMaze);

            yield return WaitUntilHideCanvas<UITutorialGuide>();

            ShowTutorial();
            HideBlind();

            yield return WaitUntilHideCanvas<UIContentsLauncher>();

            ShowBlind();

            // 프론테라 미궁에서 강력한 진동이 느껴지는군…쿡쿡쿡\n어서 프론테라 미궁으로 가보자구!!
            yield return Show(LocalizeKey._26149.ToText(), UIWidget.Pivot.Bottom);

            HideBlind();

            MultiMazeLobbyEntry multiMazeLobbyEntry = BattleManager.Instance.GetCurrentEntry() as MultiMazeLobbyEntry;
            multiMazeLobbyEntry.GoToCurrentNpc(1);

            yield return Timing.WaitUntilTrue(multiMazeLobbyEntry.IsMoveFinishedToNpc);

            ShowBlind();

            // NPC 옆으로 가면 입장 버튼을 볼 수 있지!\n쿡쿡쿡. 입장 버튼을 눌러줘.
            yield return Show(LocalizeKey._26150.ToText(), UIWidget.Pivot.Bottom, multiMazeLobbyEntry.GetMazeLobbyFieldWidget(), IsVisibleCanvas<UIMazeSelect>);

            UIMazeSelect mazeSelect = UI.GetUI<UIMazeSelect>();

            // 좋아! 강력했던 진동이 더욱 가까이 느껴지고 있어.\n빨리 가보자구! 쿡쿡쿡..
            yield return Show(LocalizeKey._26151.ToText(), UIWidget.Pivot.Bottom, mazeSelect.GetBtnSingleWidget(), mazeSelect.IsSelectSingleMaze);

            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}