using UnityEngine;

namespace Ragnarok
{
    public class DummyCupetEntity : CupetEntity, IPoolObject<DummyCupetEntity>
    {
        public override UnitEntityType type => UnitEntityType.UI;

        private IPoolDespawner<DummyCupetEntity> despawner;

        public void Initialize(IPoolDespawner<DummyCupetEntity> despawner)
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
            throw new System.NotImplementedException("Dummy Cupet는 대미지 체크가 음슴");
        }        
    }
}