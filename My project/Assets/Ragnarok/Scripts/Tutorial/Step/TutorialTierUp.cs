using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialTierUp : TutorialStep
    {
        public TutorialTierUp() : base(TutorialType.TierUp)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.TierUp))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Make, false))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.TierUp, false))
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
            UI.Show<UIStoryBook>().SetChapter(6);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            UIContentsUnlock contentsUnlock = UI.Show<UIContentsUnlock>();
            contentsUnlock.Set(ContentType.TierUp);

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            ShowTutorial();
            HideBlind();

            yield return WaitUntilHideCanvas<UIContentsLauncher>();

            ShowBlind();

            yield return Show(LocalizeKey._26082.ToText(), UIWidget.Pivot.Bottom);

            yield return Show(LocalizeKey._26083.ToText(), UIWidget.Pivot.Bottom);

            yield return WaitUntilShowCanvas<UIMain>();

            UIMain mainUI = UI.GetUI<UIMain>();

            yield return Show(LocalizeKey._26084.ToText(), UIWidget.Pivot.Top, mainUI.MakeBtn, IsVisibleCanvas<UIMake>);

            UIMake makeUI = UI.GetUI<UIMake>();
            makeUI.SetMode(UIMake.Mode.Hextech);
            makeUI.SetHextechTab(UIHextechView.Mode.TierUp);
            makeUI.ForceHextechContentsOpen(false);

            SetNPC(Npc.PRUIT);
            yield return Show(LocalizeKey._26085.ToText(), UIWidget.Pivot.Top);

            yield return Show(LocalizeKey._26086.ToText(), UIWidget.Pivot.Top);

            ShowEmtpy();

            makeUI.StartTierUpContentsOpenEffect();
            yield return Timing.WaitUntilTrue(makeUI.IsContentsOpenEffectFinished);

            yield return Show(LocalizeKey._26087.ToText(), UIWidget.Pivot.Top);

            yield return Show(LocalizeKey._26088.ToText(), UIWidget.Pivot.Top);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}