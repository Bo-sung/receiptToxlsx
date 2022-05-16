namespace Ragnarok
{
    public class MultiLobbyPlayerPacket : IPacket<Response>, IMultiPlayerInput
    {
        private string name;
        private int cid;
        private byte job;
        private int job_level;
        private int profileId;
        private int weapon_item_id;
        private byte gender;
        private string guildName;
        private float posX;
        private float posZ;
        private string privateStoreComment;
        private PrivateStoreSellingState sellingState;
        private int uid;
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

        int GuildModel.IInputValue.GuildId => 0;
        string GuildModel.IInputValue.GuildName => guildName;
        int GuildModel.IInputValue.GuildEmblem => 0;
        byte GuildModel.IInputValue.GuildPosition => 0;
        int GuildModel.IInputValue.GuildCoin => 0;
        int GuildModel.IInputValue.GuildQuestRewardCount => 0;
        long GuildModel.IInputValue.GuildSkillBuyDateTime => 0L;
        byte GuildModel.IInputValue.GuildSkillBuyCount => 0;
        long GuildModel.IInputValue.GuildRejoinTime => 0L;

        float IMultiPlayerInput.PosX => posX;
        float IMultiPlayerInput.PosY => 0f;
        float IMultiPlayerInput.PosZ => posZ;
        int IMultiPlayerInput.UID => uid;
        byte IMultiPlayerInput.State => 0;
        string TradeModel.IInputValue.PrivateStoreComment => privateStoreComment;
        PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => sellingState;
        bool IMultiPlayerInput.HasMaxHp => false;
        int IMultiPlayerInput.MaxHp => 0;
        bool IMultiPlayerInput.HasCurHp => false;
        int IMultiPlayerInput.CurHp => 0;
        byte IMultiPlayerInput.TeamIndex => 0;
        bool IMultiPlayerInput.IsExceptEquippedItems => true;

        ItemInfo.IEquippedItemValue[] IMultiPlayerInput.GetEquippedItems => equippedItems;

        void IInitializable<Response>.Initialize(Response response)
        {
            name = response.GetUtfString("n");
            guildName = response.GetUtfString("g");
            if (string.Equals(guildName, "0")) // TODO: 길드 없을 때 0으로 들어오는데 길드명이 진짜 0이면 문제가 생길 수 있다 ..
                guildName = string.Empty;

            posX = response.GetFloat("x");
            posZ = response.GetFloat("z");
            weapon_item_id = (int)response.GetLong("wp");
            privateStoreComment = response.GetUtfString("sc");
            int[] cInfos = response.GetIntArray("app");
            cid = cInfos[0];
            job = (byte)cInfos[1];
            gender = (byte)cInfos[2];
            sellingState = cInfos[3].ToEnum<PrivateStoreSellingState>();
            job_level = cInfos[4];
            if (cInfos.Length > 5)
                uid = cInfos[5];

            if (cInfos.Length > 6)
                profileId = cInfos[6];

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