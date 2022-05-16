using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIAgentBookSlot : UIInfo<AgentBookSlotViewInfo>, IInspectorFinder
    {
        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UILabelHelper battleOptionLabel;
        [SerializeField] UIGridHelper grid;
        [SerializeField] UIAgentIconHelper[] agentSlotSprites;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UIButtonHelper receiveButton;
        [SerializeField] GameObject activeBookEffect;
        [SerializeField] GameObject completeMark;
        [SerializeField] GameObject canCompleteIcon;
        [SerializeField] UIButtonHelper bookInfoButton;

        private AgentBookData agentBookData;
        private List<(AgentData, bool)> infoList = new List<(AgentData, bool)>();

        protected override void Start()
        {
            base.Start();
            EventDelegate.Add(receiveButton.OnClick, OnClickReceive);
            EventDelegate.Add(bookInfoButton.OnClick, OpenBookDetailInfo);

            for (int i = 0; i < agentSlotSprites.Length; ++i)
                agentSlotSprites[i].OnClick += OpenProfile;
        }

        protected override void OnLocalize()
        {
            labelReward.LocalKey = LocalizeKey._47347; // 보상 목록

            if (info != null)
                base.OnLocalize();
        }

        protected override void Refresh()
        {
            bool hasUnpossessingAgent = false;
            agentBookData = info.bookData;
            var it = info.bookState.GetAgentStates().GetEnumerator();
            canCompleteIcon.SetActive(info.bookState.CanComplete());

            infoList.Clear();

            for (int i = 0; i < agentSlotSprites.Length; ++i)
            {
                if (i < info.bookState.RequireAgentCount)
                {
                    it.MoveNext();
                    var state = it.Current;
                    infoList.Add(state);
                    var agent = state.Item1;
                    var isOwningAgent = state.Item2;

                    agentSlotSprites[i].SetActive(true);
                    agentSlotSprites[i].SetData(state.Item1);
                    agentSlotSprites[i].SetActiveSilhouette(!isOwningAgent);
                    if (!isOwningAgent)
                        hasUnpossessingAgent = true;
                }
                else
                {
                    agentSlotSprites[i].SetActive(false);
                }
            }

            grid.Reposition();

            titleLabel.Text = info.bookData.name_id.ToText();

            int count = 0;
            BattleOption firstOption = default;
            foreach (var each in info.bookData.GetBattleOptions())
            {
                count++;
                if (count == 1)
                    firstOption = each;
            }

            if (count > 0)
            {
                string color = info.bookState.IsRewarded ? "69B2E6" : "9E9B9E";

                battleOptionLabel.SetActive(true);
                battleOptionLabel.Text = LocalizeKey._47312.ToText() // [9E9B9E]보유 효과{{COUNT}}[-] [{COLOR}]{VALUE}[-]
                    .Replace(ReplaceKey.COUNT, count)
                    .Replace(ReplaceKey.COLOR, color)
                    .Replace(ReplaceKey.VALUE, $"{firstOption.GetDescription()}{(count > 1 ? " ..." : "")}");
            }
            else
            {
                battleOptionLabel.SetActive(false);
            }

            reward.SetData(new RewardData((byte)(int)info.bookData.reward_type, info.bookData.reward_value, info.bookData.reward_count));

            if (info.bookState.IsRewarded)
            {
                receiveButton.IsEnabled = false;
                receiveButton.Text = LocalizeKey._47335.ToText();
                completeMark.SetActive(true);
            }
            else if (hasUnpossessingAgent)
            {
                receiveButton.IsEnabled = false;
                receiveButton.Text = LocalizeKey._12003.ToText();
                completeMark.SetActive(false);
            }
            else
            {
                receiveButton.IsEnabled = true;
                receiveButton.Text = LocalizeKey._12003.ToText();
                completeMark.SetActive(false);
            }

            activeBookEffect.SetActive(info.bookState.IsRewarded);
        }

        private void OnClickReceive()
        {
            info.InvokeReceiveReward();
        }

        private void OpenProfile(AgentData agentData)
        {
            var infoIndex = infoList.FindIndex(v => v.Item1 == agentData);

            if (infoIndex != -1)
                UI.Show<UIAgentDetailInfo>(new UIAgentDetailInfo.Input(agentData, infoList[infoIndex].Item2));
        }

        private void OpenBookDetailInfo()
        {
            UI.Show<UIAgentBookDetailInfo>(new UIAgentBookDetailInfo.Input(agentBookData));
        }

        bool IInspectorFinder.Find()
        {
            agentSlotSprites = GetComponentsInChildren<UIAgentIconHelper>();
            return true;
        }
    }
}