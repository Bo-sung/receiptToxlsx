using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialSkillEquip : TutorialStep
    {
        public interface IEquipSkillImpl
        {
            UIWidget GetSkillSlot();
            bool IsSelectedSkillSlot();
        }

        public TutorialSkillEquip() : base(TutorialType.SkillEquip)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            // 스킬배우기 튜토리얼을 하지 않았음
            if (!Tutorial.HasAlreadyFinished(TutorialType.SkillLearn))
                return false;

            // UIBattlePlayerStatus UI가 없음
            if (!IsVisibleCanvas<UIBattlePlayerStatus>())
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

            // 튜토리얼 화면
            ShowTutorial();

            UIMain uiMain = UI.GetUI<UIMain>();

            yield return Show(LocalizeKey._66016.ToText(), UIWidget.Pivot.Center, uiMain.MenuWidget, IsVisibleCanvas<UIHome>);

            UIHome uiHome = UI.GetUI<UIHome>();

            yield return Show(LocalizeKey._66017.ToText(), UIWidget.Pivot.Top, uiHome.SkillButtonWidget, IsVisibleCanvas<UISkill>);
            
            TutorialSkillLearn.ISkillImpl skillImpl = UI.GetUI<UISkill>();
            TutorialSkillLearn.ISelectActiveSkillImpl selectActiveSkillImpl = skillImpl.GetSelectActiveSkillImpl();
            IEquipSkillImpl equipSkillImpl = skillImpl.GetEquipSkillImpl();

            selectActiveSkillImpl.SetTutorialMode(true); // 튜토리얼 모드로 변경

            // Step2: 해당 스킬을 선택해주세요.
            yield return Show(LocalizeKey._66011.ToText(), UIWidget.Pivot.Center, selectActiveSkillImpl.GetActiveSkill(), selectActiveSkillImpl.IsSelectedActiveSkill);

            // Step3: 해당 슬롯을 눌러 선택한 스킬을 [62AEE4][C]장착[/c][-]해봅시다.
            yield return Show(LocalizeKey._66013.ToText(), UIWidget.Pivot.Center, equipSkillImpl.GetSkillSlot(), equipSkillImpl.IsSelectedSkillSlot);

            yield return RequestFinish(); // 튜토리얼완료 프로토콜 호출

            ShowEmtpy();

            selectActiveSkillImpl.SetTutorialMode(false); // 튜토리얼 모드로 변경

            UIManager.Instance.ShortCut(); // 모든 UI 닫기
        }
    }
}