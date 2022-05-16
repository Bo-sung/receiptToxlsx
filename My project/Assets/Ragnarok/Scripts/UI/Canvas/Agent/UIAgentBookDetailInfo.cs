using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class UIAgentBookDetailInfo : UICanvas
    {
        public class Input : IUIData
        {
            public AgentBookData agentBookData;

            public Input(AgentBookData agentBookData)
            {
                this.agentBookData = agentBookData;
            }
        }

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIAgentIconHelper[] agentProfiles;
        [SerializeField] UILabelHelper agentBookName;

        [SerializeField] GameObject[] optionRoots;
        [SerializeField] UILabelHelper[] optionNames;
        [SerializeField] UILabelHelper[] optionValues;
        [SerializeField] UIButtonHelper closeButton;
        [SerializeField] UILabelHelper owningOptionPanelLabel;

        private AgentBookData agentBookData;

        protected override void OnInit()
        {
            EventDelegate.Add(closeButton.OnClick, OnClickClose);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(closeButton.OnClick, OnClickClose);
        }

        protected override void OnShow(IUIData data = null)
        {
            Input input = data as Input;
            agentBookData = input.agentBookData;

            agentBookName.Text = input.agentBookData.name_id.ToText();

            var bookStates = Entity.player.Agent.GetBookStates();
            var bookState = bookStates.FirstOrDefault(v => v.BookData.id == agentBookData.id);

            int index = 0;
            foreach (var each in bookState.GetAgentStates())
            {
                agentProfiles[index].SetActive(true);
                agentProfiles[index].SetData(each.Item1);
                agentProfiles[index].SetActiveSilhouette(!each.Item2);
                ++index;
            }

            for (int i = index; i < agentProfiles.Length; ++i)
                agentProfiles[i].SetActive(false);

            int optionCount = 0;
            foreach (var each in agentBookData.GetBattleOptions())
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

        protected override void OnLocalize()
        {
            titleLabel.LocalKey = LocalizeKey._47338; // 인연 정보
            owningOptionPanelLabel.LocalKey = LocalizeKey._47336; // 보유 효과
        }

        protected override void OnHide()
        {
        }

        private void OnClickClose()
        {
            UI.Close<UIAgentBookDetailInfo>();
        }
    }
}