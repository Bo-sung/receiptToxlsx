using UnityEngine;
using System.Collections;
using System;

namespace Ragnarok
{
    public class UIBookLevelInfoPanel : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIGrid lastLevelGrid;
        [SerializeField] UILabelHelper lastLevelLabel;
        [SerializeField] UILabelHelper lastOptionLabel;

        [SerializeField] GameObject nextLevelInfoRoot;
        [SerializeField] UILabelHelper nextLevelLabel;
        [SerializeField] UILabelHelper nextOptionLabel;
        
        [SerializeField] UILabelHelper countLabel;
        [SerializeField] UILabelHelper[] etcLabels;

        [SerializeField] UIProgressBar progress;

        [SerializeField] UIButtonHelper levelUpButton;
        [SerializeField] UIButtonHelper rewardDetailButton;

        public event Action OnClickRewardsDetail;
        public event Action OnClickLevelUp;

        private string titleText;

        private void Start()
        {
            EventDelegate.Add(levelUpButton.OnClick, InvokeOnClickLevelUp);
            EventDelegate.Add(rewardDetailButton.OnClick, InvokeOnClickRewardDetail);
        }
        
        public void LocalizeText(string titleText, string countPanelTitleText)
        {
            this.titleText = titleText;
            etcLabels[0].Text = countPanelTitleText;
            etcLabels[1].Text = LocalizeKey._40209.ToText(); // 다음 레벨이 없습니다.\n업데이트를 기대해 주세요!

            levelUpButton.Text = LocalizeKey._40210.ToText(); // 레벨업
        }

        public void SetLevelInfo(BookData lastRewardData, BookData nextRewardData, int curCount)
        {
            int baseCount = 0;

            if (lastRewardData != null)
            {
                lastLevelLabel.Text = LocalizeKey._40211.ToText().Replace(ReplaceKey.NAME, titleText).Replace(ReplaceKey.LEVEL, lastRewardData.Level); // [c][6F6C72]{NAME}[-] [4997EE]Lv.{LEVEL}[-][/c];
                lastOptionLabel.Text = LocalizeKey._40213.ToText().Replace(ReplaceKey.VALUE, lastRewardData.GetOption().GetDescription()); // [+] {VALUE};
                lastOptionLabel.SetActive(true);
                baseCount = lastRewardData.score;
            }
            else
            {
                lastLevelLabel.Text = LocalizeKey._40211.ToText().Replace(ReplaceKey.NAME, titleText).Replace(ReplaceKey.LEVEL, 0); // [c][6F6C72]{NAME}[-] [4997EE]Lv.{LEVEL}[-][/c];
                lastOptionLabel.SetActive(false);
            }
            lastLevelGrid.repositionNow = true;

            int nextCount = 0;

            if (nextRewardData != null)
            {
                nextLevelLabel.Text = LocalizeKey._40212.ToText().Replace(ReplaceKey.LEVEL, nextRewardData.Level); // [c][4997EE]Lv.{LEVEL}[-] [6F6C72]도달 시[-][/c]
                nextOptionLabel.Text = LocalizeKey._40213.ToText().Replace(ReplaceKey.VALUE, nextRewardData.GetOption().GetDescription());
                nextLevelInfoRoot.SetActive(true);
                etcLabels[1].SetActive(false);
                nextCount = nextRewardData.score;
            }
            else
            {
                nextLevelInfoRoot.SetActive(false);
                etcLabels[1].SetActive(true);
            }

            if (nextRewardData == null)
            {
                countLabel.Text = LocalizeKey._40214.ToText(); // MAX
                progress.value = 1;
            }
            else
            {
                countLabel.Text = LocalizeKey._40215.ToText().Replace(ReplaceKey.VALUE, 1, curCount.ToString()).Replace(ReplaceKey.VALUE, 2, nextCount.ToString()); // [c][FFC600]{VALUE1}[-][/c]/{VALUE2}
                progress.value = (float)Mathf.Clamp01(curCount / (float)nextCount);
            }

            BookData progressRefData = nextRewardData;
            if (progressRefData == null)
                progressRefData = lastRewardData;

            levelUpButton.IsEnabled = nextRewardData != null && curCount >= nextRewardData.score;
        }

        private void InvokeOnClickLevelUp()
        {
            OnClickLevelUp?.Invoke();
        }

        private void InvokeOnClickRewardDetail()
        {
            OnClickRewardsDetail?.Invoke();
        }
    }
}