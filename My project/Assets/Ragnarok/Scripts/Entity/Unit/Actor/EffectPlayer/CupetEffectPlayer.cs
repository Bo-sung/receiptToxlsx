namespace Ragnarok
{
    public sealed class CupetEffectPlayer : UnitEffectPlayer
    {
        protected override bool IsCharacter => false;

        CupetEntity entity;
        private HudUnitName hudName;

        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            this.entity = entity as CupetEntity;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (hudName)
            {
                hudName.Release();
                hudName = null;
            }
        }

        public override void ShowName()
        {
            //if (hudName == null)
            //    hudName = hudPool.SpawnUnitName(CachedTransform);

            //hudName.Initialize(entity.GetName(), entity.Cupet.Level, entity.type);
        }

        protected override bool IsHideFullHP()
        {
            return true;
        }
    }
}