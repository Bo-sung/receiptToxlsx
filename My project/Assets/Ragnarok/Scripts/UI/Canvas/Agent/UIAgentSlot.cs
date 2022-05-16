using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIAgentList"/>
    /// 동료 UI 슬롯
    /// </summary>
    public class UIAgentSlot : UIInfo<AgentSlotViewInfo>
    {
        [SerializeField] UIAgentIconHelper profile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UITextureHelper[] iconSkills;
        [SerializeField] UIButtonHelper[] skillButtons;
        [SerializeField] UIAgentSlotExploreTypeSlot[] exploreTypeSlots;

        [SerializeField] GameObject agentStateEquipped;
        [SerializeField] GameObject agentStateExploring;
        [SerializeField] GameObject agentStateExploreFinished;
        [SerializeField] UILabelHelper agentStateEquippedLabel;
        [SerializeField] UILabelHelper agentStateExploringLabel;
        [SerializeField] UILabelHelper agentStateExploreFinishedLabel;

        [SerializeField] Color32 enableDuplicationColor, disableDuplicationColor;
        [SerializeField] UILabelHelper duplicationCountText;
        [SerializeField] UILabelHelper battleOptionText;
        [SerializeField] GameObject selectionEffect;
        [SerializeField] GameObject disableMask;
        [SerializeField] GameObject duplicationCountRoot;

        [SerializeField] GameObject[] agentTypes;
        [SerializeField] UILabelHelper[] agentTypeLabels;
        [SerializeField] UITweener focusEffectTween;
        [SerializeField] GameObject focusEffect;
        [SerializeField] GameObject newIcon;
        [SerializeField] UIButton detailButton;        

        private bool needLocalize = true;

        protected override void Start()
        {
            base.Start();
            for (int i = 0; i < skillButtons.Length; ++i)
                EventDelegate.Add(skillButtons[i].OnClick, OnClickSkill);

            EventDelegate.Add(focusEffectTween.onFinished, HideFocusEffect);

            EventDelegate.Add(detailButton.onClick, OpenProfileUI2);
            profile.OnClick += InvokeOnClick;
        }

        protected override void AddInfoEvent()
        {
            base.AddInfoEvent();
            info.OnFocus += DoFocusEffect;
        }

        protected override void RemoveInfoEvent()
        {
            base.RemoveInfoEvent();
            info.OnFocus -= DoFocusEffect;
        }

        private void DoFocusEffect()
        {
            focusEffect.SetActive(true);
            focusEffectTween.PlayForward();
            focusEffectTween.ResetToBeginning();
        }

        private void OpenProfileUI2()
        {
            if (info.OwningAgent != null && info.OwningAgent.IsNew)
            {
                info.OwningAgent.IsNew = false;
                newIcon.SetActive(false);
            }

            UI.Show<UIAgentDetailInfo>(new UIAgentDetailInfo.Input(info.AgentData, info.OwningAgent != null));
        }

        private void InvokeOnClick(AgentData agentData)
        {
            OnClick();
        }

        protected override void Refresh()
        {
            if (needLocalize)
            {
                agentStateEquippedLabel.LocalKey = LocalizeKey._47310; // 장착 중
                agentStateExploringLabel.LocalKey = LocalizeKey._47308; // 파견 중
                agentStateExploreFinishedLabel.LocalKey = LocalizeKey._47309; // 파견완료
                agentTypeLabels[0].LocalKey = LocalizeKey._47306; // PVP 동료
                agentTypeLabels[1].LocalKey = LocalizeKey._47307; // 파견 동료

                needLocalize = false;
            }

            newIcon.SetActive(info.ShowAgentNewIcon && info.OwningAgent != null && info.OwningAgent.IsNew);
            focusEffect.SetActive(false);
            focusEffectTween.ResetToBeginning();

            profile.SetData(info.AgentData);
            profile.SetActiveSilhouette(info.OwningAgent == null);

            labelName.Text = info.AgentData.name_id.ToText();

            if (info.HideDuplicationCountOnZero && info.DuplicationCount == 0)
            {
                duplicationCountRoot.SetActive(false);
            }
            else
            {
                duplicationCountRoot.SetActive(true);
                duplicationCountText.Text = info.DuplicationCount > 99 ? "99+" : $"{info.DuplicationCount}";
                if (info.DuplicationCount == 0)
                {
                    duplicationCountText.Color = disableDuplicationColor;
                }
                else
                {
                    duplicationCountText.Color = enableDuplicationColor;
                }
            }

            if (info.ShowAgentTypePanel)
            {
                agentTypes[0].SetActive(info.AgentData.agent_type == (int)AgentType.CombatAgent);
                agentTypes[1].SetActive(info.AgentData.agent_type == (int)AgentType.ExploreAgent);
            }
            else
            {
                agentTypes[0].SetActive(false);
                agentTypes[1].SetActive(false);
            }

            selectionEffect.SetActive(info.IsSelected);

            if (info.ShowNoDuplicationMask)
                disableMask.SetActive(info.DuplicationCount == 0);
            else
                disableMask.SetActive(false);

            int count = 0;
            BattleOption firstOption = default;
            foreach (var each in info.AgentData.GetBattleOptions())
            {
                count++;
                if (count == 1)
                    firstOption = each;
            }

            if (count > 0)
            {
                string color = info.OwningAgent != null ? "69B2E6" : "9E9B9E";

                battleOptionText.SetActive(true);
                battleOptionText.Text = LocalizeKey._47312.ToText() // [9E9B9E]보유 효과{{COUNT}}[-] [{COLOR}]{VALUE}[-]
                    .Replace(ReplaceKey.COUNT, count)
                    .Replace(ReplaceKey.COLOR, color)
                    .Replace(ReplaceKey.VALUE, $"{firstOption.GetDescription()}{(count > 1 ? " ..." : "")}");
            }
            else
            {
                battleOptionText.SetActive(false);
            }

            var agentType = (AgentType)(int)info.AgentData.agent_type;

            if (agentType == AgentType.CombatAgent)
            {
                if (info.ShowAgentStatePanel && info.OwningAgent != null)
                {
                    agentStateExploring.SetActive(false);
                    agentStateExploreFinished.SetActive(false);
                    agentStateEquipped.SetActive(IsUsingAgent());
                }
                else
                {
                    agentStateExploring.SetActive(false);
                    agentStateExploreFinished.SetActive(false);
                    agentStateEquipped.SetActive(false);
                }

                duplicationCountText.SetActive(true);

                for (int i = 0; i < exploreTypeSlots.Length; ++i)
                    exploreTypeSlots[i].gameObject.SetActive(false);

                for (int i = 0; i < iconSkills.Length; ++i)
                {
                    int skillId = info.AgentData.GetSkillId(i);
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
            else if (agentType == AgentType.ExploreAgent)
            {
                ExploreAgent agent = info.OwningAgent as ExploreAgent;

                if (info.ShowAgentStatePanel && info.OwningAgent != null)
                {
                    agentStateEquipped.SetActive(false);

                    if (agent.IsExploring)
                    {
                        float remainTime = agent.ProgressingExplore.RemainTime;
                        agentStateExploring.SetActive(remainTime > 0.0f);
                        agentStateExploreFinished.SetActive(remainTime == 0.0f);
                    }
                    else
                    {
                        agentStateExploring.SetActive(false);
                        agentStateExploreFinished.SetActive(false);
                    }
                }
                else
                {
                    agentStateExploring.SetActive(false);
                    agentStateExploreFinished.SetActive(false);
                    agentStateEquipped.SetActive(false);
                }

                for (int i = 0; i < iconSkills.Length; ++i)
                    iconSkills[i].gameObject.SetActive(false);

                int exploreTypeCount = 0;
                bool isExploring = false;

                foreach (var each in info.AgentData.GetExploreTypes())
                    if (agent != null && agent.IsDoingExplore(each))
                        isExploring = true;

                foreach (var each in info.AgentData.GetExploreTypes())
                {
                    exploreTypeSlots[exploreTypeCount].gameObject.SetActive(true);

                    if (agent != null && agent.IsDoingExplore(each))
                    {
                        exploreTypeSlots[exploreTypeCount].SetView(agent.ProgressingExplore.RemainTime == 0 ?
                            AgentListPresenter.ExploreSlotState.ExploreFinished : AgentListPresenter.ExploreSlotState.Exploring,
                            each, agent.ProgressingExplore.RemainTime);
                    }
                    else
                    {
                        exploreTypeSlots[exploreTypeCount].SetView(isExploring ?
                            AgentListPresenter.ExploreSlotState.CantExplore : AgentListPresenter.ExploreSlotState.NoExploring,
                            each, 0);
                    }

                    exploreTypeCount++;
                }

                for (int i = exploreTypeCount; i < exploreTypeSlots.Length; ++i)
                    exploreTypeSlots[i].gameObject.SetActive(false);
            }
        }

        private void OnClickSkill()
        {
            int index = UIButton.current.transform.GetSiblingIndex();
            int skillId = info.AgentData.GetSkillId(index);

            if (skillId == 0)
                return;

            UI.Show<UISkillTooltip>(new UISkillTooltip.Input(skillId, 1));
        }

        void OnClick()
        {
            info.InvokeOnClick();
        }

        bool IsUsingAgent()
        {
            if (info.AgentData.agent_type == (int)AgentType.CombatAgent)
            {
                if (info.OwningAgent == null)
                    return false;

                var agent = info.OwningAgent as CombatAgent;
                return agent.IsUsingAgent;
            }

            return false;
        }

        private void HideFocusEffect()
        {
            focusEffect.SetActive(true);
        }
    }
}