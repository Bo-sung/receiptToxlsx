using UnityEngine;

namespace Ragnarok
{
    public class BattleTrap : IInitializable<IBattleTrapInput>
    {
        public readonly static IBattleTrapInput DEFAULT = new BattleTrapInput();

        private class BattleTrapInput : IBattleTrapInput
        {
            int IBattleTrapInput.Id => 0;
            byte IBattleTrapInput.State => 0;
            short IBattleTrapInput.IndexX => 0;
            short IBattleTrapInput.IndexZ => 0;
        }

        private readonly BattleTrapType trapType;
        private readonly IBattlePool battlePool;

        public IBattleTrapInput Input { get; private set; }
        public int Id => Input == null ? 0 : Input.Id;

        private Vector3 position;

        private NavPoolObjectWithReady poolObject;

        public BattleTrap(BattleTrapType trapType)
        {
            this.trapType = trapType;
            battlePool = BattlePoolManager.Instance;
        }

        public void Initialize(IBattleTrapInput input)
        {
            Input = input;
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
            // 갑자기 사라져버리면 어색해진다
            //Despawn();
        }

        public void ResetData()
        {
            Despawn();
        }

        private NavPoolObjectWithReady Spawn()
        {
            if (poolObject == null)
            {
                poolObject = battlePool.SpawnBoxTrap(trapType);
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