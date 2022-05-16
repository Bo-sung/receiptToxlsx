using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialBossSummon : TutorialStep
    {
        public interface IImpl
        {
            UIWidget GetBtnSummonWidget();
        }

        public TutorialBossSummon() : base(TutorialType.BossSummon)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Boss, false))
                return false;

            // UIBattleStageMenu UI가 없음
            if (!IsVisibleCanvas<UIBattleStageMenu>())
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

            // UIDailyCheck UI 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideDailyCheck();

            // UIQuestReward UI가 보이지 않을 때까지 기다린다.
            yield return WaitUntilHideCanvas<UIQuestReward>();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            // 튜토리얼 화면
            ShowTutorial();

            IImpl impl = UI.GetUI<UIBattleStageMenu>();
            
            yield return Show(LocalizeKey._66014.ToText(), UIWidget.Pivot.Center);
            
            yield return Show(LocalizeKey._66015.ToText(), UIWidget.Pivot.Center, impl.GetBtnSummonWidget());

            // var stageMenu = UI.GetUI<UIBattleStageMenu>();
            // 
            // stageMenu.ForceShowAssemble(true);
            // 
            // yield return Show(LocalizeKey._66019.ToText(), UIWidget.Pivot.Top, stageMenu.AssembleWidget);
            // 
            // stageMenu.ForceShowAssemble(false);

            yield return Show(LocalizeKey._66020.ToText(), UIWidget.Pivot.Center);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}