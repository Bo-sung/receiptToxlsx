namespace Ragnarok
{
    public class DummyCharacterPacket : IPacket<Response>, IMultiPlayerInput
    {
        public string name;
        public int cid;
        public byte job;
        public int job_level;
        public int weapon_item_id;
        public byte gender;
        public int[] skill_ids;
        public int profileId;

        public bool IsExceptEquippedItems => true; // 장착아이템 옵션 제외 (더미)

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
        int IMultiPlayerInput.UID => default;

        byte IMultiPlayerInput.State => 0;
        string TradeModel.IInputValue.PrivateStoreComment => string.Empty;
        PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => default;

        bool IMultiPlayerInput.HasMaxHp => false;
        int IMultiPlayerInput.MaxHp => 0;
        bool IMultiPlayerInput.HasCurHp => false;
        int IMultiPlayerInput.CurHp => 0;
        byte IMultiPlayerInput.TeamIndex => 0;
        public ItemInfo.IEquippedItemValue[] GetEquippedItems => default;

        void IInitializable<Response>.Initialize(Response response)
        {
            name = response.GetUtfString("1");
            cid = response.GetInt("2");
            job = response.GetByte("3");
            job_level = response.GetInt("4");
            weapon_item_id = response.GetInt("5");
            gender = response.GetByte("6");
            skill_ids = response.GetIntArray("7");
            profileId = response.GetInt("8");
        }

        DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(default, 0, 0);
        }
    }
}