using UnityEngine;

namespace Ragnarok
{
    public class UIGuildRankInfo : MonoBehaviour, IAutoInspectorFinder
    { 
        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시

        [SerializeField] UISprite rankIcon;
        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UIGuildEmblemInfo guildEmblem;
        [SerializeField] UILabelHelper labelMaster;
        [SerializeField] UILabelHelper labelExp;
        [SerializeField] UILabelHelper labelMemberCount;
        [SerializeField] UILabelHelper labelName;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void SetData(RankInfo info)
        {
            if (info == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            bool isTopRank = info.Rank <= TOP_RANK;

            if (isTopRank)
            {
                rankIcon.enabled = true;
                rankIcon.spriteName = $"Ui_Common_Icon_Rank_0{info.Rank}";
                labelRank.Text = string.Empty;
            }
            else
            {
                rankIcon.enabled = false;
                labelRank.Text = LocalizeKey._32008.ToText()
                    .Replace(ReplaceKey.RANK, info.Rank.ToString()); // {RANK}등
            }

            guildEmblem.SetData(info.EmblemBg, info.EmblemFrame, info.EmblemIcon);
            labelMaster.Text = info.GuildMasterName;
            labelExp.Text = info.Score.ToString("N0");
            labelMemberCount.Text = string.Concat(info.CurMemberCount.ToString(), "/", info.MaxMemberCount.ToString());
            labelName.Text = $"Lv. {info.GuildLevel} {info.GuildName}";
        }

        private void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }
    }
}