using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialStoryBook13Open : TutorialStep
    {
        public TutorialStoryBook13Open() : base(TutorialType.StoryBook13Open)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.StoryBook13Open))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.StoryBook13Open, false))
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
            UI.Show<UIStoryBook>().SetChapter(13);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();

            // 
            yield return Show(LocalizeKey._26109.ToText(), UIWidget.Pivot.Bottom);

            // 
            yield return Show(LocalizeKey._26110.ToText(), UIWidget.Pivot.Bottom);

            // 
            yield return Show(LocalizeKey._26111.ToText(), UIWidget.Pivot.Bottom);

            // 
            yield return Show(LocalizeKey._26112.ToText(), UIWidget.Pivot.Bottom);

            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}