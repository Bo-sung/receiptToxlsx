using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TurotialJobChange : TutorialStep
    {
        public interface IOpenJobChangeImpl
        {
            UIWidget GetBtnJobChange();
            bool IsClickedBtnJobChange();
        }

        public interface ISelectJobImpl
        {
            bool IsFinishedJobChange();
        }

        public interface IAutoEquipImpl
        {
            void SetTutorialMode(bool isTutorialMode);

            UIWidget GetBackgroundWidget();
            bool IsClickedBtnEquip();
        }

        public TurotialJobChange() : base(TutorialType.JobChange)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            // 전직 불가능
            if (!Entity.player.Character.CanChangeJob())
                return false;

            // 이미 전직 차수가 높을 경우
            if (Entity.player.Character.JobGrade() != 0)
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.JobChange, false))
                return false;

            // UIJobChangeMenu UI가 존재 또는 UIJobChange UI가 존재
            return IsVisibleCanvas<UIJobChangeMenu>() || IsVisibleCanvas<UIJobChange>();
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

            // UIDailyCheck UI가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideDailyCheck();

            // UIQuestReward UI가 보이지 않을 때까지 기다린다.
            yield return WaitUntilHideCanvas<UIQuestReward>();

            // UIQuestReward UI가 보이지 않을 때까지 기다린다.
            yield return WaitUntilHideCanvas<UIQuestReward>();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            // 튜토리얼 화면
            ShowTutorial();

            // UIJobChangeMenu UI가 보일 때
            if (IsVisibleCanvas<UIJobChangeMenu>())
            {
                IOpenJobChangeImpl openJobChangeImpl = UI.GetUI<UIJobChangeMenu>();

                // Step1: 전직 가능한 레벨이 되셨네요. 전직을 하러 가볼까요?
                yield return Show(LocalizeKey._66007.ToText(), UIWidget.Pivot.Top, openJobChangeImpl.GetBtnJobChange(), openJobChangeImpl.IsClickedBtnJobChange);
            }

            // UIJobChange UI가 보일 때까지 기다림
            yield return WaitUntilShowCanvas<UIJobChange>();

            ISelectJobImpl selectJobImpl = UI.GetUI<UIJobChange>();

            // Step2: 원하는 직업을 선택하면, 직업 상세정보를 확인할 수 있습니다.
            yield return Show(LocalizeKey._66008.ToText(), UIWidget.Pivot.Bottom);

            HideTutorial();

            // 전직할 때까지 기다린다.
            yield return Timing.WaitUntilTrue(selectJobImpl.IsFinishedJobChange);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}