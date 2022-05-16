using Ragnarok.View;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterShare2nd"/>
    /// </summary>
    public sealed class CharacterShare2ndPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterModel characterModel;
        private readonly QuestModel questModel;
        private readonly SharingModel sharingModel;

        // <!-- Repositories --!>
        private readonly QuestDataManager questDataRepo;
        private readonly ShareStatBuildUpDataManager shareStatBuildUpDataRepo;
        private readonly int needResetCatCoin;

        // <!-- Data --!>
        private readonly Share2ndSuitSlotElement[] elements;
        private readonly ShareForceUpgradeElement[] stats;

        // <!-- Event --!>
        public event System.Action OnUpdateShareForce;

        public event System.Action OnUpdateShareFreeTicket
        {
            add { sharingModel.OnUpdateShareFreeTicket += value; }
            remove { sharingModel.OnUpdateShareFreeTicket -= value; }
        }

        public CharacterShare2ndPresenter()
        {
            characterModel = Entity.player.Character;
            questModel = Entity.player.Quest;
            sharingModel = Entity.player.Sharing;

            questDataRepo = QuestDataManager.Instance;
            shareStatBuildUpDataRepo = ShareStatBuildUpDataManager.Instance;
            needResetCatCoin = BasisType.REQUEST_SHARE_STAT_RESET_CATCOIN.GetInt();

            System.Array arrType = System.Enum.GetValues(typeof(ShareForceType));
            elements = new Share2ndSuitSlotElement[arrType.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = new Share2ndSuitSlotElement((ShareForceType)arrType.GetValue(i));
            }

            DataGroup<ShareStatBuildUpData>[] groups = shareStatBuildUpDataRepo.GetData();
            stats = new ShareForceUpgradeElement[groups.Length];
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] = new ShareForceUpgradeElement(groups[i]);
            }

            RefreshShareForce();
        }

        public override void AddEvent()
        {
            characterModel.OnShareForceLevelUp += InvokeShareForce;
            characterModel.OnUpdateShareForceStatus += InvokeShareForce;
        }

        public override void RemoveEvent()
        {
            characterModel.OnShareForceLevelUp -= InvokeShareForce;
            characterModel.OnUpdateShareForceStatus -= InvokeShareForce;
        }

        void InvokeShareForce()
        {
            RefreshShareForce();
            OnUpdateShareForce?.Invoke();
        }

        /// <summary>
        /// 현재 스탯 포인트
        /// </summary>
        public int GetStatPoint()
        {
            return characterModel.GetShareForce();
        }

        /// <summary>
        /// 쉐어 포스 슬롯정보 반환
        /// </summary>
        public Share2ndSuitSlot.IInput[] GetSlots()
        {
            return elements;
        }

        /// <summary>
        /// 쉐어 포스 스탯정보 반환
        /// </summary>
        public UIShareForceUpgradeElement.IInput[] GetStatus()
        {
            return stats;
        }

        /// <summary>
        /// 쉐어 포스 스탯초기화 가능 여부
        /// </summary>
        public bool CanReset()
        {
            return characterModel.CanGetShareForceStatusReset();
        }

        /// <summary>
        /// 쉐어 포스 타입에 해당하는 선행 퀘스트
        /// </summary>
        public QuestData GetQuestData(ShareForceType type)
        {
            int seq = questModel.OpenShareForceQuestSeq(type);
            return questDataRepo.GetTimePatrolQuest(seq);
        }

        /// <summary>
        /// 스탯 초기화
        /// </summary>
        public void RequestResetStatus()
        {
            RequestSkillInitializeAsync().WrapUIErrors();
        }

        /// <summary>
        /// 캐릭터 셰어 빨콩 여부
        /// </summary>
        public bool HasCharacterShareNotice()
        {
            return sharingModel.GetShareTicketCount(ShareTicketType.DailyFree) > 0; // 무료 티켓이 존재할 경우
        }

        /// <summary>
        /// 쉐어포스 스탯 빨콩 여부
        /// </summary>
        public bool HasNoticeShareForceStat()
        {
            foreach (var item in stats)
            {
                if (item.HasNotice)
                    return true;
            }

            return false;
        }

        private void RefreshShareForce()
        {
            foreach (var item in elements)
            {
                item.Update(GetShareForceLevel(item.Type));
            }

            int statPoint = GetStatPoint();
            foreach (var item in stats)
            {
                item.Update(characterModel.GetShareForceStatusLevel(item.Group), statPoint);
            }
        }

        private int GetShareForceLevel(ShareForceType type)
        {
            if (characterModel.HasShareForce(type))
                return characterModel.GetShareForceLevel(type);

            return -1;
        }

        private async Task RequestSkillInitializeAsync()
        {
            string title = LocalizeKey._10273.ToText(); // 포스 강화 스탯 초기화
            string description = LocalizeKey._10274.ToText(); // 스탯 초기화를 진행하시겠습니까?
            if (!await UI.CostPopup(CoinType.CatCoin, needResetCatCoin, title, description))
                return;

            characterModel.RequestShareStatReset().WrapNetworkErrors();
        }

        private class Share2ndSuitSlotElement : Share2ndSuitSlot.IInput
        {
            public ShareForceType Type { get; }
            public int Level { get; private set; }

            public Share2ndSuitSlotElement(ShareForceType type)
            {
                Type = type;
            }

            public void Update(int level)
            {
                Level = level;
            }
        }

        private class ShareForceUpgradeElement : UIShareForceUpgradeElement.IInput
        {
            public readonly DataGroup<ShareStatBuildUpData> dataGroup;
            public int Group { get; }

            public int Level => level;
            public string TitleText { get; private set; }
            public string ValueText { get; private set; }
            public bool IsMaxLevel { get; private set; }
            public bool HasNotice { get; private set; }

            private int level = -1;
            private int statPoint;

            public ShareForceUpgradeElement(DataGroup<ShareStatBuildUpData> dataGroup)
            {
                this.dataGroup = dataGroup;
                Group = dataGroup.First.group;
            }

            public void Update(int level, int statPoint)
            {
                bool isDirty = false;

                if (this.level != level)
                {
                    this.level = level;

                    ShareStatBuildUpData data = level > 0 ? Find(level) : dataGroup.First;
                    BattleOption battleOption = data.GetBattleOption();
                    TitleText = battleOption.GetTitleText();
                    ValueText = level > 0 ? battleOption.GetValueText() : string.Empty;
                    IsMaxLevel = level == dataGroup.Last.stat_lv;
                }

                if (this.statPoint != statPoint)
                {
                    this.statPoint = statPoint;
                    isDirty = true;
                }

                // 레벨 또는 보유한 StatPoint 가 변경되었을 때
                if (isDirty)
                {
                    if (IsMaxLevel)
                    {
                        HasNotice = false;
                    }
                    else
                    {
                        ShareStatBuildUpData nextData = Find(Level + 1);
                        HasNotice = statPoint >= nextData.GetNeedPoint();
                    }
                }
            }

            private ShareStatBuildUpData Find(int level)
            {
                foreach (var item in dataGroup)
                {
                    if (item.stat_lv == level)
                        return item;
                }

                return null;
            }
        }
    }
}