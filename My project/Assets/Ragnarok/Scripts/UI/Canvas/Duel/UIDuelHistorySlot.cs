using UnityEngine;

namespace Ragnarok
{
    public class UIDuelHistorySlot : MonoBehaviour
    {
        [SerializeField] UILabelHelper nameLabel;
        [SerializeField] UILabelHelper battleScoreLabel;
        [SerializeField] UILabelHelper recordLabel;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] GameObject[] resultDecos;
        [SerializeField] UILabelHelper resultLabel;
        [SerializeField] UILabelHelper dateLabel;
        [SerializeField] UILabel[] resultDecoLabels;
        [SerializeField] UIButtonHelper profileButton;
        [SerializeField] UITextureHelper profile;

        private CharDuelHistory curInfo;

        private void Start()
        {
            EventDelegate.Add(profileButton.OnClick, ShowDetail);
        }

        private void OnDestroy()
        {
            EventDelegate.Remove(profileButton.OnClick, ShowDetail);
        }

        private void ShowDetail()
        {
            if (curInfo != null)
            {
                Entity.player.User.RequestOtherCharacterInfo(curInfo.TargetUID, curInfo.TargetCID).WrapNetworkErrors();
            }
        }

        public void SetData(CharDuelHistory info)
        {
            curInfo = info;
            nameLabel.Text = $"Lv.{info.JobLevel} {info.TargetName} [c][BEBEBE]({MathUtils.CidToHexCode(info.TargetCID)})[-][/c]";
            battleScoreLabel.Text = LocalizeKey._10204.ToText()
                .Replace(ReplaceKey.VALUE, info.BattleScore.ToString("n0"));
            recordLabel.Text = LocalizeKey._47812.ToText() // {WIN}승 / {LOSE}패
                .Replace(ReplaceKey.WIN, info.WinCount)
                .Replace(ReplaceKey.LOSE, info.DefeatCount);
            profile.Set(info.ProfileName);
            jobIcon.Set(info.TargetJob.GetJobIcon());
            dateLabel.Text = $"{info.InsertDate.Year}.{info.InsertDate.Month:D2}.{info.InsertDate.Day:D2}  {info.InsertDate.Hour:D2}:{info.InsertDate.Minute:D2}";

            for (int i = 0; i < resultDecos.Length; ++i)
                resultDecos[i].SetActive(false);

            if (info.Result == 1)
            {
                resultLabel.Text = LocalizeKey._47813.ToText() // 승리하여 「{NAME}」을 획득했습니다.
                    .Replace(ReplaceKey.NAME, info.DuelPieceNameID.ToText());
                resultDecos[0].SetActive(true);
            }
            else if (info.Result == 2)
            {
                resultLabel.Text = LocalizeKey._47814.ToText() // 승리했지만, 「{NAME}」을 획득하진 못했습니다.
                    .Replace(ReplaceKey.NAME, info.DuelPieceNameID.ToText());
                resultDecos[0].SetActive(true);
            }
            else if (info.Result == 3)
            {
                resultLabel.Text = LocalizeKey._47815.ToText() // 패배하여 「{NAME}」을 획득하지 못했습니다.
                    .Replace(ReplaceKey.NAME, info.DuelPieceNameID.ToText());
                resultDecos[2].SetActive(true);
            }
            else if (info.Result == 4)
            {
                resultLabel.Text = LocalizeKey._47816.ToText() // 승리하여 「{NAME}」을 지켜냈습니다.
                    .Replace(ReplaceKey.NAME, info.DuelPieceNameID.ToText());
                resultDecos[1].SetActive(true);
            }
            else if (info.Result == 5)
            {
                resultLabel.Text = LocalizeKey._47817.ToText() // 패배하여 「{NAME}」을 빼앗겼습니다.
                    .Replace(ReplaceKey.NAME, info.DuelPieceNameID.ToText());
                resultDecos[3].SetActive(true);
            }
            else if (info.Result == 6)
            {
                resultLabel.Text = LocalizeKey._47818.ToText() // 패배했지만, 「{NAME}」을 빼앗기진 않았습니다.
                    .Replace(ReplaceKey.NAME, info.DuelPieceNameID.ToText());
                resultDecos[3].SetActive(true);
            }

            resultDecoLabels[0].text = LocalizeKey._47803.ToText();
            resultDecoLabels[1].text = LocalizeKey._47805.ToText();
            resultDecoLabels[2].text = LocalizeKey._47804.ToText();
            resultDecoLabels[3].text = LocalizeKey._47806.ToText();
        }
    }
}