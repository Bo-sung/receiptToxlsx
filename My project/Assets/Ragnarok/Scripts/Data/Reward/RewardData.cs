using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public sealed class RewardData : IData, System.IEquatable<RewardData>
    {
        private readonly ObscuredByte rewardType;  // Item이외 타입은 즉시 지급
        private readonly ObscuredInt rewardValue;  // rewardType이 Item 일경우 ItemId, 이외 일경우는 지급 수량
        private readonly ObscuredInt rewardCount;  // rewardType이이 Item 일경우 수량
        private readonly ObscuredInt rewardOption; // 1 이상의 값이 있으면, 상점
        private object userCustomValue;
        public int ItemId => rewardValue;
        public int ShopId => rewardOption;
        public bool IsEvent { get; private set; }

        public RewardData(byte rewardType, int rewardValue, int rewardCount, int rewardOption = 0)
        {
            this.rewardType = rewardType;
            this.rewardValue = rewardValue;
            this.rewardCount = rewardCount;
            this.rewardOption = rewardOption;
        }

        public RewardData(int rewardType, int rewardValue, int rewardCount, int rewardOption = 0)
        {
            this.rewardType = (byte)rewardType;
            this.rewardValue = rewardValue;
            this.rewardCount = rewardCount;
            this.rewardOption = rewardOption;
        }

        public RewardData(RewardType rewardType, int rewardValue, int rewardCount, int rewardOption = 0)
        {
            this.rewardType = rewardType.ToByteValue();
            this.rewardValue = rewardValue;
            this.rewardCount = rewardCount;
            this.rewardOption = rewardOption;
        }

        public void SetUserCustomValue(object userCustomValue)
        {
            this.userCustomValue = userCustomValue;
        }

        public void SetIsEvent(bool isEvent)
        {
            IsEvent = isEvent;
        }

        public RewardType RewardType { get { return rewardType.ToEnum<RewardType>(); } }

        public ItemData ItemData => ItemDataManager.Instance.Get(rewardValue);
        public AgentData AgentData => AgentDataManager.Instance.Get(rewardValue);

        public string IconName
        {
            get
            {
                switch (RewardType)
                {
                    case RewardType.Item:
                        return ItemData.icon_name;

                    case RewardType.Agent:
                        return AgentData.GetIconName(AgentIconType.RectIcon);

                }
                return RewardType.IconName();
            }
        }

        public int RewardValue => rewardValue;

        public int RewardCount => rewardCount;

        public int Count
        {
            get
            {
                if (RewardType == RewardType.Item)
                    return rewardCount;
                else if (RewardType == RewardType.Agent)
                    return rewardCount;

                return rewardValue;
            }
        }

        public int Weight
        {
            get
            {
                if (RewardType != RewardType.Item)
                    return 0;

                return ItemData.weight * rewardCount;
            }
        }

        /// <summary>
        /// 보상 아이템 이름
        /// </summary>
        public string ItemName
        {
            get
            {
                if (RewardType == RewardType.Item)
                    return ItemData.name_id.ToText();
                else if (RewardType == RewardType.Agent)
                    return AgentData.name_id.ToText();

                return RewardType.GetItemName();
            }
        }

        public object UserCustomValue => userCustomValue;

        public ItemGroupType ItemGroupType => GetItemGroupType();
        public ItemType ItemType => GetItemType();

        public int Rating => GetRating();
        public int Transcend { get; private set; } = 0;
        public int ChangedElement { get; private set; } = 0;
        public int ElementLevel { get; private set; } = 0;

        public void SetTranscend(int transcend)
        {
            Transcend = transcend;
        }

        public void SetChangedElement(int element)
        {
            ChangedElement = element;
        }

        public void SetElementLevel(int elementLevel)
        {
            ElementLevel = elementLevel;
        }

        public int TotalWeight()
        {
            // 보상이 아이템일 경우 무게 체크
            if (RewardType == RewardType.Item)
            {
                return ItemData.weight * rewardCount;
            }

            // 보상이 보상그룹테이블 참조
            if (RewardType == RewardType.RefRewardGroup)
            {
                var refRewardData = RewardGroupDataManager.Instance.Get(rewardValue, rewardCount);
                return refRewardData.rewardData.Weight;
            }

            // 보상이 가챠 테이블 참조
            if (RewardType == RewardType.RefGacha)
            {
                var refGachas = GachaDataManager.Instance.Gets(rewardValue);
                var itemRepo = ItemDataManager.Instance;
                int totalWeight = 0;
                for (int i = 0; i < refGachas.Length; i++)
                {
                    if (refGachas[i].reward_type.ToEnum<RewardType>() == RewardType.Item)
                    {
                        totalWeight += itemRepo.Get(refGachas[i].reward_value).weight;
                    }
                }
                return totalWeight;
            }

            return 0;
        }

        public AgentType GetAgentType()
        {
            if (RewardType != RewardType.Agent)
                return AgentType.None;

            AgentData agentData = AgentData;
            if (agentData == null)
                return AgentType.None;

            return agentData.agent_type.ToEnum<AgentType>();
        }

        /// <summary>
        /// 속성석 레벨
        /// </summary>
        public int GetElementStoneLevel()
        {
            if (RewardType != RewardType.Item)
                return -1;

            if (ItemData == null)
                return -1;

            return ItemData.GetElementStoneLevel();
        }

        ItemType GetItemType()
        {
            if (RewardType != RewardType.Item)
                return ItemType.None;

            return ItemData.ItemType;
        }

        ItemGroupType GetItemGroupType()
        {
            if (RewardType != RewardType.Item)
                return ItemGroupType.None;

            return ItemData.ItemGroupType;
        }

        int GetRating()
        {
            if (RewardType == RewardType.Agent)
                return AgentData.agent_rating;

            if (ItemType == ItemType.Equipment || ItemType == ItemType.Card || (ItemType == ItemType.Box))
                return ItemData.rating;

            return 0;
        }

        public string GetDescription()
        {
            ItemData itemData = ItemData;
            if (itemData == null)
                return string.Empty;

            // 거래 가능
            if (itemData.point_value > 0)
                return itemData.des_id.ToText();

            return StringBuilderPool.Get()
                .Append(itemData.des_id.ToText())
                .Append(LocalizeKey._30508.ToText()) // [c][cdcdcd] (거래 불가)[-][/c]
                .Release();
        }

        /// <summary>
        /// 쉐도우 아이템 여부
        /// </summary>
        public bool IsShadow()
        {
            ItemData itemData = ItemData;
            if (itemData == null)
                return false;

            if (RewardType != RewardType.Item)
                return false;

            if (ItemGroupType == ItemGroupType.Equipment && itemData.duration == 1)
                return true;

            if (ItemGroupType == ItemGroupType.Card && itemData.duration == 1)
                return true;

            return false;
        }

        public bool Equals(RewardData other)
        {
            if (other == null)
                return false;

            return rewardType.Equals(other.rewardType) && rewardValue.Equals(other.rewardValue) && rewardCount.Equals(other.rewardCount) && rewardOption.Equals(other.rewardOption);
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 29 + rewardType.GetHashCode();
            hash = hash * 29 + rewardValue.GetHashCode();
            hash = hash * 29 + rewardCount.GetHashCode();
            hash = hash * 29 + rewardOption.GetHashCode();

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RewardData other))
                return false;

            return Equals(other);
        }
    }
}