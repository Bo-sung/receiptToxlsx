using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class AdventureRankingElement : UIElement<AdventureRankingElement.IInput>
    {
        public interface IInput
        {
            long Rank { get; }
            int UID { get; }
            int CID { get; }
            string CharName { get; }
            Job Job { get; }
            Gender Gender { get; }
            string CIDHex { get; }
            int JobLevel { get; }
            int BattleScore { get; }
            string Description { get; }
            string ProfileName { get; }
        }

        public delegate void SelectCharacterEvent(int uid, int cid);

        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시
        private const string TOP_RANK_ICON_FORMAT = "Ui_Common_Icon_Rank_{0:D2}";

        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] UILabelHelper labelName, labelJobLevel, labelBattleScore;
        [SerializeField] UILabelHelper labelRankValue, labelRank;
        [SerializeField] UISprite iconRank;
        [SerializeField] UIButtonHelper btnSelect;

        public event SelectCharacterEvent OnSelect;

        protected override void Awake()
        {
            base.Awake();

            if (btnSelect)
            {
                EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (btnSelect)
            {
                EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
            }
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.UID, info.CID);
        }

        protected override void OnLocalize()
        {
            if (info == null)
            {
                labelName.Text = string.Empty;
                labelJobLevel.Text = string.Empty;
                labelRankValue.Text = string.Empty;
                return;
            }

            const string NAME_FORMAT = "[5A575B]{NAME}[-] [BEBEBE]({VALUE})[-]";
            labelName.Text = NAME_FORMAT
                .Replace(ReplaceKey.NAME, info.CharName)
                .Replace(ReplaceKey.VALUE, info.CIDHex);

            labelJobLevel.Text = LocalizeKey._47925.ToText() // JOB Lv.{LEVEL}
                .Replace(ReplaceKey.LEVEL, info.JobLevel);

            labelRankValue.Text = info.Description;
        }

        protected override void Refresh()
        {
            iconJob.Set(info.Job.GetJobIcon());
            thumbnail.Set(info.ProfileName);
            labelBattleScore.Text = info.BattleScore.ToString();
            SetRank(info.Rank);

            OnLocalize();
        }

        private void SetRank(long rank)
        {
            if (rank > TOP_RANK) // 3위 밖
            {
                NGUITools.SetActive(iconRank.cachedGameObject, false);

                labelRank.SetActive(true);
                labelRank.Text = LocalizeKey._47914.ToText() // {RANK}위
                    .Replace(ReplaceKey.RANK, rank);
            }
            else if (rank > 0) // 3위 안
            {
                NGUITools.SetActive(iconRank.cachedGameObject, true);
                iconRank.spriteName = string.Format(TOP_RANK_ICON_FORMAT, rank);

                labelRank.SetActive(false);
            }
            else // 순위 밖
            {
                NGUITools.SetActive(iconRank.cachedGameObject, false);

                labelRank.SetActive(true);
                labelRank.Text = "-";
            }
        }
    }
}