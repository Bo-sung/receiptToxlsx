using UnityEngine;

namespace Ragnarok
{
    public sealed class MonsterAppearance : UnitAppearance
    {
        IMonsterPool monsterPool;

        private PoolObject body;

        protected override void Awake()
        {
            base.Awake();

            monsterPool = MonsterPoolManager.Instance;
        }

        public override void OnReady(UnitEntity entity)
        {
            SetData(entity.GetPrefabName()); // 외형 세팅
            SetLayer(entity.Layer); // 레이어 세팅
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

        private void SetData(string prefabName)
        {
            OnRelease(); // 혹시라도 남아있을 전의 Body Release

            body = monsterPool.Spawn(prefabName);
            LinkBody(body.CachedGameObject);
        }             
    }
}