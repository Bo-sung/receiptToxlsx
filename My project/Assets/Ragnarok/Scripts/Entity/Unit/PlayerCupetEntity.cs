using UnityEngine;

namespace Ragnarok
{
    public class PlayerCupetEntity : CupetEntity, IPoolObject<PlayerCupetEntity>
    {
        public override UnitEntityType type => UnitEntityType.PlayerCupet;

        private IPoolDespawner<PlayerCupetEntity> despawner;

        public void Initialize(IPoolDespawner<PlayerCupetEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        /// <summary>
        /// Actor 생성
        /// </summary>
        protected override UnitActor SpawnEntityActor()
        {
            SetCupetRespawnable(false);

            switch (player.State)
            {
                case UnitState.Maze:
                    return unitActorPool.SpawnMazeCupet();
            }

            return unitActorPool.SpawnPlayerCupet();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Cupet, Cupet.CupetID, Cupet.Level);
        }

        protected override bool IsNeedSaveDamagePacket()
        {
            return true;
        }

        protected override bool IsDamageIgnore()
        {
            // 큐펫이면서 미로 모드의 경우에는 스킬 처리를 하지 않는다.
            if (State == UnitState.Maze)
                return true;

            return base.IsDamageIgnore();
        }

        public override UnitEntitySettings CreateUnitSettings()
        {
            var settings = base.CreateUnitSettings();

            if (State == UnitState.Maze)
                settings.unitSettings.cognizanceDistance = -1f;

            return settings;
        }
    }
}