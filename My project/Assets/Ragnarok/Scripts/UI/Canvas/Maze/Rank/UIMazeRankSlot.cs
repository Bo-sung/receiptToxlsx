using UnityEngine;

namespace Ragnarok
{
    public class UIMazeRankSlot : MonoBehaviour, IAutoInspectorFinder
    {
        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시
        [SerializeField] UISprite rankIcon;
        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelTime;
        [SerializeField] GameObject activeObject;

        public void Set(int rank, string name, double time)
        {
            bool isTopRank = rank <= TOP_RANK;
            if (isTopRank)
            {
                rankIcon.enabled = true;
                rankIcon.spriteName = $"Ui_Common_Icon_Rank_0{rank}";
                labelRank.Text = string.Empty;
            }
            else
            {
                rankIcon.enabled = false;
                labelRank.Text = rank.ToString();
            }
            labelName.Text = name;
            if(time == 0)
            {
                labelTime.Text = "-";
            }
            else
            {
                var span = ((float)time).ToTimeSpan();
                if(span.TotalSeconds >= 3600)
                {
                    labelTime.Text = "60:00:00";
                }
                else
                {
                    labelTime.Text = ((float)time).ToStringTime(@"mm\:ss\:ff");
                }            
            }
        }

        public void SetActive(bool isActive)
        {
            activeObject.SetActive(isActive);
        }
    } 
}
