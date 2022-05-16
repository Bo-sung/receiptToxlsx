using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialFirstStage : TutorialStep
    {
        public interface IImpl
        {
            UIWidget GetBtnAutoSharing();
            bool IsClickedBtnAutoSharing();
        }

        public TutorialFirstStage() : base(TutorialType.FirstStage)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            // UIBattleStageMenu 가 없음
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

            // Step1: 스토리북 오픈
            UI.Show<UIStoryBook>().SetChapter(1);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial(); // 튜토리얼 화면
            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}