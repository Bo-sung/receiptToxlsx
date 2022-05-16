using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialSkillLearn : TutorialStep
    {
        public interface IOpenSkillImpl
        {
            UIWidget GetBtnQuickSkill();
        }

        public interface ISkillImpl
        {
            ISelectActiveSkillImpl GetSelectActiveSkillImpl();
            ILevelUpSkillImpl GetLevelUpSkillImpl();
            TutorialSkillEquip.IEquipSkillImpl GetEquipSkillImpl();
        }

        public interface ISelectActiveSkillImpl
        {
            void SetTutorialMode(bool isTutorialMode);

            UIWidget GetActiveSkill();
            bool IsSelectedActiveSkill();
        }

        public interface ILevelUpSkillImpl
        {
            UIWidget GetBtnLevelUpSkill();
            bool IsClickedBtnLevelUpSkill();
        }

        public TutorialSkillLearn() : base(TutorialType.SkillLearn)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            // 레벨업 한 스킬이 존재할 경우
            if (Entity.player.Skill.HasInPossessionSkill())
                return false;

            // 스킬배우기 튜토리얼을 하지 않았음
            if (!Tutorial.HasAlreadyFinished(TutorialType.SharingCharacterEquip))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Skill, false))
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

            // SingleReward UI 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UISingleReward>();

            // UIJobReward UI 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIJobReward>();

            // UIQuestReward UI가 보이지 않을 때까지 기다린다.
            yield return WaitUntilHideCanvas<UIQuestReward>();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            // 튜토리얼 화면
            ShowTutorial();
            
            UIMain uiMain = UI.GetUI<UIMain>();
            
            yield return Show(LocalizeKey._66016.ToText(), UIWidget.Pivot.Center, uiMain.MenuWidget, IsVisibleCanvas<UIHome>);
            
            UIHome uiHome = UI.GetUI<UIHome>();
            
            yield return Show(LocalizeKey._66017.ToText(), UIWidget.Pivot.Top, uiHome.SkillButtonWidget, IsVisibleCanvas<UISkill>);

            // UISkill UI가 보일 때까지 기다림
            ISkillImpl skillImpl = UI.GetUI<UISkill>();
            ISelectActiveSkillImpl selectActiveSkillImpl = skillImpl.GetSelectActiveSkillImpl();
            ILevelUpSkillImpl levelUpSkillImpl = skillImpl.GetLevelUpSkillImpl();
            TutorialSkillEquip.IEquipSkillImpl equipSkillImpl = skillImpl.GetEquipSkillImpl();

            selectActiveSkillImpl.SetTutorialMode(true); // 튜토리얼 모드로 변경

            // Step2: 해당 스킬을 선택해주세요.
            yield return Show(LocalizeKey._66011.ToText(), UIWidget.Pivot.Center, selectActiveSkillImpl.GetActiveSkill(), selectActiveSkillImpl.IsSelectedActiveSkill);

            // Step3: 레벨업 버튼을 통해 [62AEE4][C]스킬 습득 및 강화[/c][-]가 가능합니다.
            yield return Show(LocalizeKey._66012.ToText(), UIWidget.Pivot.Top, levelUpSkillImpl.GetBtnLevelUpSkill(), levelUpSkillImpl.IsClickedBtnLevelUpSkill);

            yield return RequestFinish(); // 튜토리얼완료 프로토콜 호출

            // Step4: 해당 슬롯을 눌러 선택한 스킬을 [62AEE4][C]장착[/c][-]해봅시다.
            yield return Show(LocalizeKey._66013.ToText(), UIWidget.Pivot.Center, equipSkillImpl.GetSkillSlot(), equipSkillImpl.IsSelectedSkillSlot);

            yield return RequestFinish(type: TutorialType.SkillEquip); // 튜토리얼완료 프로토콜 호출 (스킬 장착)
            
            ShowEmtpy();

            selectActiveSkillImpl.SetTutorialMode(false); // 튜토리얼 모드로 변경

            UIManager.Instance.ShortCut(); // 모든 UI 닫기
        }
    }
}