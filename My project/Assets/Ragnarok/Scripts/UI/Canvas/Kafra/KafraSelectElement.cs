using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class KafraSelectElement : UIView
    {
        [SerializeField] private KafraType kafraType;
        [SerializeField] private UILabelHelper labelTitle;
        [SerializeField] private UILabelHelper labelDesc;
        [SerializeField] private UILabelHelper labelReward;
        [SerializeField] private UIRewardHelper rewardHelper;
        [SerializeField] private UIButton btnSelect;

        public event Action<KafraType> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.onClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnSelect.onClick, OnClickedBtnSelect);
        }

        protected override void OnLocalize()
        {
            labelReward.LocalKey = LocalizeKey._19102; // 보상

            if (kafraType == KafraType.RoPoint)
            {
                labelTitle.LocalKey = LocalizeKey._19103; // 귀금속 전달
                labelDesc.LocalKey = LocalizeKey._19104; // 귀금속 전달 설명
            }
            else if (kafraType == KafraType.Zeny)
            {
                labelTitle.LocalKey = LocalizeKey._19105; // 긴급! 도움 요청
                labelDesc.LocalKey = LocalizeKey._19106; // 긴급! 도움 요청 설명
            }
        }

        public override void Show()
        {
            base.Show();
            
            if (kafraType == KafraType.RoPoint)
            {
                rewardHelper.SetData(new RewardData(RewardType.ROPoint, 0, 0));
            }
            else if (kafraType == KafraType.Zeny)
            {
                rewardHelper.SetData(new RewardData(RewardType.Zeny, 0, 0));
            }
        }

        void OnClickedBtnSelect()
        {
            OnSelect?.Invoke(kafraType);
        }
    }
}