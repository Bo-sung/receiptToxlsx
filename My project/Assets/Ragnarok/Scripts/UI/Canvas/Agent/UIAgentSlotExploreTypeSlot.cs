using UnityEngine;

namespace Ragnarok
{
    public class UIAgentSlotExploreTypeSlot : MonoBehaviour
    {
        [SerializeField] UISprite bgSprite;
        [SerializeField] UISprite exploreTypeIcon;
        [SerializeField] GameObject exploreFinishEffect;
        [SerializeField] Color iconColorOnDisabled;
        [SerializeField] Color bgColorOnDisabled;

        private float timeToEnd;
        private AgentListPresenter.ExploreSlotState curState;
        private ExploreType curExploreType;

        public void SetView(AgentListPresenter.ExploreSlotState state, ExploreType exploreType, float remainTime)
        {
            curState = state;
            curExploreType = exploreType;
            exploreTypeIcon.spriteName = exploreType.ToSpriteName();

            if (state == AgentListPresenter.ExploreSlotState.ExploreFinished)
            {
                exploreTypeIcon.color = iconColorOnDisabled;
                bgSprite.color = Color.white;
                if (exploreFinishEffect != null)
                    exploreFinishEffect.SetActive(true);
            }
            else if (state == AgentListPresenter.ExploreSlotState.Exploring)
            {
                timeToEnd = Time.realtimeSinceStartup + remainTime;

                exploreTypeIcon.color = iconColorOnDisabled;
                bgSprite.color = Color.white;
                if (exploreFinishEffect != null)
                    exploreFinishEffect.SetActive(false);
            }
            else if (state == AgentListPresenter.ExploreSlotState.NoExploring)
            {
                exploreTypeIcon.color = iconColorOnDisabled;
                bgSprite.color = Color.white;
                if (exploreFinishEffect != null)
                    exploreFinishEffect.SetActive(false);
            }
            else if (state == AgentListPresenter.ExploreSlotState.CantExplore)
            {
                exploreTypeIcon.color = iconColorOnDisabled;
                bgSprite.color = bgColorOnDisabled;
                if (exploreFinishEffect != null)
                    exploreFinishEffect.SetActive(false);
            }
        }

        void Update()
        {
            if (curState == AgentListPresenter.ExploreSlotState.Exploring && timeToEnd < Time.realtimeSinceStartup)
                SetView(AgentListPresenter.ExploreSlotState.ExploreFinished, curExploreType, 0);
        }
    }
}