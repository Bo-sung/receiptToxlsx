using UnityEngine;

namespace Ragnarok.View.League
{
    public class UILeagueMyRank : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput : UILeagueRankBar.IInput
        {
            string TierName { get; }
            int WinCount { get; }
            int LoseCount { get; }
        }

        [SerializeField] UITextureHelper iconTier;
        [SerializeField] UILabelHelper labelTier;
        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabelHelper labelName, labelPower, labelJobLevel, labelHistory, labelScore;
        [SerializeField] UIButton profileButton;

        private void Start()
        {
            EventDelegate.Add(profileButton.onClick, OnClickProfile);
        }

        private void OnDestroy()
        {
            EventDelegate.Remove(profileButton.onClick, OnClickProfile);
        }

        public void SetData(IInput input)
        {
            labelTier.Text = input.TierName;

            string rankText;
            if (input.Score > 0)
            {
                rankText = LocalizeKey._47007.ToText() // {RANK}위
                    .Replace(ReplaceKey.RANK, input.Ranking);
            }
            else
            {
                rankText = LocalizeKey._47030.ToText(); // 순위밖
            }

            labelRank.Text = StringBuilderPool.Get()
                .Append(input.TierName).Append(" - ").Append(rankText)
                .Release();

            iconJob.Set(input.ProfileName);
            labelName.Text = StringBuilderPool.Get()
                .Append(input.Name)
                .Append("[c]").Append("[908F90]").Append(" (").Append(input.CidHex).Append(")").Append("[-]").Append("[/c]")
                .Release();

            labelPower.Text = input.Power.ToString();
            labelJobLevel.Text = LocalizeKey._47033.ToText().Replace(ReplaceKey.LEVEL, input.JobLevel.ToString()); // 직업 레벨 : Lv {LEVEL}
            iconTier.SetRankTier(input.TierIconName);
            labelHistory.Text = LocalizeKey._47008.ToText() // {WIN}승 {LOSE}패
                .Replace(ReplaceKey.WIN, input.WinCount)
                .Replace(ReplaceKey.LOSE, input.LoseCount);

            labelScore.Text = input.Score.ToString();
        }


        private void OnClickProfile()
        {
            Entity.player.User.RequestOtherCharacterInfo(Entity.player.User.UID, Entity.player.Character.Cid).WrapNetworkErrors();
        }
    }
}