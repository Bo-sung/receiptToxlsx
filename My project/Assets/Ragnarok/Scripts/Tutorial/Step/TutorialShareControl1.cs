using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialShareControl1 : TutorialStep
    {
        public TutorialShareControl1() : base(TutorialType.ShareControl1)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.ShareControl1))
                return false;
            
            if (!Entity.player.Quest.IsOpenContent(ContentType.ShareControl, false))
                return false;
            
            if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.MultiMazeLobby)
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
            UI.Show<UIStoryBook>().SetChapter(3);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();
            
            yield return Show(LocalizeKey._26061.ToText(), UIWidget.Pivot.Bottom);

            yield return Show(LocalizeKey._26062.ToText(), UIWidget.Pivot.Bottom);
            
            UIContentsUnlock contentsUnlock = UI.Show<UIContentsUnlock>();
            contentsUnlock.Set(ContentType.ShareControl);

            HideTutorial();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            ShowTutorial();
            HideBlind();

            yield return WaitUntilHideCanvas<UIContentsLauncher>();

            ShowBlind();
            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}