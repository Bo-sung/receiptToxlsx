using UnityEngine;

namespace Ragnarok
{
    public class UIBookLevelPopupSlot : MonoBehaviour
    {
        public enum OpenState { Opened, Challenging, Locked }

        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UILabelHelper levelLabel;
        [SerializeField] UILabelHelper optionLabel;
        [SerializeField] GameObject challengingOrLockedMask;
        [SerializeField] UILabelHelper progressLabel;

        public void SetData(BookTabType tabType, int curCount, BookData rewardData, BookData prevRewardData, OpenState openState)
        {
            icon.Set(rewardData.level_img);

            switch (tabType)
            {
                case BookTabType.Equipment:
                    titleLabel.Text = LocalizeKey._40201.ToText(); // 장비 도감
                    break;
                case BookTabType.Card:
                    titleLabel.Text = LocalizeKey._40203.ToText(); // 카드 도감
                    break;
                case BookTabType.Monster:
                    titleLabel.Text = LocalizeKey._40205.ToText(); // 몬스터 도감
                    break;
                case BookTabType.Costume:
                    titleLabel.Text = LocalizeKey._40207.ToText(); // 코스튬 도감
                    break;
                case BookTabType.Special:
                    titleLabel.Text = LocalizeKey._40238.ToText(); // 스페셜 도감
                    break;
            }

            levelLabel.Text = $"Lv.{rewardData.Level}";
            optionLabel.Text = LocalizeKey._40213.ToText().Replace(ReplaceKey.VALUE, rewardData.GetOption().GetDescription()); // [+] {VALUE}

            progressLabel.SetActive(false);

            if (openState == OpenState.Challenging)
            {
                int baseCount = prevRewardData == null ? 0 : (int)prevRewardData.score;
                int nextCount = rewardData.score;

                int diff = nextCount - baseCount;

                progressLabel.SetActive(true);
                progressLabel.Text = LocalizeKey._40215.ToText().Replace(ReplaceKey.VALUE, 1, curCount.ToString()).Replace(ReplaceKey.VALUE, 2, nextCount.ToString()); // [c][FFC600]{VALUE1}[-][/c]/{VALUE2}
            }

            challengingOrLockedMask.SetActive(openState != OpenState.Opened);
        }
    }
}