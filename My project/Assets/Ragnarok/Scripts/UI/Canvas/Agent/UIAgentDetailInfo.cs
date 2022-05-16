using UnityEngine;

namespace Ragnarok
{
    public class UIAgentDetailInfo : UICanvas
    {
        public class Input : IUIData
        {
            public AgentData agentData;
            public bool isOwning;

            public Input(AgentData agentData, bool isOwning)
            {
                this.agentData = agentData;
                this.isOwning = isOwning;
            }
        }

        protected override UIType uiType => UIType.Destroy | UIType.Back;

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIAgentIconHelper agentProfile;
        [SerializeField] UILabelHelper agentName;
        [SerializeField] UITextureHelper[] iconSkills;
        [SerializeField] UIButtonHelper[] skillButtons;
        [SerializeField] UIAgentSlotExploreTypeSlot[] exploreTypeSlots;
        [SerializeField] GameObject[] agentTypes;
        [SerializeField] GameObject[] optionRoots;
        [SerializeField] UILabelHelper[] optionNames;
        [SerializeField] UILabelHelper[] optionValues;
        [SerializeField] UIButtonHelper closeButton;
        [SerializeField] UILabelHelper owningOptionPanelLabel;
        [SerializeField] UILabelHelper[] agentTypeLabels;

        private AgentData agentData;

        protected override void OnInit()
        {
            for (int i = 0; i < skillButtons.Length; ++i)
                EventDelegate.Add(skillButtons[i].OnClick, OpenSkillUI);
            EventDelegate.Add(closeButton.OnClick, OnClickClose);
        }

        protected override void OnClose()
        {
            for (int i = 0; i < skillButtons.Length; ++i)
                EventDelegate.Remove(skillButtons[i].OnClick, OpenSkillUI);
            EventDelegate.Remove(closeButton.OnClick, OnClickClose);
        }

        protected override void OnShow(IUIData data = null)
        {
            Input input = data as Input;
            agentData = input.agentData;

            agentProfile.SetData(input.agentData);
            agentProfile.SetActiveSilhouette(input.isOwning == false);
            agentName.Text = input.agentData.name_id.ToText();

            agentTypes[0].SetActive(agentData.agent_type == (int)AgentType.CombatAgent);
            agentTypes[1].SetActive(agentData.agent_type == (int)AgentType.ExploreAgent);

            if (input.agentData.agent_type == (int)AgentType.CombatAgent)
            {
                for (int i = 0; i < exploreTypeSlots.Length; ++i)
                    exploreTypeSlots[i].gameObject.SetActive(false);

                for (int i = 0; i < iconSkills.Length; ++i)
                {
                    int skillId = input.agentData.GetSkillId(i);
                    if (skillId == 0)
                    {
                        iconSkills[i].SetActive(false);
                    }
                    else
                    {
                        SkillData skillData = SkillDataManager.Instance.Get(skillId, 1);

                        if (skillData != null)
                        {
                            iconSkills[i].SetSkill(skillData.icon_name);
                            iconSkills[i].SetActive(true);
                        }
                        else
                        {
                            Debug.LogError($"[UIAgentSlot] SkillData 가 없습니다. : {skillId}");
                            iconSkills[i].SetActive(false);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < iconSkills.Length; ++i)
                    iconSkills[i].gameObject.SetActive(false);

                int exploreTypeCount = 0;
                foreach (var each in input.agentData.GetExploreTypes())
                {
                    exploreTypeSlots[exploreTypeCount].gameObject.SetActive(true);
                    exploreTypeSlots[exploreTypeCount].SetView(AgentListPresenter.ExploreSlotState.NoExploring, each, 0);
                    exploreTypeCount++;
                }

                for (int i = exploreTypeCount; i < exploreTypeSlots.Length; ++i)
                    exploreTypeSlots[i].gameObject.SetActive(false);
            }

            int optionCount = 0;
            foreach (var each in agentData.GetBattleOptions())
            {
                optionRoots[optionCount].SetActive(true);
                optionNames[optionCount].Text = each.GetTitleText();
                optionValues[optionCount].Text = each.GetValueText();
                ++optionCount;
            }

            for (int i = optionCount; i < optionRoots.Length; ++i)
            {
                optionRoots[i].SetActive(false);
            }
        }

        private void OpenSkillUI()
        {
            int index = UIButton.current.transform.GetSiblingIndex();
            int skillId = agentData.GetSkillId(index);

            if (skillId == 0)
                return;

            UI.Show<UISkillTooltip>(new UISkillTooltip.Input(skillId, 1));
        }

        protected override void OnLocalize()
        {
            titleLabel.LocalKey = LocalizeKey._47337; // 동료 정보
            owningOptionPanelLabel.LocalKey = LocalizeKey._47336; // 보유 효과
            agentTypeLabels[0].LocalKey = LocalizeKey._47306; // PVP 동료
            agentTypeLabels[1].LocalKey = LocalizeKey._47307; // 파견 동료
        }

        protected override void OnHide()
        {
        }

        private void OnClickClose()
        {
            UI.Close<UIAgentDetailInfo>();
        }
    }
}