using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class AgentCharacterPacket : IMultiPlayerInput
    {
        private readonly BetterList<SkillInfo> skillList;
        private readonly BetterList<EquipmentValuePacket> costumeList;

        private int agentDataId;
        public ObscuredByte job;
        private ObscuredByte gender;
        private ObscuredInt nameId;
        private ObscuredInt weaponId;

        protected ObscuredInt jobLevel;
        private ObscuredInt str;
        private ObscuredInt agi;
        private ObscuredInt vit;
        private ObscuredInt @int;
        private ObscuredInt dex;
        private ObscuredInt luk;

        private long skillNo;

        public bool IsExceptEquippedItems => false; // 장착아이템 옵션 제외하지 않음 (클라 계산 필요)

        public virtual int Cid => 0;
        public virtual string Name => nameId.ToText();
        public byte Job => job;
        public byte Gender => gender;
        public int Level => 0;
        public int LevelExp => 0;
        public int JobLevel => jobLevel;
        public long JobLevelExp => 0;
        public int RebirthCount => 0;
        public int RebirthAccrueCount => 0;
        public int NameChangeCount => 0;
        public string CidHex => string.Empty;
        public int ProfileId => 0;

        int StatusModel.IInputValue.Str => str;
        int StatusModel.IInputValue.Agi => agi;
        int StatusModel.IInputValue.Vit => vit;
        int StatusModel.IInputValue.Int => @int;
        int StatusModel.IInputValue.Dex => dex;
        int StatusModel.IInputValue.Luk => luk;
        int StatusModel.IInputValue.StatPoint => 0;

        public int WeaponItemId => weaponId;
        public BattleItemInfo.IValue ItemStatusValue => null;
        int IMultiPlayerInput.ArmorItemId => 0;
        ElementType IMultiPlayerInput.WeaponChangedElement => ElementType.None;
        int IMultiPlayerInput.WeaponElementLevel => 0;
        ElementType IMultiPlayerInput.ArmorChangedElement => ElementType.None;
        int IMultiPlayerInput.ArmorElementLevel => 0;

        public SkillModel.ISkillValue[] Skills => skillList.ToArray();
        public SkillModel.ISlotValue[] Slots => skillList.ToArray();

        public CupetListModel.IInputValue[] Cupets => null;

        public IBattleOption[] BattleOptions => null;
        public IBattleOption[] GuildBattleOptions => null;

        int GuildModel.IInputValue.GuildId => 0; // 0이면 길드 없는 것처럼 처리되므로 (-1은 더미 길드)
        string GuildModel.IInputValue.GuildName => null;
        int GuildModel.IInputValue.GuildEmblem => 0;
        byte GuildModel.IInputValue.GuildPosition => 0;
        int GuildModel.IInputValue.GuildCoin => 0;
        int GuildModel.IInputValue.GuildQuestRewardCount => 0;
        long GuildModel.IInputValue.GuildSkillBuyDateTime => 0L;
        byte GuildModel.IInputValue.GuildSkillBuyCount => 0;
        long GuildModel.IInputValue.GuildRejoinTime => 0L;

        float IMultiPlayerInput.PosX => 0;
        float IMultiPlayerInput.PosY => 0f;
        float IMultiPlayerInput.PosZ => 0;
        byte IMultiPlayerInput.State => 0;
        int IMultiPlayerInput.UID => default;
        string TradeModel.IInputValue.PrivateStoreComment => string.Empty;
        PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => default;
        bool IMultiPlayerInput.HasMaxHp => false;
        int IMultiPlayerInput.MaxHp => 0;
        bool IMultiPlayerInput.HasCurHp => false;
        int IMultiPlayerInput.CurHp => 0;
        byte IMultiPlayerInput.TeamIndex => 0;
        public ItemInfo.IEquippedItemValue[] GetEquippedItems => costumeList.ToArray();

        public AgentCharacterPacket(AgentData agentData)
            : this(agentData.job_id, agentData.gender, agentData.name_id, agentData.weapon_id
                  , agentData.skill_id_1, agentData.skill_id_2, agentData.skill_id_3, agentData.skill_id_4
                  , agentData.costume_id_1, agentData.costume_id_2, agentData.costume_id_3, agentData.costume_id_4)
        {
            agentDataId = agentData.id;
        }

        public AgentCharacterPacket(int job, int gender, int nameId, int weaponId
            , int skillId1, int skillId2, int skillId3, int skillId4
            , int costumeId1, int costumeId2, int costumeId3, int costumeId4)
        {
            skillList = new BetterList<SkillInfo>();
            costumeList = new BetterList<EquipmentValuePacket>();

            this.job = (byte)job;
            this.gender = (byte)gender;
            this.nameId = nameId;
            this.weaponId = weaponId;

            AddSkill(skillId1);
            AddSkill(skillId2);
            AddSkill(skillId3);
            AddSkill(skillId4);

            AddCostume(costumeId1);
            AddCostume(costumeId2);
            AddCostume(costumeId3);
            AddCostume(costumeId4);
        }

        public void UpdateStatus(int jobLevel, int str, int agi, int vit, int @int, int dex, int luk)
        {
            this.jobLevel = jobLevel;
            this.str = str;
            this.agi = agi;
            this.vit = vit;
            this.@int = @int;
            this.dex = dex;
            this.luk = luk;
        }

        private void AddSkill(int skillId)
        {
            if (skillId == 0)
                return;

            skillList.Add(new SkillInfo(++skillNo, skillId));
        }

        private void AddCostume(int costumeId)
        {
            if (costumeId == 0)
                return;

            costumeList.Add(new EquipmentValuePacket(costumeId));
        }

        private class SkillInfo : SkillModel.ISkillValue, SkillModel.ISlotValue
        {
            public bool IsInPossession => true;
            public long SkillNo { get; private set; }
            public int SkillId { get; private set; }
            public int SkillLevel { get; private set; }
            public int OrderId => 0;
            public int ChangeSkillId => 0;

            public long SlotNo => SkillNo;
            public int SlotIndex => 0;
            public bool IsAutoSkill => true;

            public SkillInfo(long skillNo, int skillId)
            {
                SkillNo = skillNo;
                SkillId = skillId;
                SkillLevel = 1;
            }
        }

        public virtual DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Agent, agentDataId, jobLevel);
        }
    }
}