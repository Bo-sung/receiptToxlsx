using UnityEngine;

namespace Ragnarok.View
{
    public class AgentSelectView : UIView, IInspectorFinder
    {
        [SerializeField] UIButtonHelper btnAgent;
        [SerializeField] UIDuelCombatAngetProfileSlot[] combatAgentSlots;

        public event System.Action OnSelectAgent;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnAgent.OnClick, OnClickedBtnAgent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnAgent.OnClick, OnClickedBtnAgent);
        }

        protected override void OnLocalize()
        {
            btnAgent.LocalKey = LocalizeKey._47032; // 동료 장착
        }

        void OnClickedBtnAgent()
        {
            OnSelectAgent?.Invoke();
        }

        /// <summary>
        /// 전투동료 세팅
        /// </summary>
        public void SetCombatAgents(CombatAgent[] combatAgents, int validSlotCount)
        {
            for (int i = 0; i < combatAgentSlots.Length; ++i)
            {
                combatAgentSlots[i].SetLabelLock(i); // 잠금라벨 셋팅

                if (i < combatAgents.Length)
                    combatAgentSlots[i].SetData(combatAgents[i], i < validSlotCount);
                else
                    combatAgentSlots[i].SetData(null, i < validSlotCount);
            }
        }

        /// <summary>
        /// 전투동료 알림 업데이트
        /// </summary>
        public void UpdateNotice(bool isNotice)
        {
            btnAgent.SetNotice(isNotice);
        }

        bool IInspectorFinder.Find()
        {
            combatAgentSlots = GetComponentsInChildren<UIDuelCombatAngetProfileSlot>();
            return true;
        }
    }
}