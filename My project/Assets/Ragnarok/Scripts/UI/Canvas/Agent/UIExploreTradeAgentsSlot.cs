using UnityEngine;
using System.Collections;
using System;

namespace Ragnarok
{
    public class UIExploreTradeAgentsSlot : MonoBehaviour
    {
        [SerializeField] GameObject[] stars;
        [SerializeField] UILabelHelper requiredTimeLabel;
        [SerializeField] UILabelHelper requiredTimePanelLabel;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper noAgentNoti; // 해당 랭크의 동료가 없습니다.

        private bool doInit = true;
        private UIExploreAgentSlot.Info[] infos;
        private Action<ExploreAgent> onClickAgent;

        public void SetData(int rating, StageData stageData, UIExploreAgentSlot.Info[] infos, Action<ExploreAgent> onClickAgent)
        {
            this.onClickAgent = onClickAgent;
            this.infos = infos;
            
            requiredTimeLabel.Text = stageData.GetExploreRequiredTime(rating).ToStringTime();
            var type = stageData.agent_explore_type.ToEnum<ExploreType>();
            string exploreTypeName = type.ToExploreName();
            requiredTimePanelLabel.Text = LocalizeKey._47403.ToText().Replace(ReplaceKey.NAME, exploreTypeName);

            noAgentNoti.Text = LocalizeKey._47407.ToText(); // 해당 랭크의 동료가 없습니다.
            noAgentNoti.SetActive(infos.Length == 0);

            for (int i = 0; i < stars.Length; ++i)
                stars[i].SetActive(i < rating);

            if (doInit)
            {
                doInit = false;
                wrapper.SpawnNewList(prefab, 0, 0);
                wrapper.SetRefreshCallback(OnElementRefresh);
            }

            wrapper.Resize(infos.Length);
            wrapper.SetProgress(0);
        }

        private void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<UIExploreAgentSlot>();
            slot.SetAgent(infos[index], OnClickAgent);
        }

        private void OnClickAgent(ExploreAgent exploreAgent)
        {
            onClickAgent(exploreAgent);
        }
    }
}