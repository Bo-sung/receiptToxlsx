using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIProgressGuildQuestInfo : UIProgressQuestInfo
    {
        protected override void UpdateView()
        {
            if (IsInvalid())
                return;

            base.UpdateView();

            bool isDailyGuildQuestComplete = presenter.GuildQestRewardCount >= presenter.GuildQuestRewardLimit;

            switch (questInfo.CompleteType)
            {
                case QuestInfo.QuestCompleteType.InProgress: // 진행중
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnComplete.IsEnabled = false;
                    if (completeBase)
                        completeBase.SetActive(false);
                    break;

                case QuestInfo.QuestCompleteType.StandByReward: // 보상 대기
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득

                    // 길드 퀘스트 보상을 전부 받았을경우 버튼 비활성화처리
                    if(isDailyGuildQuestComplete)
                    {
                        btnComplete.IsEnabled = false;
                    }
                    else
                    {
                        btnComplete.IsEnabled = true;
                    }

                    if (completeBase)
                        completeBase.SetActive(false);
                    break;

                case QuestInfo.QuestCompleteType.ReceivedReward: // 보상 받음
                    btnComplete.LocalKey = LocalizeKey._10014; // 획득완료
                    btnComplete.IsEnabled = false;
                    if (completeBase)
                        completeBase.SetActive(true);
                    break;
            }
        }
    }
}