using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialMazeOpen : TutorialStep
    {
        public TutorialMazeOpen() : base(TutorialType.MazeOpen)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.MazeOpen))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Maze, false))
                return false;

            if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.Stage)
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

            yield return Show(LocalizeKey._26056.ToText(), UIWidget.Pivot.Bottom);

            yield return Show(LocalizeKey._26057.ToText(), UIWidget.Pivot.Bottom);

            UIContentsUnlock contentsUnlock = UI.Show<UIContentsUnlock>();
            contentsUnlock.Set(ContentType.Maze);

            HideTutorial();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            ShowTutorial();
            HideBlind();

            yield return WaitUntilHideCanvas<UIContentsLauncher>();

            ShowBlind();
            HideTutorial();
        }
    }
}