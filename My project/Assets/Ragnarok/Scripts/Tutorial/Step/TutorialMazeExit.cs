using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialMazeExit : TutorialStep
    {
        public interface IImpl
        {
            UIWidget GetBtnCompleteWidget();
            UIWidget GetBtnConfirmWidget();
            bool IsSelectMultiMazeQuestReward();
        }

        public interface IExitImpl
        {
            UIWidget GetBtnExitWidget();
        }

        public TutorialMazeExit() : base(TutorialType.MazeExit)
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

            // UIBattleMenu 가 음슴
            if (IsInvisibleCanvas<UIBattleMenu>())
                return false;

            // MultiMazeEnter 튜토리얼 후에만 가능
            if (!Tutorial.HasAlreadyFinished(TutorialType.MultiMazeEnter))
                return false;

            // 메인퀘스트가 멀티미로 도전 퀘스트일 경우에만 가능 (서비스하는 지역의 경우에는 튜토리얼을 다시 진행하는 것을 막기 위함)
            if (!Entity.player.Quest.IsStandByRewardQuest(QuestType.MULTI_MAZE_ENTER_COUNT))
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
            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄

            HideTutorial();

            yield return WaitUntilHideDailyCheck();
            yield return WaitUntilHideCanvas<UIQuestReward>();
            yield return WaitUntilHideCanvas<UICharacterShareWaiting>();

            ShowTutorial();

            UISpecialEvent.selectedType = SpecialEventType.MultiMazeQuestView;
            UISpecialEvent uiSpecialEvent = UI.Show<UISpecialEvent>();
            IImpl impl = uiSpecialEvent;
            UIWidget widget = impl.GetBtnCompleteWidget();

            if (widget)
            {
                // 짜잔~ 갑자기 나와서 놀랐지?쿡쿡쿡…\n도전한 자에게는 보상이 있나니~!
                yield return Show(LocalizeKey._26159.ToText(), UIWidget.Pivot.Bottom, impl.GetBtnCompleteWidget(), impl.IsSelectMultiMazeQuestReward);

                // 미궁을 클리어하면\n추가 보상을 획득할 수 있으니\n꼭 받으라구~쿡쿡쿡.
                yield return Show(LocalizeKey._26160.ToText(), UIWidget.Pivot.Center, impl.GetBtnConfirmWidget(), IsInvisibleCanvas<UISpecialEvent>);
            }
            else
            {
                UI.Close<UISpecialEvent>();
            }

            // 드디어 진동이 멈췄군…\n미궁에 대해서 꾸준히 조사할 필요가 있겠어!
            yield return Show(LocalizeKey._26155.ToText(), UIWidget.Pivot.Bottom);

            IExitImpl exitImpl = UI.GetUI<UIBattleMenu>();

            // 이제 돌아가자구~.\n나가기를 터치해줘.
            yield return Show(LocalizeKey._26156.ToText(), UIWidget.Pivot.Top, exitImpl.GetBtnExitWidget());

            HideTutorial();
        }
    }
}