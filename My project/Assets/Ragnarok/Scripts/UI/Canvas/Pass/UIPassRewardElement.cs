using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="PassRewardView"/>
    /// </summary>
    public class UIPassRewardElement : UIBasePassRewardElement
    {
        [SerializeField] GameObject goLastReward;
        [SerializeField] UIButtonHelper btnBuyExp;
        [SerializeField] PassReward lastReward;
        [SerializeField] UILabelHelper labelTitle, labelDesc;
        [SerializeField] UILabelHelper labelCount;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnBuyExp.OnClick, InvokeSelectBuyExp);
            lastReward.OnSelectReceive += InvokeSelectReceiveFree;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnBuyExp.OnClick, InvokeSelectBuyExp);
            lastReward.OnSelectReceive -= InvokeSelectReceiveFree;
        }

        protected override void OnLocalize()
        {
            btnBuyExp.Text = BasisType.PASS_LEVEL_UP_CAT_COIN.GetInt().ToString();
            labelTitle.LocalKey = LocalizeKey._39813; // 라비린스 패스 완료 선물!
        }

        protected override void RefreshReward(bool isPreLastLevel, bool isLastLevel, bool isCurLevel, bool isOpenLevel)
        {
            goLastReward.SetActive(isLastLevel);
            freeReward.SetActive(!isLastLevel);
            passReward.SetActive(!isLastLevel);

            // 마지막 보상
            if (isLastLevel)
            {
                lastReward.SetRewardData(info.FreeRewards);
                labelLevel.Text = string.Empty;
                labelDesc.Text = info.FreeRewards[0].ItemName;
                labelCount.Text = info.LastRewardCount.ToString();
                labelCount.SetActive(info.LastRewardCount > 0);

                lastReward.SetBtnReceice(isCurLevel);
                lastReward.SetNotice(isCurLevel);
                lastReward.SetLock(false);
                lastReward.SetComplete(false);
                return;
            }

            base.RefreshReward(isPreLastLevel, isLastLevel, isCurLevel, isOpenLevel);
        }
    }
}