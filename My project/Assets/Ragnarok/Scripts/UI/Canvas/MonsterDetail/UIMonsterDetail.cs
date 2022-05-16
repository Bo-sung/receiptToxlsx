using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIMonsterDetail : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabel titleLabel;
        [SerializeField] UILabel skillPanelLabel;

        [SerializeField] UILabel nameLabel;
        [SerializeField] UILabel typeLabel;
        [SerializeField] UIButtonHelper iconElement;
        [SerializeField] UITextureHelper iconMonster;
        [SerializeField] UILabel hpLabel;
        [SerializeField] UILabel hpPanelLabel;
        [SerializeField] UILabel rangeLabel;
        [SerializeField] UILabel rangePanelLabel;
        [SerializeField] UILabel movingSpeedLabel;
        [SerializeField] UILabel movingSpeedPanelLabel;
        [SerializeField] UILabel attackSpeedLabel;
        [SerializeField] UILabel attackSpeedPanelLabel;
        [SerializeField] UILabel patkLabel;
        [SerializeField] UILabel patkPanelLabel;
        [SerializeField] UILabel pamrLabel;
        [SerializeField] UILabel pamrPanelLabel;
        [SerializeField] UILabel matkLabel;
        [SerializeField] UILabel matkPanelLabel;
        [SerializeField] UILabel mamrLabel;
        [SerializeField] UILabel mamrPanelLabel;
        [SerializeField] UITextureHelper[] skillIcons;
        [SerializeField] UIButton[] skillButtons;
        [SerializeField] UIButton exitButton;

        ElementType elementType;

        List<SkillInfo> skillInfoList = new List<SkillInfo>();

        protected override void OnInit()
        {
            for (int i = 0; i < skillButtons.Length; ++i)
                EventDelegate.Add(skillButtons[i].onClick, OnClickSkill);

            EventDelegate.Add(exitButton.onClick, OnClickClose);
            EventDelegate.Add(iconElement.OnClick, OnClickElement);
        }

        protected override void OnClose()
        {
            for (int i = 0; i < skillButtons.Length; ++i)
                EventDelegate.Remove(skillButtons[i].onClick, OnClickSkill);

            EventDelegate.Remove(exitButton.onClick, OnClickClose);
            EventDelegate.Remove(iconElement.OnClick, OnClickElement);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void ShowMonster(int monsterID, int monsterLevel)
        {
            var entity = MonsterEntity.Factory.CreateMonster(MonsterType.Normal, monsterID, monsterLevel, 1f);
            entity.ReloadStatus();
            entity.Dispose();

            var monsterData = entity.Monster.GetMonsterData();
            elementType = monsterData.element_type.ToEnum<ElementType>();

            nameLabel.text = string.Format("Lv. {0} {1}", monsterLevel, monsterData.name_id.ToText());
            var basicSkill = SkillDataManager.Instance.Get(monsterData.basic_active_skill_id, 1);

            int skillType = basicSkill.battle_option_type_1;
            string skillTypeStr = "";
            if (skillType == 1)
                skillTypeStr = LocalizeKey._49401.ToText();
            else if (skillType == 2)
                skillTypeStr = LocalizeKey._49402.ToText();
            else
                skillTypeStr = LocalizeKey._49403.ToText();

            //1 물리 2 마법 3 혼합
            typeLabel.text = LocalizeKey._49400.ToText().Replace(ReplaceKey.NAME, skillTypeStr);

            iconMonster.Set(monsterData.icon_name);
            iconElement.SpriteName = elementType.GetIconName();

            hpLabel.text = entity.MaxHP.ToString();
            hpPanelLabel.text = LocalizeKey._19012.ToText();

            rangeLabel.text = entity.battleSkillInfo.basicActiveSkill.SkillRange.ToString("F1");
            rangePanelLabel.text = LocalizeKey._19013.ToText();

            movingSpeedLabel.text = (entity.battleStatusInfo.MoveSpd / 100f).ToString("f2");
            movingSpeedPanelLabel.text = LocalizeKey._19014.ToText();

            attackSpeedLabel.text = (entity.battleStatusInfo.AtkSpd / 100f).ToString("f2");
            attackSpeedPanelLabel.text = LocalizeKey._19015.ToText();

            patkLabel.text = Math.Max(1, entity.battleStatusInfo.MeleeAtk).ToString();
            patkPanelLabel.text = LocalizeKey._19016.ToText();

            pamrLabel.text = entity.battleStatusInfo.Def.ToString();
            pamrPanelLabel.text = LocalizeKey._19017.ToText();

            matkLabel.text = Math.Max(1, entity.battleStatusInfo.MAtk).ToString();
            matkPanelLabel.text = LocalizeKey._19018.ToText();

            mamrLabel.text = entity.battleStatusInfo.MDef.ToString();
            mamrPanelLabel.text = LocalizeKey._19019.ToText();

            var skills = entity.battleSkillInfo.GetActiveSkills();
            int index = 0;

            for (int i = 0; i < 4; ++i)
                skillIcons[i].SetActive(false);

            skillInfoList.Clear();

            foreach (var each in skills)
            {
                skillInfoList.Add(each);
                skillIcons[index].SetActive(true);
                skillIcons[index].Set(each.IconName);
                ++index;
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleLabel.text = LocalizeKey._68000.ToText();
            skillPanelLabel.text = LocalizeKey._4101.ToText();
        }

        private void OnClickSkill()
        {
            int index = Array.IndexOf(skillButtons, UIButton.current);
            if (index == -1)
                return;

            if (index >= skillInfoList.Count)
                return;

            var skillData = skillInfoList[index];
            UI.Show<UISkillTooltip>(new UISkillTooltip.Input(skillData.SkillId, skillData.SkillLevel));
        }

        private void OnClickClose()
        {
            UI.Close<UIMonsterDetail>();
        }

        private void OnClickElement()
        {
            UI.Show<UISelectPropertyPopup>().ShowElementView(elementType);
        }
    }
}
