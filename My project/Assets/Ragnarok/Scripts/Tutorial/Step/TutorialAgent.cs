using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialAgent : TutorialStep
    {
        public interface IAgentImpl
        {
        }

        public TutorialAgent() : base(TutorialType.Agent)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            // Agent UI 가 없음
            if (!IsVisibleCanvas<UIAgent>())
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

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}