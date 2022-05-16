using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialChangeElement : TutorialStep
    {
        public TutorialChangeElement() : base(TutorialType.ChangeElement)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.ChangeElement))
                return false;
            
            if (!Entity.player.Quest.IsOpenContent(ContentType.Make, false))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.ChangeElement, false))
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
            UI.Show<UIStoryBook>().SetChapter(5);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            UIContentsUnlock contentsUnlock = UI.Show<UIContentsUnlock>();
            contentsUnlock.Set(ContentType.ChangeElement);

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            ShowTutorial();
            HideBlind();

            yield return WaitUntilHideCanvas<UIContentsLauncher>();

            ShowBlind();

            yield return Show(LocalizeKey._26075.ToText(), UIWidget.Pivot.Bottom);

            yield return WaitUntilShowCanvas<UIMain>();

            UIMain mainUI = UI.GetUI<UIMain>();

            yield return Show(LocalizeKey._26076.ToText(), UIWidget.Pivot.Top, mainUI.MakeBtn, IsVisibleCanvas<UIMake>);

            UIMake makeUI = UI.GetUI<UIMake>();

            yield return Show(LocalizeKey._26077.ToText(), UIWidget.Pivot.Center, makeUI.HextechBtn, makeUI.IsHextechShowing);

            makeUI.SetHextechTab(UIHextechView.Mode.GiveElemental);
            makeUI.ForceHextechContentsOpen(false);

            SetNPC(Npc.PRUIT);
            yield return Show(LocalizeKey._26078.ToText(), UIWidget.Pivot.Top);

            yield return Show(LocalizeKey._26079.ToText(), UIWidget.Pivot.Top);

            ShowEmtpy();

            makeUI.StartGiveElementContentsOpenEffect();
            yield return Timing.WaitUntilTrue(makeUI.IsContentsOpenEffectFinished);

            yield return Show(LocalizeKey._26080.ToText(), UIWidget.Pivot.Top);

            yield return Show(LocalizeKey._26081.ToText(), UIWidget.Pivot.Top);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}