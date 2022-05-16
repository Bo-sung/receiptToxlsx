using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIWordCollectionEvent"/>
    /// </summary>
    public class WordCollectionEventPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly QuestModel questModel;
        private readonly EventModel eventModel;

        // <!-- Repositories --!>
        private readonly FindAlphabetDataManager findAlphabetDataRepo;

        // <!-- Data --!>
        private EventQuestGroupInfo eventQuestGroupInfo;
        private RewardData[] materials;

        // <!-- Event --!>
        /// <summary>
        /// [글자수집] 재료 아이템 수량 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateWordCollectionItemCount
        {
            add => eventModel.OnUpdateWordCollectionItemCount += value;
            remove => eventModel.OnUpdateWordCollectionItemCount -= value;
        }

        public WordCollectionEventPresenter()
        {
            questModel = Entity.player.Quest;
            eventModel = Entity.player.Event;
            findAlphabetDataRepo = FindAlphabetDataManager.Instance;
            Initialize();
        }

        public override void AddEvent()
        {
            eventModel.OnWordCollectionCompleteRewardEvent += OnCompleteRewardEvent;
        }

        public override void RemoveEvent()
        {
            eventModel.OnWordCollectionCompleteRewardEvent -= OnCompleteRewardEvent;
        }

        public void Initialize()
        {
            eventQuestGroupInfo = questModel.GetEventQuestByShortCut(ShortCutType.WordCollectionEvent);

            int[] materialItemIds = eventModel.GetWordCollectionItems();
            materials = new RewardData[materialItemIds.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new RewardData(RewardType.Item, materialItemIds[i], eventModel.WordCollectionNeedCount);
            }
        }

        public bool HasEventData()
        {
            if (!eventModel.IsEventWordCollection())
                return false;

            return eventQuestGroupInfo != null && eventQuestGroupInfo.RemainTime.ToRemainTime() > 0;
        }

        /// <summary>
        /// 이벤트 배너 정보
        /// </summary>
        public UIBannerElement.IInput GetData()
        {
            return eventQuestGroupInfo;
        }

        /// <summary>
        /// 글자 재료 정보
        /// </summary>
        public RewardData[] GetMaterials()
        {
            return materials;
        }

        /// <summary>
        /// 완성 횟수
        /// </summary>
        public int GetCompleteCount()
        {
            return eventModel.IsWordCollectionCompleteMaterial() ? GetCompleteRewardStep() + 1 : GetCompleteRewardStep();
        }

        /// <summary>
        /// 보상 받은 횟수
        /// </summary>
        public int GetCompleteRewardStep()
        {
            return eventModel.WordCollectionRewardStep;
        }

        /// <summary>
        /// 모든 보상 받았는지 여부
        /// </summary>
        public bool IsAllCompleteReward()
        {
            return eventModel.IsAllWordCollectionCompleteReward();
        }

        /// <summary>
        /// 보상 정보
        /// </summary>
        public UIDiceCompleteElement.IInput[] GetRewards()
        {
            return findAlphabetDataRepo.GetRewards();
        }

        /// <summary>
        /// 보상 획득 요청
        /// </summary>
        public void RequestWordCollectionReward()
        {
            eventModel.RequestWordCollectionReward().WrapNetworkErrors();
        }

        void OnCompleteRewardEvent(RewardData[] rewards)
        {
            if (rewards == null || rewards.Length == 0)
                return;

            RewardData reward = rewards[0];
            var input = new UISingleReward.Input(UISingleReward.Mode.WORD_COLLECTION_COMPLETE_REWARD, reward, reward.IconName);
            UI.Show<UISingleReward>(input);
        }
    }
}