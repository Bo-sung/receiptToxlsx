using UnityEngine;

namespace Ragnarok
{
    public class UIMonsterRewardSlot : MonoBehaviour
    {
        [SerializeField] UIRewardHelper reward;
        [SerializeField] GameObject iconBoss;

        public void Set((RewardInfo reward, bool isBoss) slotData)
        {
            if (slotData.reward == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            reward.SetData(slotData.reward.data);
            iconBoss.SetActive(slotData.isBoss);
        }
    }
}