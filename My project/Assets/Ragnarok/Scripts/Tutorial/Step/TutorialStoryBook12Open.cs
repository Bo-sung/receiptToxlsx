using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialStoryBook12Open : TutorialStep
    {
        public TutorialStoryBook12Open() : base(TutorialType.StoryBook12Open)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.StoryBook12Open))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.StoryBook12Open, false))
                return false;

            var mode = BattleManager.Instance.GetCurrentEntry().mode;
            if (mode != BattleMode.MultiMazeLobby && mode != BattleMode.Stage)
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

            // Step1: 스토리북 오픈
            UI.Show<UIStoryBook>().SetChapter(12);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();

            // 
            yield return Show(LocalizeKey._26105.ToText(), UIWidget.Pivot.Bottom);

            // 
            yield return Show(LocalizeKey._26106.ToText(), UIWidget.Pivot.Bottom);

            // 
            yield return Show(LocalizeKey._26107.ToText(), UIWidget.Pivot.Bottom);

            // 
            yield return Show(LocalizeKey._26108.ToText(), UIWidget.Pivot.Bottom);

            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}