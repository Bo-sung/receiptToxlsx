using UnityEngine;

namespace Ragnarok
{
    public class UIWorldBossRankSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDamage;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UISprite rankIcon;
        [SerializeField] UILabelHelper labelRank;

        public void Set(int rank, string name, int damage, int maxHp, RewardData reward)
        {
            SetRank(rank);
            labelName.Text = name;
            labelDamage.Text = $"{damage:N0}({damage / (float)maxHp:P0})";

            if (rewardHelper != null)
                rewardHelper.SetData(reward);
        }

        public void Set(int rank, string name, float damage)
        {
            SetRank(rank);
            labelName.Text = name;
            labelDamage.Text = $"{damage:P0}";
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        private void SetRank(int rank)
        {
            if (rank == -1)
            {
                if (rankIcon)
                {
                    NGUITools.SetActive(rankIcon.cachedGameObject, false);
                }

                if (labelRank)
                {
                    labelRank.SetActive(true);
                    labelRank.Text = "-";
                }
            }
            else if (rank <= 3)
            {
                if (rankIcon)
                {
                    NGUITools.SetActive(rankIcon.cachedGameObject, true);
                    rankIcon.spriteName = string.Concat("Ui_Common_Icon_Rank_", rank.ToString("00"));
                }

                if (labelRank)
                {
                    labelRank.SetActive(false);
                }
            }
            else
            {
                if (rankIcon)
                {
                    NGUITools.SetActive(rankIcon.cachedGameObject, false);
                }

                if (labelRank)
                {
                    labelRank.SetActive(true);
                    labelRank.Text = rank.ToString();
                }
            }
        }
    } 
}