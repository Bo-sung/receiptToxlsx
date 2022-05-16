using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialMazeEnter : TutorialStep
    {
        public interface ISelectImpl
        {
            UIWidget GetBtnMazeWidget();
        }

        public interface IImpl
        {
            void SetTutorialMode(bool isTutorialMode);
            UIWidget GetFirstMazeWidget();
            bool IsSelectFirstMazeEnter();

            UIWidget GetTimePatrolWidget();
            UIWidget GetGate1Widget();
        }

        public TutorialMazeEnter() : base(TutorialType.MazeEnter)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Maze, false))
                return false;

            if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.Stage)
                return false;

            // UIBattleMenu 가 음슴
            if (IsInvisibleCanvas<UIBattleMenu>())
                return false;

            // MazeOpen 튜토리얼 후에만 가능
            if (!Tutorial.HasAlreadyFinished(TutorialType.MazeOpen))
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

            ShowTutorial();

            ISelectImpl selectImpl = UI.GetUI<UIBattleMenu>();

            // [62AEE4][C]미궁섬[/c][-]을 터치해줘.
            yield return Show(LocalizeKey._26148.ToText(), UIWidget.Pivot.Bottom, selectImpl.GetBtnMazeWidget(), IsVisibleCanvas<UIAdventureMazeSelect>);

            IImpl impl = UI.GetUI<UIAdventureMazeSelect>();

            impl.SetTutorialMode(true);

            yield return Show(impl.GetFirstMazeWidget(), impl.IsSelectFirstMazeEnter);

            impl.SetTutorialMode(false);

            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}