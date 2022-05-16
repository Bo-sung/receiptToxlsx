using UnityEngine;

namespace Ragnarok.View.League
{
    public class UILeagueRankBar : UIElement<UILeagueRankBar.IInput>, IAutoInspectorFinder
    {
        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시
        private const string TOP_RANK_ICON_FORMAT = "Ui_Common_Icon_Rank_{0:D2}";

        public interface IInput
        {
            int UID { get; }
            int CID { get; }
            int Ranking { get; }
            Job Job { get; }
            Gender Gender { get; }
            string Name { get; }
            string CidHex { get; }
            int JobLevel { get; }
            int Power { get; }
            int Score { get; }
            string TierIconName { get; }
            string ProfileName { get; }
        }

        [SerializeField] UISprite iconRank;
        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabelHelper labelName, labelJobLevel, labelPower;
        [SerializeField] UITextureHelper iconTier;
        [SerializeField] UILabelHelper labelScore;
        [SerializeField] UIButton profileButton;

        public event System.Action<(int uid, int cid)> OnSelectProfile;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(profileButton.onClick, OnClickProfile);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Add(profileButton.onClick, OnClickProfile);       
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            int rank = info.Ranking;
            if (rank > TOP_RANK)
            {
                iconRank.cachedGameObject.SetActive(false);

                labelRank.SetActive(true);
                labelRank.Text = rank.ToString();
            }
            else if (info.Score > 0)
            {
                iconRank.cachedGameObject.SetActive(true);
                iconRank.spriteName = string.Format(TOP_RANK_ICON_FORMAT, rank);

                labelRank.SetActive(false);
            }
            else
            {
                iconRank.cachedGameObject.SetActive(false);

                labelRank.SetActive(true);
                labelRank.Text = "-"; // 순위밖의 경우
            }

            iconJob.Set(info.ProfileName);
            labelName.Text = StringBuilderPool.Get()
                .Append(info.Name)
                .Append("[c]").Append("[908F90]").Append(" (").Append(info.CidHex).Append(")").Append("[-]").Append("[/c]")
                .Release();

            labelPower.Text = info.Power.ToString();
            labelJobLevel.Text = LocalizeKey._47033.ToText().Replace(ReplaceKey.LEVEL, info.JobLevel.ToString()); // 직업 레벨 : Lv {LEVEL}
            iconTier.SetRankTier(info.TierIconName);
            labelScore.Text = info.Score.ToString();
        }        

        private void OnClickProfile()
        {
            if (info == null)
                return;

            OnSelectProfile?.Invoke((info.UID, info.CID));
        }
    }
}