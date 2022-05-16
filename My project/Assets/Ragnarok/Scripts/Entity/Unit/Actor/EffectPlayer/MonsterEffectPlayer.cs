using MEC;

namespace Ragnarok
{
    public class MonsterEffectPlayer : UnitEffectPlayer
    {
        protected MonsterEntity entity;
        protected HudUnitName hudName;
        protected PoolObject rageEffect;

        protected override bool IsCharacter => false;

        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            this.entity = entity as MonsterEntity;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (hudName)
            {
                hudName.Release();
                hudName = null;
            }

            if (rageEffect)
            {
                rageEffect.Release();
                rageEffect = null;
            }
        }

        protected override void OnDie(UnitEntity unit, UnitEntity attacker)
        {
            base.OnDie(unit, attacker);
        }

        public override void ShowName()
        {
            if (hudName == null)
                hudName = hudPool.SpawnUnitName(CachedTransform);

            if(Cheat.IS_MONSTER_LEVEL)
            {
                hudName.Initialize(entity.GetNameId(), entity.Monster.MonsterLevel, entity.type);
            }
            else
            {
                hudName.Initialize(entity.GetNameId(), entity.type);
            }

            hudName.Show();
        }

        public override void HideName()
        {
            if (hudName)
                hudName.Hide();
        }
        
        public void PlayAngerEffect()
        {
            if (rageEffect != null)
                rageEffect.Release();
            rageEffect = battlePool.SpawnRageEffect(CachedTransform);
        }

        protected override bool IsHideHp()
        {
            return entity.type.IsHideHpMonster();
        }
    }
}