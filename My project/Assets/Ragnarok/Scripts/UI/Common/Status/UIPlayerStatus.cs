using Ragnarok.View.BattleStage;
using UnityEngine;

namespace Ragnarok.View
{
    public class UIPlayerStatus : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UICharacterStatus character;
        [SerializeField] UIBattleStageAgentSlot[] agentSlots;

        public void ResetData()
        {
            character.ResetData();

            for (int i = 0; i < agentSlots.Length; i++)
            {
                agentSlots[i].ResetData();
            }
        }

        public void SetData(CharacterEntity entity)
        {
            character.SetData(entity);
        }

        public void SetAgents(CharacterEntity[] agents)
        {
            for (int i = 0; i < agentSlots.Length; i++)
            {
                if (agents is null)
                {
                    agentSlots[i].SetData(null);
                }
                else
                {
                    agentSlots[i].SetData(i < agents.Length ? agents[i] : null);
                }
            }
        }

        bool IInspectorFinder.Find()
        {
            agentSlots = GetComponentsInChildren<UIBattleStageAgentSlot>();
            return true;
        }
    }
}