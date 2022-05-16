using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialDuel : TutorialStep
    {
        public TutorialDuel() : base(TutorialType.Duel)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Duel, false))
                return false;

            if (Entity.player.Character.Job == Job.Novice)
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

            // UIQuestReward UI가 보이지 않을 때까지 기다린다.
            yield return WaitUntilHideCanvas<UIQuestReward>();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            // 튜토리얼 화면
            ShowTutorial();

            yield return WaitUntilShowCanvas<UIBattleMenu>();

            UIBattleMenu battleMenu = UI.GetUI<UIBattleMenu>();

            yield return Show(LocalizeKey._26043.ToText(), UIWidget.Pivot.Center, battleMenu.DuelButtonWidget, () => IsVisibleCanvas<UIDuel>());

            UIDuel duelUI = UI.GetUI<UIDuel>();

            yield return Show(LocalizeKey._26044.ToText(), UIWidget.Pivot.Bottom, duelUI.DuelWordPanelWidget);

            yield return Show(LocalizeKey._26045.ToText(), UIWidget.Pivot.Bottom, duelUI.DuelWordPanelWidget);

            yield return Show(LocalizeKey._26048.ToText(), UIWidget.Pivot.Bottom, duelUI.CombatAgentButtonWidget);

            yield return Show(LocalizeKey._26049.ToText(), UIWidget.Pivot.Bottom, duelUI.DuelWordPanelWidget);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}