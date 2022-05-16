using UnityEngine;

namespace Ragnarok.View
{
    public class DarkMazeView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIIconLabelValue info1;
        [SerializeField] UIIconLabelValue info2;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] UILabelHelper labelDailyReward;
        [SerializeField] UIRewardHelper dailyReward;
        [SerializeField] GameObject goComplete;
        [SerializeField] UILabelHelper labelClearReward;
        [SerializeField] UIRewardHelper clearReward;

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._39701; // 이벤트 미궁
            info1.TitleKey = LocalizeKey._39702; // 어두운 미로
            info1.ValueKey = LocalizeKey._39703; // 프론테라에 어둠이 찾아왔어요.\n시야를 밝혀주는 "루드"를 포획하여 어둠을 물리쳐보세요!
            info2.TitleKey = LocalizeKey._39704; // 어둠 속 실체
            info2.ValueKey = LocalizeKey._39705; // "루드"를 충분히 포획하면 어둠 속 실체를 볼 수 있을거에요.\n어둠 속 무언가를 잡아 실체를 밝혀냅시다!
            labelReward.LocalKey = LocalizeKey._39706; // 보상 리스트
            labelDailyReward.LocalKey = LocalizeKey._39707; // 매일 최초 보상
            labelClearReward.LocalKey = LocalizeKey._39708; // 클리어 보상
        }

        public void Initialize(RewardData dailyRewardData, RewardData clearRewardData, bool isReceivedDailyReward)
        {
            dailyReward.SetData(dailyRewardData);
            clearReward.SetData(clearRewardData);
            NGUITools.SetActive(goComplete, isReceivedDailyReward);
        }
    }
}