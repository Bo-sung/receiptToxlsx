using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class NpcEntity : UnitEntity, IPoolObject<NpcEntity>
    {
        public override UnitEntityType type => UnitEntityType.NPC;
        public override int Layer => Ragnarok.Layer.NPC;

        public NpcModel NPC { get; private set; }

        public override bool IsDie => false; // NPC는 스탯이 없어서 죽은 것처럼 처리되므로 직접 지정.

        private IPoolDespawner<NpcEntity> despawner;

        public void Initialize(IPoolDespawner<NpcEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        protected override UnitActor SpawnEntityActor()
        {
            UnitActor actor = unitActorPool.SpawnNPC();
            return actor;
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return default;
        }

        /// <summary>
        /// Npc 이름 id 반환
        /// </summary>
        public override int GetNameId()
        {
            Npc data = NPC.GetNpc();
            if (data == null)
                return 0;

            return data.nameLocalKey;
        }

        /// <summary>
        /// NPC 이름 반환
        /// </summary>
        public override string GetName()
        {
            return GetNameId().ToText();
        }

        /// <summary>
        /// NPC 프리팹명 반환
        /// </summary>
        public override string GetPrefabName()
        {
            Npc data = NPC.GetNpc();
            if (data == null)
                return "Empty";

            return data.prefabName;
        }

        public override string GetProfileName()
        {
            return string.Empty;
        }

        public override string GetThumbnailName()
        {
            return string.Empty; // 원형 썸네일 음슴
        }

        public NpcType GetNpcType()
        {
            Npc data = NPC.GetNpc();
            if (data == null)
                return default;

            return data.Key;
        }

        public override UnitEntitySettings CreateUnitSettings()
        {
            return null;
        }

        public override SkillInfo[] GetValidGuildSkills()
        {
            return null;
        }

        protected override BattleBuffItemInfo.ISettings[] GetArrayBuffItemSettings()
        {
            return null;
        }

        protected override EventBuffInfo[] GetArrayEventBuffInfos()
        {
            return null;
        }

        protected override IEnumerable<BlessBuffItemInfo> GetBlssBuffItems()
        {
            return null;
        }

        protected override ItemInfo[] GetEquippedItems()
        {
            return null;
        }

        protected override int GetServerTotalItemAtk()
        {
            return 0;
        }

        protected override int GetServerTotalItemMatk()
        {
            return 0;
        }

        protected override int GetServerTotalItemDef()
        {
            return 0;
        }

        protected override int GetServerTotalItemMdef()
        {
            return 0;
        }

        protected override SkillInfo[] GetValidSkills()
        {
            return null;
        }

        protected override IEnumerable<IAgent> GetAllAgents()
        {
            return null;
        }

        protected override IEnumerable<AgentBookState> GetEnabledBookStates()
        {
            return null;
        }

        protected override IEnumerable<ShareStatBuildUpData> GetShareForceData()
        {
            return null;
        }

        protected override BattleOption[] GetServerBattleOptions()
        {
            return null;
        }

        protected override BattleOption[] GetServerGuildBattleOptions()
        {
            return null;
        }

        public new class Factory
        {
            /// <summary>
            /// NPC 생성
            /// </summary>
            public static NpcEntity CreateNPC(NpcType npcType)
            {
                NpcEntity entity = new NpcEntity();
                entity.NPC = Create<NpcModel>(entity);
                entity.NPC.Initialize(npcType);
                entity.Initialize();

                return entity;
            }

            private static T Create<T>(NpcEntity entity)
                where T : UnitModel<NpcEntity>, new()
            {
                return UnitModel<NpcEntity>.Create<T>(entity);
            }
        }
    }
}