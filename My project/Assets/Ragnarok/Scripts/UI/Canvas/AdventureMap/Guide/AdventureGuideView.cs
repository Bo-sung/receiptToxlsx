using UnityEngine;

namespace Ragnarok.View
{
    public sealed class AdventureGuideView : UIView
    {
        [SerializeField] UITable table;
        [SerializeField] UIAdventureGuideTextInfo eventInfo, challengeInfo;
        [SerializeField] UIAdventureGuideRankInfo rankScore, rankKill;
        [SerializeField] UILabelHelper labelNotice;

        protected override void OnLocalize()
        {
            const int EVENT_NAME_LOCAL_KEY = LocalizeKey._48269; // 이벤트 모드
            const int EVENT_TITLE_LOCAL_KEY = LocalizeKey._48270; // 끝없는 나의 도전!
            const int EVENT_VALUE_1_LOCAL_KEY = LocalizeKey._48271; // - 클리어할수록 강력해지는 몬스터들을 물리쳐주세요!
            const int EVENT_VALUE_2_LOCAL_KEY = LocalizeKey._48272; // - 강한 보스를 처치해나갈수록 많은 점수를 받을 수 있어요!

            const int CHALLENGE_NAME_LOCAL_KEY = LocalizeKey._48273; // 챌린지
            const int CHALLENGE_TITLE_LOCAL_KEY = LocalizeKey._48274; // 더욱 강력해진 몬스터!
            const int CHALLENGE_VALUE_1_LOCAL_KEY = LocalizeKey._48275; // - 클리어할수록 강력해지는 몬스터들을 물리쳐주세요!
            const int CHALLENGE_VALUE_2_LOCAL_KEY = LocalizeKey._48276; // - 이벤트 모드보다 더 높은 점수를 얻을 수 있어요!
            const int CHALLENGE_VALUE_3_LOCAL_KEY = LocalizeKey._48277; // - 입장권은 보스 처치 시 소모됩니다.

            eventInfo.Set(EVENT_NAME_LOCAL_KEY, EVENT_TITLE_LOCAL_KEY, EVENT_VALUE_1_LOCAL_KEY, EVENT_VALUE_2_LOCAL_KEY);
            challengeInfo.Set(CHALLENGE_NAME_LOCAL_KEY, CHALLENGE_TITLE_LOCAL_KEY, CHALLENGE_VALUE_1_LOCAL_KEY, CHALLENGE_VALUE_2_LOCAL_KEY, CHALLENGE_VALUE_3_LOCAL_KEY);

            rankScore.SetTitle(LocalizeKey._48278); // 점수 순위 보상
            rankKill.SetTitle(LocalizeKey._48279); // 처치 순위 보상
            labelNotice.LocalKey = LocalizeKey._48280; // 이벤트 기간 종료 시 Lv은 초기화됩니다
        }

        public void SetData(UIDuelReward.IInput[] scoreRankData, UIDuelReward.IInput[] killRankData)
        {
            rankScore.SetData(scoreRankData);
            rankKill.SetData(killRankData);
            table.Reposition();
        }
    }
}