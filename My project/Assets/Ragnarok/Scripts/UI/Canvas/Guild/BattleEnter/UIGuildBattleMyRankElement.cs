using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIGuildBattleMyRankElement : UIElement<UIGuildBattleMyRankElement.IInput>
    {
        public interface IInput
        {
            long Rank { get; }
            int UID { get; }
            int CID { get; }
            string CharName { get; }
            string CIDHex { get; }
            string ProfileName { get; }
            double Score { get; }
            int BattleScore { get; }
        }

        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시

        [SerializeField] UISprite rankIcon;
        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UITextureHelper profile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelBattleScore;
        [SerializeField] UILabelHelper labelDamage;
        [SerializeField] UIButton btnInfo;

        public event System.Action<IInput> OnSelectBtnInfo;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnInfo.onClick, OnClickedBtnInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnInfo.onClick, OnClickedBtnInfo);
        }

        protected override void OnLocalize()
        {
        }

        void OnClickedBtnInfo()
        {
            if (info == null)
                return;

            OnSelectBtnInfo?.Invoke(info);
        }

        protected override void Refresh()
        {
            if (info.Rank <= TOP_RANK)
            {
                rankIcon.enabled = true;
                rankIcon.spriteName = StringBuilderPool.Get()
                    .Append("Ui_Common_Icon_Rank_0").Append(info.Rank)
                    .Release();

                labelRank.Text = string.Empty;
            }
            else
            {
                rankIcon.enabled = false;
                labelRank.Text = LocalizeKey._32008.ToText() // {RANK}등
                    .Replace(ReplaceKey.RANK, info.Rank);
            }

            profile.SetJobProfile(info.ProfileName, isAsync: false);
            labelName.Text = StringBuilderPool.Get()
                .Append(info.CharName).Append('(').Append(info.CIDHex).Append(')')
                .Release();

            labelBattleScore.Text = LocalizeKey._32021.ToText() // 전투력 : {VALUE}
                .Replace(ReplaceKey.VALUE, info.BattleScore.ToString("N0"));

            labelDamage.Text = info.Score.ToString("N0");
        }
    }
}