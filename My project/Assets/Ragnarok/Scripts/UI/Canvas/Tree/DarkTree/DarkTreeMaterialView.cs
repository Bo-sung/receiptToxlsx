using UnityEngine;

namespace Ragnarok.View
{
    public class DarkTreeMaterialView : SingleMaterialSelectView
    {
        [SerializeField] UIRewardHelper result;

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelNoData.LocalKey = LocalizeKey._9040; // 보유한 수확 재료가 없습니다.
            labelNotice.LocalKey = LocalizeKey._9036; // 수확에 필요한 재료를 선택하세요.
        }

        public void SetReward(RewardData rewardData, int curPoint, int maxPoint)
        {
            result.SetData(rewardData);

            SetPoint(curPoint, maxPoint);
        }
    }
}