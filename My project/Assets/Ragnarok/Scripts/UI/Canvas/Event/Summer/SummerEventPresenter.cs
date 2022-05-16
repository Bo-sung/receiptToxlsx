using System;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISummerEvent"/>
    /// </summary>
    public class SummerEventPresenter : ViewPresenter
    {
        private const int QUEST_COUNT = 10;

        // <!-- Models --!>
        private readonly QuestModel questModel;

        // <!-- Repositories --!>

        // <!-- Data --!>
        private EventQuestGroupInfo eventQuestGroupInfo;
        private QuestInfo[] infoArray;
        private QuestInfo lastQuestInfo;
        private bool isInvalid;

        // <!-- Event --!>
        /// <summary>
        /// 보상 받을수 있는 퀘스트 수 변경시 호출
        /// </summary>
        public event System.Action OnStandByReward
        {
            add { questModel.OnStandByReward += value; }
            remove { questModel.OnStandByReward -= value; }
        }       

        public SummerEventPresenter()
        {
            questModel = Entity.player.Quest;
            infoArray = new QuestInfo[QUEST_COUNT];
            Initialize();
        }

        public override void AddEvent()
        {
            questModel.OnUpdateEventQuestSkip += RequestQuestReward;
        }

        public override void RemoveEvent()
        {
            questModel.OnUpdateEventQuestSkip -= RequestQuestReward;
        }

        public void Initialize()
        {
            eventQuestGroupInfo = questModel.GetEventQuestByShortCut(ShortCutType.SummerEvent);

            if (eventQuestGroupInfo == null)
                return;

            QuestInfo[] infos = questModel.GetEventQuests(eventQuestGroupInfo.GroupId, false);

            isInvalid = infos.Length != QUEST_COUNT + 1;
            if (isInvalid) // 유효하지 않음
                return; 

            Array.Sort(infos, SortByGroup);
            for (int i = 0; i < QUEST_COUNT; i++)
            {
                infoArray[i] = infos[i];
            }
            lastQuestInfo = infos[QUEST_COUNT];
        }

        private int SortByGroup(QuestInfo x, QuestInfo y)
        {
            return x.Group.CompareTo(y.Group);
        }

        public bool HasEventData()
        {
            if (isInvalid)
                return false;

            return eventQuestGroupInfo != null && eventQuestGroupInfo.RemainTime.ToRemainTime() > 0;
        }

        public string GetTitle()
        {
            return eventQuestGroupInfo == null ? string.Empty : eventQuestGroupInfo.Name;
        }

        public UISummerEventElement.IInput[] GetArrayData()
        {
            return infoArray;
        }

        public QuestInfo GetLastQuestInfo()
        {
            return lastQuestInfo;
        }

        /// <summary>
        /// 퀘스트 보상 요청
        /// </summary>
        public void RequestQuestReward(int questId)
        {
            if (!HasEventData()) // 이벤트 종료
            {
                UI.ShowToastPopup(LocalizeKey._90254.ToText()); // 이벤트 기간이 아닙니다.
                return;
            }

            foreach (var item in questModel.GetEventQuests(eventQuestGroupInfo.GroupId, isSort: false))
            {
                if (item.ID == questId)
                {
                    questModel.RequestQuestRewardAsync(item, isShowRewardPopup: true).WrapNetworkErrors();
                    break;
                }
            }
        }

        public void RequestQuestReward()
        {
            if (!HasEventData()) // 이벤트 종료
            {
                UI.ShowToastPopup(LocalizeKey._90254.ToText()); // 이벤트 기간이 아닙니다.
                return;
            }

            foreach (var item in questModel.GetEventQuests(eventQuestGroupInfo.GroupId, isSort: false))
            {
                if (item.CompleteType == QuestInfo.QuestCompleteType.StandByReward)
                {
                    questModel.RequestQuestRewardAsync(item, isShowRewardPopup: true).WrapNetworkErrors();
                    break;
                }
            }
        }

        public async void RequestQuestSkip()
        {
            if (!HasEventData()) // 이벤트 종료
            {
                UI.ShowToastPopup(LocalizeKey._90254.ToText()); // 이벤트 기간이 아닙니다.
                return;
            }

            int needCoin = BasisType.DAILY_QUEST_EVENT_SKIP_CAT_COIN.GetInt();
            string title = LocalizeKey._5.ToText(); // 알람
            string message = LocalizeKey._7303.ToText(); // 낭다래를 소모하여 보상을 획득하시겠습니까?
            if (!await UI.CostPopup(CoinType.CatCoin, needCoin, title, message))
                return;

            questModel.RequestEventConnectQuestSkip().WrapNetworkErrors();
        }

        public void ReqeustLastQuestReward()
        {
            RequestQuestReward(lastQuestInfo.ID);
        }
    }
}