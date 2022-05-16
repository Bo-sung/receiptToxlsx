using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialGateEnter : TutorialStep
    {
        public TutorialGateEnter() : base(TutorialType.GateEnter)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!Tutorial.HasAlreadyFinished(TutorialType.GateOpen))
                return false;

            if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.MultiMazeLobby)
                return false;

            int finalStageId = Entity.player.Dungeon.FinalStageId;
            int chapter = GateDataManager.Instance.First.chapter;
            StageData find = StageDataManager.Instance.FindWithChapter(StageChallengeType.Normal, chapter);
            if (find == null)
                return false;

            if (finalStageId < find.id)
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

            ShowTutorial();
            ShowBlind();

            // 저기 선장한테 다가가 보자. 쿡쿡쿡
            yield return Show(LocalizeKey._26173.ToText(), UIWidget.Pivot.Bottom);

            HideBlind();

            MultiMazeLobbyEntry multiMazeLobbyEntry = BattleManager.Instance.GetCurrentEntry() as MultiMazeLobbyEntry;
            multiMazeLobbyEntry.GoToCurrentNpc(16);

            yield return Timing.WaitUntilTrue(multiMazeLobbyEntry.IsMoveFinishedToNpc);

            ShowBlind();

            // NPC 옆으로 가면 입장 버튼을 볼 수 있지!\n쿡쿡쿡. 입장 버튼을 눌러줘.
            yield return Show(LocalizeKey._26150.ToText(), UIWidget.Pivot.Bottom, multiMazeLobbyEntry.GetMazeLobbyFieldWidget(), IsVisibleCanvas<UIGate>);

            UIGate gate = UI.GetUI<UIGate>();

            // 파티 생성을 누르면 자동으로 파티가 만들어질 꺼야.\n파티가 생성되면 파티원들은 항시 들어올 수 있어.
            yield return Show(LocalizeKey._26174.ToText(), UIWidget.Pivot.Bottom, gate.GetBtnCreateWidget());

            // 이곳은 다른 파티장이 만든 목록을 확인할 수 있어.\n파티 인원을 구하기 힘들다면 파티 참가해서 함께 도전하는 것도 좋은 방법이지~쿡쿡쿡!
            yield return Show(LocalizeKey._26175.ToText(), UIWidget.Pivot.Bottom, gate.GetBtnJoinWidget());

            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}