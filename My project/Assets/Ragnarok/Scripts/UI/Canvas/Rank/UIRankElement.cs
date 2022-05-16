using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIRankElement : UIElement<UIRankElement.IInput>
    {
        public interface IInput
        {
            RankType RankType { get; }
            long Rank { get; }
            int UID { get; }
            int CID { get; }
            string CharName { get; }
            Job Job { get; }
            Gender Gender { get; }
            string CIDHex { get; }
            bool IsScore { get; }
            int JobLevel { get; }
            double Score { get; }
            string Description { get; }
            string ProfileName { get; }
        }

        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시
        [SerializeField] UISprite rankIcon;
        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UITextureHelper jobProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnSelect;

        public event System.Action<IInput> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            if (btnSelect)
                EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (btnSelect)
                EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            bool isTopRank = info.Rank <= TOP_RANK;

            if (isTopRank && info.Rank != -1)
            {
                rankIcon.enabled = true;
                rankIcon.spriteName = $"Ui_Common_Icon_Rank_0{info.Rank}";
                labelRank.Text = string.Empty;
            }
            else
            {
                rankIcon.enabled = false;
                labelRank.Text = info.Rank != -1 ? LocalizeKey._32008.ToText().Replace(ReplaceKey.RANK, info.Rank) : LocalizeKey._32012.ToText(); // {RANK}등 : -
            }

            jobProfile.Set(info.ProfileName);
            labelName.Text = $"{info.CharName}({info.CIDHex})";
            if (info.IsScore)
            {
                labelDesc.Text = info.Description;
            }
            else
            {
                labelDesc.LocalKey = LocalizeKey._32012; // -
            }
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info);
        }
    }
}