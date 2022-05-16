using System;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildAttack"/>
    /// </summary>
    public class GuildAttackPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GuildModel guildModel;
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly SkillDataManager skillDataRepo;
        private readonly ItemDataManager itemDataRepo;
        private readonly GuildSquareManager guildSquareManager;

        // <!-- Event --!>
        public event Action OnUpdateRefineEmperium
        {
            add { guildSquareManager.OnUpdateRefineEmperium += value; }
            remove { guildSquareManager.OnUpdateRefineEmperium -= value; }
        }

        public event Action OnUpdateGuildAttackStartTime
        {
            add { guildSquareManager.OnUpdateGuildAttackStartTime += value; }
            remove { guildSquareManager.OnUpdateGuildAttackStartTime -= value; }
        }

        public event Action OnUpdateCreateEmperium
        {
            add { guildSquareManager.OnUpdateCreateEmperium += value; }
            remove { guildSquareManager.OnUpdateCreateEmperium -= value; }
        }

        private int donationRewardCount;
        private int buffSkillId;
        private int changeTimeCoin;

        public GuildAttackPresenter()
        {
            guildModel = Entity.player.Guild;
            inventoryModel = Entity.player.Inventory;

            skillDataRepo = SkillDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            guildSquareManager = GuildSquareManager.Instance;

            donationRewardCount = BasisType.GUILD_ATTACK_DONATION_REWARD_COIN.GetInt(); // 길드습격 기부 보상 수
            buffSkillId = BasisType.GUILD_ATTACK_EMPERIUM_BUFF_SKILL_ID.GetInt(); // 버프 스킬 ID
            changeTimeCoin = BasisType.GUILD_ATTACK_CHANGE_TIME_COIN.GetInt();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 길드 습격 시간 변경
        /// </summary>
        public void RequestChangeStartTime()
        {
            // 수량 부족
            if (GetRefineEmperiumCount() < GetChangeTimeCoin())
            {
                UI.ShowToastPopup(LocalizeKey._90262.ToText()); // 아이템이 부족합니다.
                return;
            }

            guildModel.RequestChangeGuildAttackTime().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드 기부
        /// </summary>
        public void RequestDonation()
        {
            RewardData needItem = GetDonationItem();

            if (needItem == null)
                return;

            int count = GetDoantionNeedItemOwnedCount();
            int needCount = GetDonationNeedCount();
            string text;
            bool isAllow = true;
            if (count >= needCount)
            {
                // 강화 재료로 아이템 투입
                text = LocalizeKey._90261.ToText()
                    .Replace(ReplaceKey.ITEM, GetDonationItemName()); // [62AEE4][C]{ITME}[/c][-]을 기부하시겠습니까?
            }
            else
            {
                isAllow = false;
                text = LocalizeKey._90262.ToText(); // 아이템이 부족합니다.
            }

            UI.Show<UISelectMaterialPopup>().Set(needItem, count, needCount, text, LocalizeKey._2905.ToText(), isAllow, OnSelectPopup);
        }

        void OnSelectPopup()
        {
            guildModel.RequestDonationGuildAttack().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드 마스터 여부
        /// </summary>
        public bool IsGuildMaster()
        {
            return guildModel.HaveGuild && guildModel.GuildPosition == GuildPosition.Master;
        }

        /// <summary>
        /// 길드 기부 보상
        /// </summary>
        /// <returns></returns>
        public RewardData GetDonationReward()
        {
            return new RewardData(RewardType.GuildCoin, donationRewardCount, default);
        }

        /// <summary>
        /// 엠펠리움 레벨
        /// </summary>
        public int GetEmperiumLevel()
        {
            return guildSquareManager.EmperiumLevel;
        }

        /// <summary>
        /// 엠펠리움 버프 정보
        /// </summary>
        public IEnumerable<BattleOption> GetBattleOptions(int level)
        {
            SkillData data = skillDataRepo.Get(buffSkillId, level);

            if (data == null)
                yield break;

            if (data.battle_option_type_1 > 0)
                yield return new BattleOption(data.battle_option_type_1, data.value1_b1, data.value2_b1);

            if (data.battle_option_type_2 > 0)
                yield return new BattleOption(data.battle_option_type_2, data.value1_b2, data.value2_b2);

            if (data.battle_option_type_3 > 0)
                yield return new BattleOption(data.battle_option_type_3, data.value1_b3, data.value2_b3);

            if (data.battle_option_type_4 > 0)
                yield return new BattleOption(data.battle_option_type_4, data.value1_b4, data.value2_b4);
        }

        /// <summary>
        /// 기부 시 필요 아이템 수량
        /// </summary>
        public int GetDonationNeedCount()
        {
            return 1; // 1로 고정
        }

        /// <summary>
        /// 기부 시 필요 아이템 ID
        /// </summary>
        public int GetDonationNeedItemId()
        {
            return BasisItem.Emperium.GetID();
        }

        /// <summary>
        /// 기부 시 필요 아이템 보유 수량
        /// </summary>
        public int GetDoantionNeedItemOwnedCount()
        {
            return inventoryModel.GetItemCount(GetDonationNeedItemId());
        }

        /// <summary>
        /// 기부 시 필요 아이템 아이콘 이름
        /// </summary>
        public string GetDonationItemIconName()
        {
            ItemData item = itemDataRepo.Get(GetDonationNeedItemId());
            if (item == null)
                return string.Empty;

            return item.icon_name;
        }

        /// <summary>
        /// 기부 시 필요 아이템 이름
        /// </summary>
        public string GetDonationItemName()
        {
            ItemData item = itemDataRepo.Get(GetDonationNeedItemId());
            if (item == null)
                return string.Empty;

            return item.name_id.ToText();
        }

        /// <summary>
        /// 길드 시 필요한 아이템 정보
        /// </summary>
        public RewardData GetDonationItem()
        {
            ItemData item = itemDataRepo.Get(GetDonationNeedItemId());
            if (item == null)
                return null;

            return new RewardData(RewardType.Item, GetDonationNeedItemId(), GetDonationNeedCount());
        }

        /// <summary>
        /// 길드습격 시간 변경시 필요한 재화 필요 수량
        /// </summary>
        public int GetChangeTimeCoin()
        {
            return changeTimeCoin;
        }

        /// <summary>
        /// 정제된 엠펠리움 보유 수량
        /// </summary>
        public int GetRefineEmperiumCount()
        {
            return guildSquareManager.RefineEmperium;
        }

        /// <summary>
        /// 길드습격 엠펠리움 최대 레벨 여부
        /// </summary>
        public bool IsMaxEmperiumLevel()
        {
            return GetEmperiumLevel() == BasisType.GUILD_ATTACK_EMPERIUM_MAX_LEVEL.GetInt();
        }

        /// <summary>
        /// 길드습격까지 남은 시간
        /// </summary>
        public TimeSpan GetReaminTime()
        {
            return guildSquareManager.GuildAttackStartTime - ServerTime.Now;
        }

        /// <summary>
        /// 길드습격 시작 시간
        /// </summary>
        public DateTime GetStartTime()
        {
            return guildSquareManager.GuildAttackStartTime;
        }

        /// <summary>
        /// 길드습격 진행 시간 목록
        /// </summary>
        public string GetGuildAttackStartTimes()
        {
            var sb = StringBuilderPool.Get();
            List<int> keyList = BasisType.GUILD_ATTACK_START_TIME_LIST.GetKeyList();
            int count = 1;
            foreach (var key in keyList)
            {
                string time = BasisType.GUILD_ATTACK_START_TIME_LIST.GetString(key);

                // string = 10:30(UTC+0) -> DateTime으로 변환
                if (TimeSpan.TryParse(time, out TimeSpan span))
                {
                    DateTime dateTime = ((long)span.TotalMilliseconds).ToDateTime();
                    string text = dateTime.ToString("HH:mm");

                    if (count == keyList.Count)
                    {
                        sb.Append(text);
                    }
                    else
                    {
                        sb.Append(text).Append("/");
                    }

                    count++;
                }
            }
            return sb.Release();
        }

        #region 길드습격 엠펠리움 생성       

        /// <summary>
        /// 길드 습격 엠펠리움 생성 비용
        /// </summary>
        /// <returns></returns>
        public int GetCreateZeny()
        {
            return BasisType.GUILD_ATTACK_EMPERIUM_CREATE_ZENY.GetInt();
        }        

        /// <summary>
        /// 엠펠리움 생성 요청 (길드 마스터만 가능)
        /// </summary>
        public async void RequestEmperiumCreate()
        {
            if (!IsGuildMaster())
                return;

            string title = LocalizeKey._5.ToText(); // 알람
            string description = LocalizeKey._90265.ToText(); // 엠펠리움을 생성하시겠습니까?
            if (!await UI.CostPopup(CoinType.Zeny, GetCreateZeny(), title, description))
                return;

            guildModel.RequestCreateEmperium().WrapNetworkErrors();
        }

        #endregion
    }
}