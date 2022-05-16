using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialShareVice2ndOpen : TutorialStep
    {        
        public interface IOpenCharacterShareImpl
        {          
            UIWidget GetShareWidget();          
        }

        public interface IOpenCharacterShare2ndImpl
        {
            UIWidget GetShareForceWidget();
            UIWidget GetShareForceSlotWidget();
        }

        public interface IShareForceLevelUpImpl
        {
            UIWidget GetBtnLevelUpWidget();
        }

        public TutorialShareVice2ndOpen() : base(TutorialType.ShareVice2ndOpen)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.ShareVice2ndOpen))
                return false;

            int finalStageId = Entity.player.Dungeon.FinalStageId;
            int timePatrolOpenStageId = BasisType.TP_OPEN_STAGE_ID.GetInt();
            if (finalStageId < timePatrolOpenStageId)
                return false;

            if (!Entity.player.Character.HasShareForce(ShareForceType.ShareForce1))
                return false;

            var mode = BattleManager.Instance.GetCurrentEntry().mode;
            if (mode != BattleMode.TimePatrol)
                return false;

            // UIBattleShare2nd UI 가 없음
            if (!IsVisibleCanvas<UIBattleShare2nd>())
                return false;

            return true;
        }

        protected override bool HasSkip()
        {
            return false;
        }

        protected override Npc GetNpc()
        {
            return Npc.CLOCK_TOWER;
        }

        protected override IEnumerator<float> Run()
        {
            HideTutorial();

            yield return WaitUntilHideDailyCheck();
            yield return WaitUntilHideCanvas<UIQuestReward>();
            yield return WaitUntilHideCanvas<UICharacterShareWaiting>();
            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            // 튜토리얼 화면
            ShowTutorial();

            // 몬스터 수가 조금 줄었군.흠흠..\n이제 2세대 쉐어 바이스를 알려주겠네.
            yield return Show(LocalizeKey._26141.ToText(), UIWidget.Pivot.Center);

            IOpenCharacterShareImpl openCharacterShareImpl = UI.GetUI<UIBattleShare2nd>();
            // 새롭게 바뀐 2세대 쉐어 바이스를 열어보게.
            yield return Show(LocalizeKey._26142.ToText(), UIWidget.Pivot.Center, openCharacterShareImpl.GetShareWidget(), IsVisibleCanvas<UICharacterShare2nd>);

            IOpenCharacterShare2ndImpl openCharacterShare2ndImpl = UI.GetUI<UICharacterShare2nd>();

            // 미래에는 강력한 몬스터와 전투 이전에 이 쉐어 바이스를 이용해서 강해지는 것이 핵심이라는 것을 명심하게나…
            yield return Show(LocalizeKey._26143.ToText(), UIWidget.Pivot.Top);

            // 퀘스트에서 얻은 쉐어 포스는 이 곳에서 활성화가 된다네.\n쉐어 포스의 Lv이 높을 수록 더욱 강해질 수 있지…흠흠.
            yield return Show(LocalizeKey._26144.ToText(), UIWidget.Pivot.Top, openCharacterShare2ndImpl.GetShareForceWidget());

            // 강화하는 방법을 알려주지.\n활성화된 쉐어 포스를 눌러보게.
            yield return Show(LocalizeKey._26145.ToText(), UIWidget.Pivot.Top, openCharacterShare2ndImpl.GetShareForceSlotWidget(), IsVisibleCanvas<UIShareForceLevelUp>);

            IShareForceLevelUpImpl shareForceLevelUpImpl = UI.GetUI<UIShareForceLevelUp>();

            // 쉐어 포스 강화 버튼을 누르면 자네는 한 층 더 강한 영웅이 될 수 있으니\n쉐어 포스 강화에 신경을 쓰는게 좋을 게야…흠흠
            yield return Show(LocalizeKey._26146.ToText(), UIWidget.Pivot.Top, shareForceLevelUpImpl.GetBtnLevelUpWidget());

            // 이로써, 쉐어 포스에 대한 설명은 끝 일세.\n 조심히 다뤄주게나..
            yield return Show(LocalizeKey._26147.ToText(), UIWidget.Pivot.Top);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄

            HideTutorial();
        }
    }
}