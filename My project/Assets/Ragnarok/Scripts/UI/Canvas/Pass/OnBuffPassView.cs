using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIOnBuffPass"/>
    /// </summary>
    public class OnBuffPassView : BasePassView
    {
        [SerializeField] GameObject arrow;

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelPassTitle.LocalKey = LocalizeKey._39818; // [OnBuff 시즌 제목]
            btnBuyPass.LocalKey = LocalizeKey._39819; // OnBuff 패스 구매
        }

        protected override void TurnOffNextRewardImage()
        {
            base.TurnOffNextRewardImage();

            arrow.SetActive(false);
        }

        public override void SetIsLastReward(bool isLastReward)
        {
            base.SetIsLastReward(isLastReward);

            if (isLastReward)
                TurnOffNextRewardImage();
        }
    }
}