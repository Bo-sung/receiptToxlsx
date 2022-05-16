using UnityEngine;

namespace Ragnarok
{
    public class UIBattleWorldBossRank : UICanvas
    {
        protected override UIType uiType => UIType.Hide;

        [SerializeField] UIWorldBossRankSlot[] worldBossRankSlots;
        [SerializeField] UIWorldBossRankSlot myRankSlot;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            ResetData();
        }

        protected override void OnHide()
        {
            ResetData();
        }

        protected override void OnLocalize()
        {
        }

        public void SetActiveWorldBossRankSlot(int index, bool isActive)
        {
            worldBossRankSlots[index].SetActive(isActive);
        }

        public void ShowMyRank(int rank, string name, float damage)
        {
            myRankSlot.SetActive(true);
            myRankSlot.Set(rank, name, damage);
        }

        public void SetWorldBossRankSlot(int index, int rank, string name, float damage)
        {
            SetActiveWorldBossRankSlot(index, isActive: true);
            worldBossRankSlots[index].Set(rank, name, damage);
        }

        private void ResetData()
        {
            for (int i = 0; i < worldBossRankSlots.Length; i++)
            {
                SetActiveWorldBossRankSlot(i, isActive: false);
            }

            myRankSlot.SetActive(false);
        }
    }
}