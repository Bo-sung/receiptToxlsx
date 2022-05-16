using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRewardDuel : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void OnInit()
        {
            EventDelegate.Add(btnConfirm.OnClick, OnBack);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._30600; // 듀얼 보상
            btnConfirm.LocalKey = LocalizeKey._30601; // 확 인
        }

        public void SetReward(RewardData[] rewardData, string name, string desc)
        {
            labelName.Text = name;
            labelDesc.Text = desc;
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < rewardData.Length ? rewardData[i] : null);
            }
            rewardGrid.repositionNow = true;
            Show();

            SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess);
        }

        void CloseUI()
        {
            UI.Close<UIRewardDuel>();
        }
    }
}