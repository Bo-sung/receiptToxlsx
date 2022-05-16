using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public class BattleCharacterPacket : IPacket<Response>, IMultiPlayerInput, IAgentMultiPlayerInfo
    {
        const int NO_GUILD = 0;
        const int DUMMY_GUILD = -1;

        protected int cid; // 서버cid
        private string name; // 이름 (보여주기 용)
        protected byte job; // 직업
        protected byte gender; // 성별 (보여주기 용)
        private short level; // 레벨 (보여주기 용)
        protected short job_level; // 직업 레벨
        protected int profileId;
        private int weapon_item_id; // 장착한 무기 아이디 (장착한 무기가 없으면 0)
        private int armor_item_id;
        private ElementType weaponChangedElement;
        private int weaponElementLevel;
        private ElementType armorChangedElement;
        private int armorElementLevel;

        private BasicStatusValuePacket basic_status_value; // 기본 스탯
        private ItemStatusValuePacket item_status_value; // 장착한 아이템 스탯
        private BattleOptionPacket[] battle_options; // 전투 옵션 합산 정보
        private BattleOptionPacket[] guild_battle_options; // 길드 전투 옵션 합산 정보
        private SkillValuePacket[] skills; // 장착한 스킬 정보
        private SkillSlotInfo[] slots; // 장착한 스킬 슬롯 정보
        private string guild_name; // 길드이름 (보여주기 용)
        private AgentSlotInfoPacket[] agentSlots; // 동료 슬롯 정보 
        protected int battle_score;
        protected int uid;
        private ItemInfo.IEquippedItemValue[] equippedItems; // 장착 중인 장비(카드 정보 포함), 코스튬 정보 목록

        // [타 유저 정보 보기]
        private SkillModel.ISkillSimpleValue[] all_skill_simple_values; // 보유한 모든 스킬 목록 (패시브 포함)
        public SkillModel.ISkillSimpleValue[] AllSkillSimpleValues => all_skill_simple_values;

        public bool IsExceptEquippedItems => true; // 장착아이템 옵션 제외 (서버값으로 계산)

        public int Cid => cid;
        public string Name => name;
        public byte Job => job;
        public byte Gender => gender;
        public int Level => level;
        public int LevelExp => 0;
        public int JobLevel => job_level;
        public int Power => battle_score;

        public long JobLevelExp => 0;
        public int RebirthCount => 0;
        public int RebirthAccrueCount => 0;
        public int NameChangeCount => 0;
        public string CidHex => MathUtils.CidToHexCode(Cid);
        public int ProfileId => profileId;

        public int Str => basic_status_value.str;
        public int Agi => basic_status_value.agi;
        public int Vit => basic_status_value.vit;
        public int Int => basic_status_value.@int;
        public int Dex => basic_status_value.dex;
        public int Luk => basic_status_value.luk;
        public int StatPoint => 0;

        public int WeaponItemId => weapon_item_id;
        public BattleItemInfo.IValue ItemStatusValue => item_status_value;
        public int ArmorItemId => armor_item_id;
        public ElementType WeaponChangedElement => weaponChangedElement;
        public int WeaponElementLevel => weaponElementLevel;
        public ElementType ArmorChangedElement => armorChangedElement;
        public int ArmorElementLevel => armorElementLevel;

        public SkillModel.ISkillValue[] Skills => skills;
        public SkillModel.ISlotValue[] Slots => slots;

        public CupetListModel.IInputValue[] Cupets => null;

        public IBattleOption[] BattleOptions => battle_options;
        public IBattleOption[] GuildBattleOptions => guild_battle_options;

        public int GuildId => string.IsNullOrEmpty(guild_name) ? NO_GUILD : DUMMY_GUILD; // 0이면 길드 없는 것처럼 처리되므로 (-1은 더미 길드)
        public string GuildName => guild_name;
        public int GuildEmblem => 0;
        public byte GuildPosition => 0;
        public int GuildCoin => 0;
        public int GuildQuestRewardCount => 0;
        public long GuildSkillBuyDateTime => 0L;
        public byte GuildSkillBuyCount => 0;
        public long GuildRejoinTime => 0L;

        public float PosX { get; protected set; }
        public float PosY { get; protected set; }
        public float PosZ { get; protected set; }
        public byte State { get; protected set; }
        public int UID => uid;
        public string PrivateStoreComment => string.Empty;
        public PrivateStoreSellingState PrivateStoreSellingState => default;

        public AgentSlotInfoPacket[] AgentSlots => agentSlots;
        public int JobId => job;
        public int BaseLevel => level;

        public bool HasMaxHp { get; protected set; }
        public int MaxHp { get; protected set; }
        public bool HasCurHp { get; protected set; }
        public int CurHp { get; protected set; }
        public byte TeamIndex { get; protected set; }
        public ItemInfo.IEquippedItemValue[] GetEquippedItems => equippedItems;

        public virtual void Initialize(Response response)
        {
            cid = response.GetInt("1");
            name = response.GetUtfString("2");
            job = response.GetByte("3");
            gender = response.GetByte("4");
            level = response.GetShort("5");
            job_level = response.GetShort("6");
            weapon_item_id = response.GetInt("7");
            basic_status_value = response.GetPacket<BasicStatusValuePacket>("8");
            item_status_value = response.GetPacket<ItemStatusValuePacket>("9");

            // 전투 옵션
            if (response.ContainsKey("10"))
            {
                battle_options = response.GetPacketArray<BattleOptionPacket>("10");
            }

            long skillNo = 0;
            if (response.ContainsKey("11"))
            {
                skills = response.GetPacketArray<SkillValuePacket>("11");
                slots = new SkillSlotInfo[skills.Length];

                for (int i = 0; i < skills.Length; ++i)
                {
                    skills[i].SetSkillNo(++skillNo); // 스킬 고유 정보 세팅
                    slots[i] = new SkillSlotInfo(skillNo, skills[i].pos);
                }
            }

            if (response.ContainsKey("13"))
            {
                guild_name = response.GetUtfString("13");

                // 길드이름이 "0"으로 올 경우에는 없는 처리
                if (guild_name.Equals("0"))
                    guild_name = string.Empty;
            }

            if (response.ContainsKey("14"))
                guild_battle_options = response.GetPacketArray<BattleOptionPacket>("14");

            if (response.ContainsKey("15"))
                agentSlots = response.GetPacketArray<AgentSlotInfoPacket>("15");

            if (response.ContainsKey("16"))
                battle_score = response.GetInt("16");

            if (response.ContainsKey("17"))
                uid = response.GetInt("17");

            // 착용중인 장비 & 코스튬 아이템 목록
            if (response.ContainsKey("18"))
                equippedItems = response.GetPacketArray<EquipmentValuePacket>("18");

            // 패시브 스킬 추가.
            if (response.ContainsKey("19"))
            {
                all_skill_simple_values = response.GetPacketArray<SkillSimpleValuePacket>("19");

                List<SkillValuePacket> addSkillList = new List<SkillValuePacket>();
                foreach (var skill in all_skill_simple_values)
                {
                    if (skills.Any(e => e.skill_id == skill.SkillId))
                        continue;

                    SkillValuePacket newSkill = new SkillValuePacket();
                    newSkill.skill_id = skill.SkillId;
                    newSkill.skill_level = skill.SkillLevel;
                    newSkill.pos = 0;
                    newSkill.SetSkillNo(++skillNo);

                    addSkillList.Add(newSkill);
                }

                if (addSkillList.Count > 0)
                    skills = skills.Concat(addSkillList).ToArray();
            }

            armorChangedElement = response.ContainsKey("20") ? response.GetInt("20").ToEnum<ElementType>() : ElementType.None;
            weaponChangedElement = response.ContainsKey("21") ? response.GetInt("21").ToEnum<ElementType>() : ElementType.None;
            armor_item_id = response.ContainsKey("22") ? response.GetInt("22") : 0;
            armorElementLevel = response.GetInt("23");
            weaponElementLevel = response.GetInt("24");
            profileId = response.GetInt("25");
        }

        public virtual DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Character, cid, job_level);
        }

        private class SkillSlotInfo : SkillModel.ISlotValue
        {
            private readonly long skillNo;
            private readonly int slotIndex;

            long SkillModel.ISlotValue.SlotNo => 0L;
            long SkillModel.ISlotValue.SkillNo => skillNo;
            int SkillModel.ISlotValue.SlotIndex => slotIndex;
            bool SkillModel.ISlotValue.IsAutoSkill => true;

            public SkillSlotInfo(long skillNo, int slotIndex)
            {
                this.skillNo = skillNo;
                this.slotIndex = slotIndex;
            }
        }
    }
}