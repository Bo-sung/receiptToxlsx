using System;
using UnityEngine;

namespace Ragnarok
{
    public class UIAgentIconHelper : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper profileSprite;
        [SerializeField] Color silhouetteColor;
        [SerializeField] GameObject questionMark;
        [SerializeField] GameObject[] rankStars;
        [SerializeField] UIButton button;
        [SerializeField] UISprite iconAgent;

        public event Action<AgentData> OnClick;

        private Color oldColor;
        private AgentData agent;

        private void Awake()
        {
            oldColor = profileSprite.color;
            if (button != null)
                EventDelegate.Add(button.onClick, OnClickButton);
            if (questionMark != null)
                questionMark.SetActive(false);          
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        public void SetData(AgentData agent)
        {
            this.agent = agent;
            profileSprite.Set(agent.GetIconName(AgentIconType.RectIcon));
            for (int i = 0; i < rankStars.Length; ++i)
                rankStars[i].SetActive(i < agent.agent_rating);

            if(iconAgent != null)
            {
                iconAgent.spriteName = agent.agent_type.ToEnum<AgentType>().GetIconName();
            }
        }

        public void SetActiveSilhouette(bool value)
        {
            if (value)
            {
                profileSprite.color = silhouetteColor;
                if (questionMark != null)
                    questionMark.SetActive(true);
            }
            else
            {
                profileSprite.color = oldColor;
                if (questionMark != null)
                    questionMark.SetActive(false);
            }
        }

        private void OnClickButton()
        {
            OnClick?.Invoke(agent);
        }
    }
}