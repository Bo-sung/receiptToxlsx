using UnityEngine;

namespace Ragnarok
{
    public class PlayerBotEntity : CharacterEntity, IPoolObject<PlayerBotEntity>, IInitializable<IMultiPlayerInput>
    {
        public readonly static IMultiPlayerInput DEFAULT = new DefaultMultiPlayerInput();

        private class DefaultMultiPlayerInput : IMultiPlayerInput
        {
            public bool IsExceptEquippedItems => true; // 장착아이템 옵션 제외 (기본값)          

            int CharacterModel.IInputValue.Cid => -1;
            string CharacterModel.IInputValue.Name => string.Empty;
            byte CharacterModel.IInputValue.Job => default;
            byte CharacterModel.IInputValue.Gender => default;
            int CharacterModel.IInputValue.Level => 0;
            int CharacterModel.IInputValue.LevelExp => 0;
            int CharacterModel.IInputValue.JobLevel => 0;
            long CharacterModel.IInputValue.JobLevelExp => 0;
            int CharacterModel.IInputValue.RebirthCount => 0;
            int CharacterModel.IInputValue.RebirthAccrueCount => 0;
            int CharacterModel.IInputValue.NameChangeCount => 0;
            string CharacterModel.IInputValue.CidHex => string.Empty;
            int CharacterModel.IInputValue.ProfileId => 0;

            int StatusModel.IInputValue.Str => 0;
            int StatusModel.IInputValue.Agi => 0;
            int StatusModel.IInputValue.Vit => 0;
            int StatusModel.IInputValue.Int => 0;
            int StatusModel.IInputValue.Dex => 0;
            int StatusModel.IInputValue.Luk => 0;
            int StatusModel.IInputValue.StatPoint => 0;

            int GuildModel.IInputValue.GuildId => 0;
            string GuildModel.IInputValue.GuildName => string.Empty;
            int GuildModel.IInputValue.GuildEmblem => 0;
            byte GuildModel.IInputValue.GuildPosition => 0;
            int GuildModel.IInputValue.GuildCoin => 0;
            int GuildModel.IInputValue.GuildQuestRewardCount => 0;
            long GuildModel.IInputValue.GuildSkillBuyDateTime => 0L;
            byte GuildModel.IInputValue.GuildSkillBuyCount => 0;
            long GuildModel.IInputValue.GuildRejoinTime => 0L;

            int IMultiPlayerInput.WeaponItemId => 0;
            int IMultiPlayerInput.ArmorItemId => 0;
            ElementType IMultiPlayerInput.WeaponChangedElement => ElementType.None;
            int IMultiPlayerInput.WeaponElementLevel => 0;
            ElementType IMultiPlayerInput.ArmorChangedElement => ElementType.None;
            int IMultiPlayerInput.ArmorElementLevel => 0;
            BattleItemInfo.IValue IMultiPlayerInput.ItemStatusValue => null;
            SkillModel.ISkillValue[] IMultiPlayerInput.Skills => null;
            SkillModel.ISlotValue[] IMultiPlayerInput.Slots => null;
            CupetListModel.IInputValue[] IMultiPlayerInput.Cupets => null;
            IBattleOption[] IMultiPlayerInput.BattleOptions => null;
            IBattleOption[] IMultiPlayerInput.GuildBattleOptions => null;
            float IMultiPlayerInput.PosX => 0f;
            float IMultiPlayerInput.PosY => 0f;
            float IMultiPlayerInput.PosZ => 0f;
            byte IMultiPlayerInput.State => 0;

            string TradeModel.IInputValue.PrivateStoreComment => string.Empty;
            PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => default;
            int IMultiPlayerInput.UID => default;
            bool IMultiPlayerInput.HasMaxHp => false;
            int IMultiPlayerInput.MaxHp => 0;
            bool IMultiPlayerInput.HasCurHp => false;
            int IMultiPlayerInput.CurHp => 0;
            byte IMultiPlayerInput.TeamIndex => 0;
            public ItemInfo.IEquippedItemValue[] GetEquippedItems => default;

            DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
            {
                return new DamagePacket.UnitKey(default, 0, 0);
            }
        }

        public override UnitEntityType type => UnitEntityType.MultiPlayer;

        private IPoolDespawner<PlayerBotEntity> despawner;

        public int Uid { get; private set; }

        public byte BotState { get; private set; }
        public int? BotMaxHp { get; private set; }
        public int? BotCurHp { get; private set; }
        public Vector3 BotPosition { get; private set; }
        public PrivateStoreSellingState BotSellingState { get; private set; }
        public string BotStoreComment { get; private set; }

        public void Initialize(IPoolDespawner<PlayerBotEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        public void Initialize(IMultiPlayerInput input)
        {
            Uid = input.UID;

            SetBotState(input.State);
            SetBotPosition(new Vector3(input.PosX, input.PosY, input.PosZ));

            if (input.HasMaxHp)
            {
                SetBotMaxHp(input.MaxHp);
            }
            else
            {
                SetBotMaxHp(null);
            }

            if (input.HasCurHp)
            {
                SetBotCurHp(input.CurHp);
            }
            else
            {
                SetBotCurHp(null);
            }

            SetBotSellingState(input.PrivateStoreSellingState, input.PrivateStoreComment);

            Character.Initialize(input);
            Status.Initialize(input);
            Status.Initialize(input.IsExceptEquippedItems, input.BattleOptions, input.GuildBattleOptions);
            Inventory.Initialize(input.ItemStatusValue, input.WeaponItemId, input.ArmorItemId, input.WeaponChangedElement, input.WeaponElementLevel, input.ArmorChangedElement, input.ArmorElementLevel, input.GetEquippedItems);
            Skill.Initialize(input.IsExceptEquippedItems, input.Skills);
            Skill.Initialize(input.Slots);
            Guild.Initialize(input);
            Trade.Initialize(input);
        }

        public void SetBotState(byte state)
        {
            BotState = state;
        }

        public void SetBotMaxHp(int? maxHp)
        {
            BotMaxHp = maxHp;
        }

        public void SetBotCurHp(int? curHp)
        {
            BotCurHp = curHp;
        }

        public void SetBotPosition(Vector3 pos)
        {
            BotPosition = pos;
        }

        public void SetBotSellingState(PrivateStoreSellingState sellingState, string storeComment)
        {
            BotSellingState = sellingState;
            BotStoreComment = storeComment;
        }

        protected override UnitActor SpawnEntityActor()
        {
            return unitActorPool.SpawnPlayerBot();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Character, Character.Cid, Character.JobLevel);
        }
    }
}