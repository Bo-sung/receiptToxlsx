using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIDuelArenaHistoryElement : UIElement<UIDuelArenaHistoryElement.IInput>
    {
        /// <summary>
        /// UIDuelHistorySlot
        /// CharDuelHistory
        /// </summary>
        public interface IInput
        {
            int TargetUID { get; }
            int TargetCID { get; }
            string TargetName { get; }
            Job TargetJob { get; }
            byte Result { get; }
            int BattleScore { get; }
            int WinCount { get; }
            int DefeatCount { get; }
            System.DateTime InsertDate { get; }
            int JobLevel { get; }
            string ProfileName { get; }
        }

        [SerializeField] UIButtonWithIconHelper btnProfile;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelBattleScore;
        [SerializeField] UILabelHelper labelRecord;
        [SerializeField] UILabelHelper labelHistory;
        [SerializeField] UILabelHelper labelDate;
        [SerializeField] UILabelHelper labelAttackWin;
        [SerializeField] UILabelHelper labelDefenseWin;
        [SerializeField] UILabelHelper labelAttackDefeat;
        [SerializeField] UILabelHelper labelDefenseDefeat;

        public event UserModel.UserInfoEvent OnSelectUserInfo;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnProfile.OnClick, OnClickedBtnProfile);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnProfile.OnClick, OnClickedBtnProfile);
        }

        protected override void OnLocalize()
        {
            labelAttackWin.LocalKey = LocalizeKey._47803; // 공격 성공
            labelDefenseWin.LocalKey = LocalizeKey._47805; // 방어 성공
            labelAttackDefeat.LocalKey = LocalizeKey._47804; // 공격 실패
            labelDefenseDefeat.LocalKey = LocalizeKey._47806; // 방어 실패
        }

        protected override void Refresh()
        {
            labelName.Text = $"Lv.{info.JobLevel} {info.TargetName} [c][BEBEBE]({MathUtils.CidToHexCode(info.TargetCID)})[-][/c]";
            labelBattleScore.Text = LocalizeKey._10204.ToText()
                .Replace(ReplaceKey.VALUE, info.BattleScore.ToString("n0"));
            labelRecord.Text = LocalizeKey._47812.ToText() // {WIN}승 / {LOSE}패
                .Replace(ReplaceKey.WIN, info.WinCount)
                .Replace(ReplaceKey.LOSE, info.DefeatCount);
            btnProfile.SetIconName(info.ProfileName);
            jobIcon.SetJobIcon(info.TargetJob.GetJobIcon());
            labelDate.Text = $"{info.InsertDate.Year}.{info.InsertDate.Month:D2}.{info.InsertDate.Day:D2}  {info.InsertDate.Hour:D2}:{info.InsertDate.Minute:D2}";

            const int NAME_LOCAL_KEY = LocalizeKey._47877; // 아레나 깃발
            labelAttackWin.SetActive(info.Result == 1 || info.Result == 2);
            labelAttackDefeat.SetActive(info.Result == 3);
            labelDefenseWin.SetActive(info.Result == 4);
            labelDefenseDefeat.SetActive(info.Result == 5 || info.Result == 6);

            if (info.Result == 1)
            {
                labelHistory.Text = LocalizeKey._47813.ToText() // 승리하여 「{NAME}」을 획득했습니다.
                    .Replace(ReplaceKey.NAME, NAME_LOCAL_KEY.ToText());
            }
            else if (info.Result == 2)
            {
                labelHistory.Text = LocalizeKey._47814.ToText() // 승리했지만, 「{NAME}」을 획득하진 못했습니다.
                    .Replace(ReplaceKey.NAME, NAME_LOCAL_KEY.ToText());
            }
            else if (info.Result == 3)
            {
                labelHistory.Text = LocalizeKey._47815.ToText() // 패배하여 「{NAME}」을 획득하지 못했습니다.
                    .Replace(ReplaceKey.NAME, NAME_LOCAL_KEY.ToText());
            }
            else if (info.Result == 4)
            {
                labelHistory.Text = LocalizeKey._47816.ToText() // 승리하여 「{NAME}」을 지켜냈습니다.
                    .Replace(ReplaceKey.NAME, NAME_LOCAL_KEY.ToText());
            }
            else if (info.Result == 5)
            {
                labelHistory.Text = LocalizeKey._47817.ToText() // 패배하여 「{NAME}」을 빼앗겼습니다.
                    .Replace(ReplaceKey.NAME, NAME_LOCAL_KEY.ToText());
            }
            else if (info.Result == 6)
            {
                labelHistory.Text = LocalizeKey._47818.ToText() // 패배했지만, 「{NAME}」을 빼앗기진 않았습니다.
                    .Replace(ReplaceKey.NAME, NAME_LOCAL_KEY.ToText());
            }
        }

        void OnClickedBtnProfile()
        {
            OnSelectUserInfo?.Invoke(info.TargetUID, info.TargetCID);
        }
    }
}