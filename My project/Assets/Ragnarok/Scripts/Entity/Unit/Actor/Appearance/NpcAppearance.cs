namespace Ragnarok
{
    public class NpcAppearance : UnitAppearance
    {
        INpcPool npcPool;

        private PoolObject body;

        protected override void Awake()
        {
            base.Awake();

            npcPool = NpcPoolManager.Instance;
        }

        public override void OnReady(UnitEntity entity)
        {
            SetData(entity.GetPrefabName());
        }

        private void SetData(string prefabName)
        {
            OnRelease(); // 혹시라도 남아있을 전의 Body Release

            body = npcPool.Spawn(prefabName);
            LinkBody(body.CachedGameObject);
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (body != null)
            {
                body.Release();
                body = null;
            }
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public override float GetRadius()
        {
            return body == null ? 0f : GetRadius(body.CachedTransform);
        }
    }
}