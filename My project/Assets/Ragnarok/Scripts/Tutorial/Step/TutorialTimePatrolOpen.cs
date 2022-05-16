using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialTimePatrolOpen : TutorialStep
    {
        public interface IEnterImpl
        {
            UIWidget GetLevelSelectWidget();
            UIWidget GetBtnEnterWidget();
            bool IsSelectedBtnEnter();
        }

        public TutorialTimePatrolOpen() : base(TutorialType.TimePatrolOpen)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            int finalStageId = Entity.player.Dungeon.FinalStageId;
            int timePatrolOpenStageId = BasisType.TP_OPEN_STAGE_ID.GetInt(); // 73
            if (finalStageId < timePatrolOpenStageId)
                return false;

            var mode = BattleManager.Instance.GetCurrentEntry().mode;
            if (mode != BattleMode.MultiMazeLobby && mode != BattleMode.Stage)
                return false;

            // UIMainShortcut UI 가 없음
            if (!IsVisibleCanvas<UIMainShortcut>())
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
            UI.ShortCut<UIStoryBook>().SetChapter(14);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();

            // 이럴 수가... 분명 보스를 처치했는데...\n이게 무슨 일이지?!
            yield return Show(LocalizeKey._26113.ToText(), UIWidget.Pivot.Center);

            // [EE3F48][c](치지직... 쉐어바이스 연결이 희미해진다.)[/c][-]
            yield return Show(LocalizeKey._26114.ToText(), UIWidget.Pivot.Center);

            // 큰일이야…. 불안정한 시공간이 화산 폭발 때문에 시공간 틀이 깨져버릴 것 같아..
            yield return Show(LocalizeKey._26115.ToText(), UIWidget.Pivot.Center);

            // 이대로 가다간..[EE3F48][c](치지직….)[/c][-] 우리도 말려들고 말겠군!
            yield return Show(LocalizeKey._26116.ToText(), UIWidget.Pivot.Center);

            TutorialMazeEnter.ISelectImpl openImpl = UI.GetUI<UIBattleMenu>();

            // 어서 Time Patrol로 이동해야 해!\n이 홀로루치님이 설명 놓치지 말고 잘 따라와야 한다구. 쿡쿡쿡!!
            yield return Show(LocalizeKey._26117.ToText(), UIWidget.Pivot.Bottom, openImpl.GetBtnMazeWidget(), IsVisibleCanvas<UIAdventureMazeSelect>);

            // Time Patrol 진입의 첫 단계!
            yield return Show(LocalizeKey._26118.ToText(), UIWidget.Pivot.Center);

            TutorialMazeEnter.IImpl impl = UI.GetUI<UIAdventureMazeSelect>();

            impl.SetTutorialMode(true);

            // Time Patrol버튼을 꾸욱 눌러보자구~
            yield return Show(LocalizeKey._26161.ToText(), UIWidget.Pivot.Center, impl.GetTimePatrolWidget(), IsVisibleCanvas<UITimePatrol>);

            impl.SetTutorialMode(false);

            IEnterImpl enterImpl = UI.GetUI<UITimePatrol>();

            // 이곳은 현재 Lv 위치와 클리어한 단계를 알 수 있지!쿡쿡쿡! 화살표를 이용한다면 클리어한 Lv만 자유롭게 오갈 수 있다는 말씀!
            yield return Show(LocalizeKey._26162.ToText(), UIWidget.Pivot.Bottom, enterImpl.GetLevelSelectWidget());           

            // 입장 버튼을 누르는 순간 Time Patrol의 세계는 시작이야! 쿡쿡쿡
            yield return Show(LocalizeKey._26163.ToText(), UIWidget.Pivot.Center);

            // 어서 미래를 향해 쭉쭉 나아가자구!
            yield return Show(LocalizeKey._26164.ToText(), UIWidget.Pivot.Center, enterImpl.GetBtnEnterWidget(), enterImpl.IsSelectedBtnEnter);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄

            HideTutorial();
        }
    }
}