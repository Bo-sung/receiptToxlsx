using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialManageCard : TutorialStep
    {
        public TutorialManageCard() : base(TutorialType.ManageCard)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.ManageCard))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Make, false))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.ManageCard, false))
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
            UI.Show<UIStoryBook>().SetChapter(7);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();
            
            yield return Show(LocalizeKey._26089.ToText(), UIWidget.Pivot.Bottom);

            yield return Show(LocalizeKey._26090.ToText(), UIWidget.Pivot.Bottom);
            
            UIContentsUnlock contentsUnlock = UI.Show<UIContentsUnlock>();
            contentsUnlock.Set(ContentType.ManageCard);

            HideTutorial();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            ShowTutorial();
            HideBlind();

            yield return WaitUntilHideCanvas<UIContentsLauncher>();

            ShowBlind();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}