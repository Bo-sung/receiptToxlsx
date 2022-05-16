using UnityEngine;

namespace Ragnarok
{
    public class UIDuelCombatAngetProfileSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper profileSprite;
        [SerializeField] UIButton button;
        [SerializeField] UILabelHelper labelLock;

        private void Start()
        {
            EventDelegate.Add(button.onClick, OnClick);
        }

        public void SetData(CombatAgent combatAgent, bool isValidSlot)
        {
            if (combatAgent != null)
            {
                button.gameObject.SetActive(false);
                profileSprite.gameObject.SetActive(true);
                profileSprite.Set(combatAgent.AgentData.GetIconName(AgentIconType.CircleIcon));

                if (labelLock)
                    labelLock.SetActive(false);
            }
            else
            {
                button.gameObject.SetActive(isValidSlot);
                profileSprite.gameObject.SetActive(false);

                if (labelLock)
                    labelLock.SetActive(!isValidSlot);
            }
        }

        public void SetLabelLock(int idx)
        {
            if (labelLock == null)
                return;

            switch(idx)
            {
                case 1:
                    labelLock.LocalKey = LocalizeKey._2027; // 1차 전직
                    break;

                case 2:
                    labelLock.LocalKey = LocalizeKey._2028; // 2차 전직
                    break;

                case 3:
                    labelLock.LocalKey = LocalizeKey._2016; // 3차 전직
                    break;
            }
        }

        private void OnClick()
        {
            UI.Show<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.CombatAgent });
        }
    }
}