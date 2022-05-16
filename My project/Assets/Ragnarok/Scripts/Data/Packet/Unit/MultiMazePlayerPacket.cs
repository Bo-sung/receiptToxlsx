namespace Ragnarok
{
    public sealed class MultiMazePlayerPacket : IPacket<Response>, IMultiPlayerInput
    {
        private string name;
        private int cid;
        private byte job;
        private int job_level;
        private int weapon_item_id;
        private byte gender;
        private string guildName;
        private float posX;
        private float posZ;
        private byte state;
        private int maxHp;
        private int profileId;
        private ItemInfo.IEquippedItemValue[] equippedItems; // 장착 코스튬 정보 목록

        public bool IsExceptEquippedItems => true; // 장착아이템 옵션 제외 (서버값으로 계산)

        public int Cid => cid;
        string CharacterModel.IInputValue.Name => name;
        byte CharacterModel.IInputValue.Job => job;
        byte CharacterModel.IInputValue.Gender => gender;
        int CharacterModel.IInputValue.Level => 0;
        int CharacterModel.IInputValue.LevelExp => 0;
        int CharacterModel.IInputValue.JobLevel => job_level;
        long CharacterModel.IInputValue.JobLevelExp => 0;
        int CharacterModel.IInputValue.RebirthCount => 0;
        int CharacterModel.IInputValue.RebirthAccrueCount => 0;
        int CharacterModel.IInputValue.NameChangeCount => 0;
        string CharacterModel.IInputValue.CidHex => string.Empty;
        int CharacterModel.IInputValue.ProfileId => profileId;

        int StatusModel.IInputValue.Str => 0;
        int StatusModel.IInputValue.Agi => 0;
        int StatusModel.IInputValue.Vit => 0;
        int StatusModel.IInputValue.Int => 0;
        int StatusModel.IInputValue.Dex => 0;
        int StatusModel.IInputValue.Luk => 0;
        int StatusModel.IInputValue.StatPoint => 0;

        int IMultiPlayerInput.WeaponItemId => weapon_item_id;
        BattleItemInfo.IValue IMultiPlayerInput.ItemStatusValue => null;
        int IMultiPlayerInput.ArmorItemId => 0;
        ElementType IMultiPlayerInput.WeaponChangedElement => ElementType.None;
        int IMultiPlayerInput.WeaponElementLevel => 0;
        ElementType IMultiPlayerInput.ArmorChangedElement => ElementType.None;
        int IMultiPlayerInput.ArmorElementLevel => 0;

        SkillModel.ISkillValue[] IMultiPlayerInput.Skills => null;
        SkillModel.ISlotValue[] IMultiPlayerInput.Slots => null;

        CupetListModel.IInputValue[] IMultiPlayerInput.Cupets => null;

        IBattleOption[] IMultiPlayerInput.BattleOptions => null;
        IBattleOption[] IMultiPlayerInput.GuildBattleOptions => null;

        int GuildModel.IInputValue.GuildId => 0;
        string GuildModel.IInputValue.GuildName => guildName;
        int GuildModel.IInputValue.GuildEmblem => 0;
        byte GuildModel.IInputValue.GuildPosition => 0;
        int GuildModel.IInputValue.GuildCoin => 0;
        int GuildModel.IInputValue.GuildQuestRewardCount => 0;
        long GuildModel.IInputValue.GuildSkillBuyDateTime => 0L;
        byte GuildModel.IInputValue.GuildSkillBuyCount => 0;
        long GuildModel.IInputValue.GuildRejoinTime => 0L;

        public float PosX => posX;
        public float PosY => 0f;
        public float PosZ => posZ;
        int IMultiPlayerInput.UID => default;

        byte IMultiPlayerInput.State => state;
        string TradeModel.IInputValue.PrivateStoreComment => string.Empty;
        PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => default;

        bool IMultiPlayerInput.HasMaxHp => true;
        public int MaxHp => maxHp;
        bool IMultiPlayerInput.HasCurHp => false;
        int IMultiPlayerInput.CurHp => 0;
        byte IMultiPlayerInput.TeamIndex => 0;
        ItemInfo.IEquippedItemValue[] IMultiPlayerInput.GetEquippedItems => equippedItems;

        void IInitializable<Response>.Initialize(Response response)
        {
            cid = response.GetInt("1");
            name = response.GetUtfString("2");
            if (response.ContainsKey("3"))
                guildName = response.GetUtfString("3");
            job = response.GetByte("4");
            gender = response.GetByte("5");
            weapon_item_id = response.GetInt("6");
            posX = response.GetFloat("7");
            posZ = response.GetFloat("8");

            job_level = response.GetShort("9");

            state = response.GetByte("11");

            if (response.ContainsKey("14"))
                maxHp = response.GetInt("14");

            profileId = response.GetInt("15");

            // 코스튬
            if (response.ContainsKey("99"))
            {
                string[] results =  response.GetUtfString("99").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

                equippedItems = new EquipmentValuePacket[results.Length];
                for (int i = 0; i < results.Length; i++)
                {
                    equippedItems[i] = new EquipmentValuePacket(int.Parse(results[i]));
                }
            }
            else
            {
                equippedItems = null;
            }
        }

        DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Character, cid, job_level);
        }
    }
}