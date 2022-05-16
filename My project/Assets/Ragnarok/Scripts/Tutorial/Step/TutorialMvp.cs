using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialMvp : TutorialStep
    {
        private readonly BattleManager battleManager;

        public TutorialMvp() : base(TutorialType.Mvp)
        {
            battleManager = BattleManager.Instance;
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (battleManager.Mode != BattleMode.Stage)
                return false;

            if (battleManager.IsPlayerDead())
                return false;

            if (battleManager.GetCurrentEntry() is StageEntry stageEntry)
            {
                // 이미 집결을 했을 경우
                if (stageEntry.IsAssemble())
                {
                    string notice = LocalizeKey._48600.ToText(); // 현재 위치에서 진행 가능한 퀘스트입니다.
                    UI.ShowToastPopup(notice);
                    return false;
                }
            }

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
            battleManager.GetCurrentEntry().Pause();

            HideTutorial();

            yield return WaitUntilHideDailyCheck();
            yield return WaitUntilHideCanvas<UICharacterShareWaiting>(); 
            yield return WaitUntilHideCanvas<UICharacterShareReward>();
            yield return WaitUntilHideCanvas<UIQuestReward>();
            yield return WaitUntilHideCanvas<UIWarning>();

            ShowTutorial();

            UIBattleStageMenu uiBattleStageMenu = UI.ShortCut<UIBattleStageMenu>(); // QuestUI의 이동하기를 통해서 튜토리얼이 시작할 수 있음

            yield return Show(LocalizeKey._26053.ToText(), UIWidget.Pivot.Center); // MVP가 등장했어요!\n캐릭터 아래의 화살표를 따라 MVP에게 접근하세요.
            yield return Show(LocalizeKey._26054.ToText(), UIWidget.Pivot.Top, uiBattleStageMenu?.AssembleWidget); // 집결!을 눌러 셰어 캐릭터와 함께 전투하세요.
            yield return Show(LocalizeKey._26055.ToText(), UIWidget.Pivot.Center); // MVP를 처치하면 보상을 획득할 수 있습니다!

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄

            battleManager.GetCurrentEntry().Resume();
        }
    }
}