namespace Ragnarok
{
    public class GuildLobbyCharacterPacket : IPacket<Response>, IMultiPlayerInput
    {
        private string name;
        private int cid;
        private byte job;
        private int job_level;
        private int profileId;
        private int weapon_item_id;
        private byte gender;
        private float posX;
        private float posZ;
        private string guildName;
        private int uid;
        private ItemInfo.IEquippedItemValue[] equippedItems; // 장착 코스튬 정보 목록
        private bool hasCurHp;
        private int curHp;

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

        byte IMultiPlayerInput.State => 0;
        int IMultiPlayerInput.UID => uid;

        bool IMultiPlayerInput.HasMaxHp => false;
        int IMultiPlayerInput.MaxHp => 0;
        bool IMultiPlayerInput.HasCurHp => hasCurHp;
        int IMultiPlayerInput.CurHp => curHp;
        byte IMultiPlayerInput.TeamIndex => 0;
        string TradeModel.IInputValue.PrivateStoreComment => string.Empty;
        PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => default;
        ItemInfo.IEquippedItemValue[] IMultiPlayerInput.GetEquippedItems => equippedItems;

        void IInitializable<Response>.Initialize(Response response)
        {
            name = response.GetUtfString("n");
            guildName = response.GetUtfString("g");
            posX = response.GetFloat("x");
            posZ = response.GetFloat("z");
            weapon_item_id = (int)response.GetLong("wp");
            int[] cInfos = response.GetIntArray("app");
            cid = cInfos[0];
            job = (byte)cInfos[1];
            gender = (byte)cInfos[2];
            job_level = cInfos[3];
            uid = cInfos[4];

            if (cInfos.Length > 5)
                profileId = cInfos[5];

            // 코스튬
            if (response.ContainsKey("99"))
            {
                string[] results = response.GetUtfString("99").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

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

            hasCurHp = response.ContainsKey("hp");
            curHp = response.GetInt("hp");

            bool hasCurMp = response.ContainsKey("mp");
            int curMp = response.GetInt("mp");
        }

        DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Character, cid, job_level);
        }
    }
}