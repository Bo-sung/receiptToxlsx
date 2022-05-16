using UnityEngine;

namespace Ragnarok
{
    public class BattleItem : IInitializable<IBattleItemInput>
    {
        private readonly IBattlePool battlePool;

        public int Id { get; private set; }

        private Vector3 position;

        private NavPoolObjectWithReady poolObject;

        public BattleItem()
        {
            battlePool = BattlePoolManager.Instance;
        }

        public void Initialize(IBattleItemInput input)
        {
            Id = input.Id;
        }

        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }

        public void ShowReady()
        {
            NavPoolObjectWithReady poolObject = Spawn();

            poolObject.Warp(position);

            // 파티클 재생을 위해서 껐다 킴
            poolObject.HideReady();
            poolObject.ShowReady();

            poolObject.HideMain();
        }

        public void Appear()
        {
            NavPoolObjectWithReady poolObject = Spawn();

            poolObject.Warp(position);
            poolObject.ShowMain();
        }

        public void Disappear()
        {
            Despawn();
        }

        public void ResetData()
        {
            Id = 0;
            SetPosition(Vector3.zero);
            Despawn();
        }

        private NavPoolObjectWithReady Spawn()
        {
            if (poolObject == null)
            {
                poolObject = battlePool.SpawnPowerUpPotion();
            }

            return poolObject;
        }

        private void Despawn()
        {
            if (poolObject == null)
                return;

            poolObject.Release();
            poolObject = null;
        }
    }
}