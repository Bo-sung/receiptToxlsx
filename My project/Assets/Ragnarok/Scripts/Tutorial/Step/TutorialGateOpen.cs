using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialGateOpen : TutorialStep
    {
        public TutorialGateOpen() : base(TutorialType.GateOpen)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            int finalStageId = Entity.player.Dungeon.FinalStageId;
            int chapter = GateDataManager.Instance.First.chapter;
            StageData find = StageDataManager.Instance.FindWithChapter(StageChallengeType.Normal, chapter);
            if (find == null)
                return false;

            if (finalStageId < find.id)
                return false;

            var mode = BattleManager.Instance.GetCurrentEntry().mode;
            if (mode != BattleMode.Stage)
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
            UI.ShortCut<UIStoryBook>().SetChapter(15);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();

            // 안녕? 2가지 소식을 가져왔어!
            yield return Show(LocalizeKey._26165.ToText(), UIWidget.Pivot.Center);

            // 한가지는, 드디어 미래를 갈 수 있다는 말씀! 쿡쿡쿡!!
            yield return Show(LocalizeKey._26166.ToText(), UIWidget.Pivot.Center);

            // 나쁜 소식은 미래를 통하는 Gate를 통해 이동해야 하는데 그곳에서 이상 감지를 느꼈어!
            yield return Show(LocalizeKey._26167.ToText(), UIWidget.Pivot.Center);

            // 누군가 Gate 문을 막고 있는 듯해!
            yield return Show(LocalizeKey._26168.ToText(), UIWidget.Pivot.Center);

            // 우리 미래 이동을 방해하는 녀석을 무찌르러 가볼까?!
            yield return Show(LocalizeKey._26169.ToText(), UIWidget.Pivot.Center);

            TutorialMazeEnter.ISelectImpl openImpl = UI.GetUI<UIBattleMenu>();

            // 이 홀로루치님이 차근차근 미래로 이동하는 방법을 알려줄게!
            yield return Show(LocalizeKey._26170.ToText(), UIWidget.Pivot.Bottom, openImpl.GetBtnMazeWidget(), IsVisibleCanvas<UIAdventureMazeSelect>);

            UIAdventureMazeSelect ui = UI.GetUI<UIAdventureMazeSelect>();
            ui.SelectId(MultiMazeWaitingRoomData.GATE_1);

            // 드디어 Gate 1이 오픈 됐어!!
            yield return Show(LocalizeKey._26171.ToText(), UIWidget.Pivot.Center);

            TutorialMazeEnter.IImpl impl = ui;

            impl.SetTutorialMode(true);

            // 어서 Gate 1을 눌러보라구~
            yield return Show(LocalizeKey._26172.ToText(), UIWidget.Pivot.Center, impl.GetGate1Widget(), impl.IsSelectFirstMazeEnter);

            impl.SetTutorialMode(false);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄

            HideTutorial();
        }
    }
}