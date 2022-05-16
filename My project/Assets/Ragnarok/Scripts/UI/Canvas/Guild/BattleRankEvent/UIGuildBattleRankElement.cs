using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="GuildBattleRankView"/>
    /// </summary>
    public class UIGuildBattleRankElement : UIElement<UIGuildBattleRankElement.IInput>
    {
        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시

        public interface IInput
        {
            long Rank { get; }
            int EmblemBg { get; }
            int EmblemFrame { get; }
            int EmblemIcon { get; }
            string GuildMasterName { get; }
            double Score { get; }
            int GuildLevel { get; }
            string GuildName { get; }
        }

        [SerializeField] UISprite rankIcon;
        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UIGuildEmblemInfo guildEmblem;
        [SerializeField] UILabelHelper labelMaster;
        [SerializeField] UILabelHelper labelScore;
        [SerializeField] UILabelHelper labelName;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
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
            labelScore.Text = info.Score.ToString("N0");
            labelName.Text = StringBuilderPool.Get()
                .Append("Lv. ").Append(info.GuildLevel).Append(" ").Append(info.GuildName)
                .Release();
        }
    }
}