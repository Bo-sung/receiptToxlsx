using UnityEngine;

namespace Ragnarok
{
    public sealed class UITamingResultPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;

        [SerializeField] UITextureHelper iconMonster;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelGuildContributionPoint;

        [SerializeField] UILabelHelper labelGuildRewardTitle;
        [SerializeField] UIRewardHelper guildReward;
        [SerializeField] GameObject goNoGuildBase;
        [SerializeField] UILabelHelper labelNoGuild;

        [SerializeField] UILabelHelper labelMyRewardTitle;
        [SerializeField] UIRewardHelper myReward;

        [SerializeField] UIButtonHelper btnClose;

        protected override void OnInit()
        {

            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnClose.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnClose.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void Show(TamingMazeEntry.TamingResult result, string monsterIcon, RewardData guildReward, RewardData myReward, int guildContributionPoint)
        {
            base.Show();

            if (result == TamingMazeEntry.TamingResult.Fail)
            {
                CloseUI();
                return;
            }

            iconMonster.Set(monsterIcon); // 아이콘

            // 성공/대성공
            if (result == TamingMazeEntry.TamingResult.GreatSuccess)
            {
                labelDescription.Text = LocalizeKey._33202.ToText(); // 테이밍 [c][i][5080E3]대성공[-][/i][/c]
            }
            else
            {
                labelDescription.Text = LocalizeKey._33201.ToText(); // 테이밍 [c][i][4DB2F9]성공[-][/i][/c]
            }

            // 길드 기여도
            labelGuildContributionPoint.Text = LocalizeKey._33207.ToText().Replace(ReplaceKey.VALUE, guildContributionPoint); // 길드 기여도 + {VALUE}

            // 길드 보상
            bool hasGuild = guildReward != null;
            goNoGuildBase.SetActive(!hasGuild);
            this.guildReward.gameObject.SetActive(hasGuild);
            if (hasGuild)
            {
                this.guildReward.SetData(guildReward);
            }

            // 개인 보상
            this.myReward.SetData(myReward);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33200; // 테이밍 결과
            labelGuildRewardTitle.LocalKey = LocalizeKey._33203; // 길드 보상
            labelMyRewardTitle.LocalKey = LocalizeKey._33204; // 내 보상
            labelNoGuild.LocalKey = LocalizeKey._33206; // 길드에서 추방 되어 길드 보상을 받을 수 없습니다.
            btnClose.LocalKey = LocalizeKey._33205; // 확인
        }

        private void CloseUI()
        {
            UI.Close<UITamingResultPopup>();
        }
    }
}