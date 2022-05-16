using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialPronteraScenario : TutorialStep
    {
        public TutorialPronteraScenario() : base(TutorialType.PronteraScenario)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.PronteraScenario, false))
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
            UI.Show<UIStoryBook>().SetChapter(2);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();
            
            yield return Show(LocalizeKey._26058.ToText(), UIWidget.Pivot.Bottom);

            yield return Show(LocalizeKey._26059.ToText(), UIWidget.Pivot.Bottom);
            
            yield return Show(LocalizeKey._26060.ToText(), UIWidget.Pivot.Bottom);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}