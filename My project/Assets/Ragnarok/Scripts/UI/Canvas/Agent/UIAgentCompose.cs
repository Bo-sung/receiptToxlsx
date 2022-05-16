using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Ragnarok
{
    public class UIAgentCompose : MonoBehaviour
    {
        public enum EventType
        {
            OnClickMaterialSlot,
            OnClickAgent,
            OnClickBulkSelectRank,
            OnClickCompose
        }
        
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIButtonHelper bulkSelectButton;
        [SerializeField] UICostButtonHelper composeButton;
        [SerializeField] UILabelHelper composeCountLabel;
        [SerializeField] UILabelHelper noticeLabel;
        [SerializeField] UIAgentMaterialSlot[] materialSlots;
        [SerializeField] TweenScale countChangeTween;
        [SerializeField] GameObject noAgentNotice;
        [SerializeField] UILabelHelper noAgentNoticeLabel;
        [SerializeField] GameObject rankSelectPanel;
        [SerializeField] UIButton rankSelectPanelClose;
        [SerializeField] UIButton[] rankButtons;
        [SerializeField] UISprite bulkSelectButtonArrow;

        private AgentComposePresenter presenter;
        private List<AgentSlotViewInfo> agentSlotViewInfos = new List<AgentSlotViewInfo>();
        private int curComposeCount;

        public void OnInit(AgentComposePresenter presenter)
        {
            this.presenter = presenter;
            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
            CloseRankSelectPanel();

            for (int i = 0; i < materialSlots.Length; ++i)
            {
                materialSlots[i].Init(i);
                materialSlots[i].AddEvent();
                materialSlots[i].OnClick += OnClickMaterialSlot;
            }

            for (int i = 0; i < rankButtons.Length; ++i)
                EventDelegate.Add(rankButtons[i].onClick, OnClickRankButton);
            EventDelegate.Add(rankSelectPanelClose.onClick, CloseRankSelectPanel);

            curComposeCount = 0;
            composeCountLabel.Text = $"x {curComposeCount}";
            EventDelegate.Add(bulkSelectButton.OnClick, OnClickBulkSelect);
            EventDelegate.Add(composeButton.OnClick, OnClickComposeButton);

            bulkSelectButtonArrow.flip = UIBasicSprite.Flip.Vertically;
        }

        public void OnClose()
        {
            for (int i = 0; i < materialSlots.Length; ++i)
            {
                materialSlots[i].RemoveEvent();
                materialSlots[i].OnClick -= OnClickMaterialSlot;
            }

            for (int i = 0; i < rankButtons.Length; ++i)
                EventDelegate.Remove(rankButtons[i].onClick, OnClickRankButton);
            EventDelegate.Remove(rankSelectPanelClose.onClick, CloseRankSelectPanel);

            EventDelegate.Remove(bulkSelectButton.OnClick, OnClickBulkSelect);
            EventDelegate.Remove(composeButton.OnClick, OnClickComposeButton);
        }

        public void OnLocalize()
        {
            bulkSelectButton.LocalKey = LocalizeKey._47334;
            composeButton.LocalKey = LocalizeKey._47304;
            noAgentNoticeLabel.LocalKey = LocalizeKey._47311;
        }

        public void ShowAgents(IAgent[] agents)
        {
            agentSlotViewInfos.Clear();

            for (int i = 0; i < agents.Length; ++i)
            {
                var agentSlotViewInfo = new AgentSlotViewInfo(agents[i].AgentData, agents[i]);
                agentSlotViewInfo.ShowNoDuplicationMask = true;
                agentSlotViewInfo.HideDuplicationCountOnZero = false;
                agentSlotViewInfo.ShowAgentStatePanel = false;
                agentSlotViewInfo.ShowAgentTypePanel = true;
                agentSlotViewInfo.ShowAgentNewIcon = false;
                agentSlotViewInfo.OnClick += OnClickAgentSlot;
                agentSlotViewInfos.Add(agentSlotViewInfo);
            }

            agentSlotViewInfos.Sort((a, b) =>
            {
                if (a.AgentData.agent_rating != b.AgentData.agent_rating)
                    return b.AgentData.agent_rating - a.AgentData.agent_rating;
                else
                    return b.AgentData.id - a.AgentData.id;
            });

            wrapper.Resize(agentSlotViewInfos.Count);
            noAgentNotice.SetActive(agents.Length == 0);
        }

        private void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<UIAgentSlot>();
            slot.SetData(agentSlotViewInfos[index]);
        }

        private void OnClickAgentSlot(IAgent agent)
        {
            presenter.ViewEventHandler(EventType.OnClickAgent, agent);
        }

        private void OnClickRankButton()
        {
            int index = UIButton.current.transform.GetSiblingIndex();
            int rank = index + 1;
            presenter.ViewEventHandler(EventType.OnClickBulkSelectRank, rank);
            CloseRankSelectPanel();
        }

        private void OnClickBulkSelect()
        {
            rankSelectPanel.SetActive(true);
            bulkSelectButtonArrow.flip = UIBasicSprite.Flip.Nothing;
        }

        private void CloseRankSelectPanel()
        {
            rankSelectPanel.SetActive(false);
            bulkSelectButtonArrow.flip = UIBasicSprite.Flip.Vertically;
        }

        private void OnClickComposeButton()
        {
            presenter.ViewEventHandler(EventType.OnClickCompose, null);
        }

        public void SetComposeCount(int count)
        {
            if (curComposeCount != count)
            {
                countChangeTween.PlayForward();
                countChangeTween.ResetToBeginning();
                curComposeCount = count;
                composeCountLabel.Text = $"x {curComposeCount}";
            }
        }

        public void UpdateNoticeText(int curSelectedAgentCount)
        {
            var sb = StringBuilderPool.Get();

            if (curSelectedAgentCount == 0)
            {
                sb.AppendLine(LocalizeKey._47315.ToText()); // 합성할 동료를 선택해주세요.
            }
            else if(curSelectedAgentCount % 3 == 0)
            {
                sb.AppendLine(LocalizeKey._47316.ToText()); // 합성 시 동일 랭크 혹은 한 단계 높은 랭크의 동료를 획득합니다.
            }
            else
            {
                sb.AppendLine(LocalizeKey._47317.ToText()); // [69B2E6][c]등록된 동료와 동일한 랭크의 동료를 선택해주세요.[/c][-]
            }

            if (GameServerConfig.IsKoreaLanguage())
            {
                noticeLabel.Text = BasisUrl.KoreanAgentCompose.AppendText(sb.Release(), useColor: true);
            }
            else
            {
                noticeLabel.Text = sb.Release();
            }
        }

        public void SetComposeButtonView(int zeny, bool active)
        {
            composeButton.SetCostCount(zeny.ToString("n0"));
            composeButton.IsEnabled = active;
        }

        public void SetMaterialSlotAgent(int index, IAgent agent, bool isNew = true)
        {
            materialSlots[index].SetSlot(agent, isNew);
        }

        private void OnClickMaterialSlot(int index)
        {
            presenter.ViewEventHandler(EventType.OnClickMaterialSlot, index);
        }

        public void UpdateAgentCount(int agentID, int count)
        {
            var info = agentSlotViewInfos.FirstOrDefault(v => v.OwningAgent.ID == agentID);
            if (info != null)
            {
                info.DuplicationCount = count;
                info.Update();
            }
        }
    }
}