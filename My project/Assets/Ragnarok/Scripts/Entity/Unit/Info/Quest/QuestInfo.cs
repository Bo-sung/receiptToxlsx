using CodeStage.AntiCheat.ObscuredTypes;
using Ragnarok.View;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class QuestInfo : DataInfo<QuestData>, UIMultiMazeQuestElement.IInput, UIPassQuestElement.IInput, UISummerEventElement.IInput
    {
        public enum QuestCompleteType
        {
            /// <summary>
            /// 보상 대기 중
            /// </summary>
            StandByReward = 1,

            /// <summary>
            /// 진행중
            /// </summary>
            InProgress = 2,

            /// <summary>
            /// 보상 완료
            /// </summary>
            ReceivedReward = 3,

            /// <summary>
            /// 진행 대기
            /// </summary>
            ProgressWait = 4,
        }

        ObscuredInt currentValue;
        ObscuredInt forcedMaxValue; // 일일퀘스트 모두 완료의 경우에는 강제로 maxValue 값을 변경해줘야 함
        ObscuredBool isReceived;
        ObscuredInt eventNo;
        ObscuredInt eventGroupId;
        ObscuredByte initType;
        RemainTime remainTime;
        ObscuredBool isWait;

        public int EventNo => eventNo;
        public int ID => data.id;
        public string Name => data.name_id.ToText();
        public short QuestTypeValue => data.quest_type;
        public QuestCategory Category => data.quest_category.ToEnum<QuestCategory>();
        public QuestType QuestType => QuestTypeValue.ToEnum<QuestType>();
        public int ConditionValue => data.condition_value;
        public string ConditionText => QuestType.ToText(MaxValue, ConditionValue);
        public string Description => data.description_id.ToText();
        public int Group => data.daily_group; // 정렬 group, MainQuset의 Seq
        public int CurrentValue => currentValue;
        public int MaxValue => forcedMaxValue > 0 ? forcedMaxValue : data.quest_value; // 강제 MaxValue 변경 값이 있을 경우
        public QuestCompleteType CompleteType => GetCompleteType();
        public RewardData[] RewardDatas => data.rewardDatas;
        public bool IsWeight => RewardDatas.Sum(x => x.TotalWeight()) > 0;
        public ShortCutType ShortCutType => data.shortCut_type.ToEnum<ShortCutType>();
        public int ShortCutValue => data.shortCut_value;
        public int GroupId => eventGroupId;
        public byte InitType => initType;
        public bool IsEventQuest => eventNo != 0;
        public RemainTime RemainTime => remainTime;
        public bool IsConditionMet => CurrentValue >= MaxValue;
        public ContentType OpenContent => data.event_content.ToEnum<ContentType>();

        public event System.Action OnUpdateQuest;

        public void GoShortCut()
        {
            ShortCutType.GoShortCut(ShortCutValue);
        }

        public int GetDescriptionID()
        {
            return data.description_id;
        }

        public void SetCurrentValue(int currentValue)
        {
            if (IsInvalidData)
                return;

            // 시간이 끝난 이벤트 퀘스트
            if (IsEventQuest && RemainTime.ToRemainTime() <= 0)
                return;

            this.currentValue = Mathf.Clamp(currentValue, 0, MaxValue);
            InvokeEvent();
            OnUpdateQuest?.Invoke();
        }

        public void SetReceived(bool isReceived)
        {
            this.isReceived = isReceived;
            InvokeEvent();
            OnUpdateQuest?.Invoke();
        }

        public void SetMaxValue(int maxValue)
        {
            forcedMaxValue = maxValue;
            InvokeEvent();
            OnUpdateQuest?.Invoke();
        }

        /// <summary>
        /// 현재 진행도 증가
        /// </summary>
        public void PlusCurrentValue(int count = 1)
        {
            SetCurrentValue(CurrentValue + count);
        }

        /// <summary>
        /// Max Value 로 변경
        /// </summary>
        public void MaxCurrentValue(int value)
        {
            SetCurrentValue(value > CurrentValue ? value : CurrentValue);
        }

        private QuestCompleteType GetCompleteType()
        {
            if (isReceived)
                return QuestCompleteType.ReceivedReward;

            if (isWait)
                return QuestCompleteType.ProgressWait;

            return CurrentValue == MaxValue ? QuestCompleteType.StandByReward : QuestCompleteType.InProgress;
        }

        /// <summary>
        /// 이벤트 No 세팅
        /// </summary>
        /// <param name="eventNo"></param>
        public void SetEventNo(int eventNo)
        {
            this.eventNo = eventNo;
        }

        public void SetEventGroupId(int eventGroupId)
        {
            this.eventGroupId = eventGroupId;
        }

        public void SetInitType(byte initType)
        {
            this.initType = initType;
        }

        public void SetRemainTime(long remainTime)
        {
            this.remainTime = remainTime;
        }

        public void SetWait(bool isWait)
        {
            this.isWait = isWait;
        }

        /// <summary>
        /// 메인 퀘스트 UI 표시에 사용된는 Group
        /// </summary>
        /// <returns></returns>
        public int GetMainQuestGroup()
        {
            return GetMainQuestGroup(Group);
        }

        public int GetMainQuestGroup(int groupId)
        {
            if (groupId > Constants.Quest.MAIN_QUEST_JUMP_START_GROUP_ID)
            {
                int diff = Constants.Quest.MAIN_QUEST_JUMP_DESTINATION_GROUP_ID - Constants.Quest.MAIN_QUEST_JUMP_START_GROUP_ID - 1;
                return groupId - diff;
            }
            return groupId;
        }         
    }

    public class FieldQuestInfo
    {
        public QuestInfo questInfo;
        public int difficulty;
        public bool isReceived { get { return questInfo.CompleteType == QuestInfo.QuestCompleteType.ReceivedReward; } set { questInfo.SetReceived(value); } }


        public void SetClearedInfo(int quest_id, int difficulty)
        {
            questInfo = new QuestInfo();
            QuestData questData = QuestDataManager.Instance.Get(quest_id);
            questInfo.SetData(questData);
            questInfo.SetCurrentValue(questInfo.MaxValue);

            this.difficulty = difficulty;
            isReceived = true;
        }
    }
}