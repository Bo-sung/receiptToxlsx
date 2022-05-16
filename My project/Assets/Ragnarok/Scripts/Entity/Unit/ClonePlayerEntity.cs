namespace Ragnarok
{
    public class ClonePlayerEntity : CharacterEntity, IPoolObject<ClonePlayerEntity>
    {
        public override UnitEntityType type => UnitEntityType.UI;

        IPoolDespawner<ClonePlayerEntity> despawner;

        public ClonePlayerEntity()
        {
            Character = Create<CharacterModel>();
            Status = Create<StatusModel>();
            BuffItemList = Create<BuffItemListModel>();
            Inventory = Create<InventoryModel>();
            Skill = Create<SkillModel>();
            Guild = Create<GuildModel>();
            Agent = Create<AgentModel>();
            Book = Create<BookModel>();
        }

        public void InitializeWithOnlyStat(CharacterEntity other)
        {
            InitializeWithOnlyStat(other, false);
        }

        public void InitializeWithOnlyStat(CharacterEntity other, bool isPreviewMode)
        {
            InitializeExtraOptions(other); // 강제 옵션 세팅
            //CloneBattleOptions(other);

            if (!isPreviewMode)
                Status.ResetData();

            Character.Initialize(other.Character);
            Status.Initialize(other.Status);

            ReloadStatus();
        }

        public void Initialize(CharacterEntity other)
        {
            Initialize(other, isPreviewMode: false);
        }

        public void Initialize(CharacterEntity other, bool isPreviewMode)
        {
            Dispose(); // 이벤트 불필요

            InitializeExtraOptions(other); // 강제 옵션 세팅
            CloneBattleOptions(other);            

            if (!isPreviewMode)
                Status.ResetData();

            Character.Initialize(other.Character);
            Character.Initialize(other.Character.GetShareForceLevels());
            Status.Initialize(other.Status);
            Status.Initialize(other.Status.IsExceptEquippedItems, other.Status.GetServerBattleOptions(), other.Status.GetServerGuildBattleOptions());
            BuffItemList.Initialize(other.BuffItemList?.GetBuffItemInfos());
            Inventory.Initialize(other.Inventory.itemList.ToArray());
            if (other.Status.IsExceptEquippedItems)
            {
                Inventory.Initialize(other.Inventory);
            }
            Skill.Initialize(other.Skill.IsExceptPassvieSkills, other.Skill.skillList.ToArray());
            Skill.Initialize(other.Skill.skillSlotList.ToArray());

            if (other.Guild)
            {
                Guild.Initialize(other.Guild.HaveGuild ? other.Guild.guildSkillList.ToArray() : null);
            }

            if (other.Agent)
            {
                Agent.Initialize(other.Agent.GetAllAgentValues(), other.Agent.bookStateList.ToArray());
            }

            if (other.Book)
            {
                Book.Initialize(other.Book.applyingBattleOptions.ToArray());
            }

            // Preview 모드일 경우에는 따로 Reload 해주는 처리가 있다.
            if (!isPreviewMode)
                ReloadStatus();
        }

        public void Initialize(IPoolDespawner<ClonePlayerEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            throw new System.NotImplementedException("Clone Player는 대미지 체크가 음슴");
        }
    }
}