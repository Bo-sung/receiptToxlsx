namespace Ragnarok
{
    public sealed class MazePlayerPacket : IPacket<Response>, IMultiPlayerInput
    {
        const int NO_GUILD = 0;
        const int DUMMY_GUILD = -1;

        public string name;
        public int cid;
        public byte job;
        public int job_level;
        public int weapon_item_id;
        public byte gender;
        public string guildName;
        public int posX, posY, posZ;
        public int profileId;
        private ItemInfo.IEquippedItemValue[] equippedItems; // 장착 코스튬 정보 목록

        public bool IsExceptEquippedItems => true; // 장착아이템 옵션 제외 (서버값으로 계산)

        int CharacterModel.IInputValue.Cid => cid;
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

        int GuildModel.IInputValue.GuildId => string.IsNullOrEmpty(guildName) ? NO_GUILD : DUMMY_GUILD; // 0이면 길드 없는 것처럼 처리되므로 (-1은 더미 길드)
        string GuildModel.IInputValue.GuildName => guildName;
        int GuildModel.IInputValue.GuildEmblem => 0;
        byte GuildModel.IInputValue.GuildPosition => 0;
        int GuildModel.IInputValue.GuildCoin => 0;
        int GuildModel.IInputValue.GuildQuestRewardCount => 0;
        long GuildModel.IInputValue.GuildSkillBuyDateTime => 0L;
        byte GuildModel.IInputValue.GuildSkillBuyCount => 0;
        long GuildModel.IInputValue.GuildRejoinTime => 0L;

        float IMultiPlayerInput.PosX => posX * 0.001f;
        float IMultiPlayerInput.PosY => posY * 0.001f;
        float IMultiPlayerInput.PosZ => posZ * 0.001f;
        int IMultiPlayerInput.UID => default;

        byte IMultiPlayerInput.State => 0;

        string TradeModel.IInputValue.PrivateStoreComment => string.Empty;
        PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => default;

        bool IMultiPlayerInput.HasMaxHp => false;
        int IMultiPlayerInput.MaxHp => 0;
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
            posX = response.GetInt("7");
            posZ = response.GetInt("8");
            job_level = response.GetShort("9");

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

            if (response.ContainsKey("11"))
            {
                posY = response.GetInt("11");
            }

            profileId = response.GetInt("12");
        }

        DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Character, cid, job_level);
        }
    }
}