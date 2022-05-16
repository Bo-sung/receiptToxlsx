using UnityEngine;

namespace Ragnarok
{
    public class MazeMonsterEntity : MonsterEntity, IPoolObject<MazeMonsterEntity>
    {
        public enum MazeMonsterType
        {
            Static = 0,
            Ghost = 1, // 플레이어를 좇는 유령
            Boss = 2, // 보스
        }

        MazeMonsterType mazeMonsterType;

        public override UnitEntityType type => UnitEntityType.MazeMonster;

        public override int Layer => Ragnarok.Layer.MAZE_ENEMY;

        private int clickCount;
        public int CubeIndex { get; private set; }

        private IPoolDespawner<MazeMonsterEntity> despawner;

        public MazeMonsterEntity()
        {
            SetMazeMonsterType(MazeMonsterType.Static);
        }

        public void Initialize(IPoolDespawner<MazeMonsterEntity> despawner)
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
            return unitActorPool.SpawnMazeMonster();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return default;
        }        

        public void SetMazeMonsterType(MazeMonsterType type)
        {
            this.mazeMonsterType = type;
        }

        public MazeMonsterType GetMazeMonsterType()
        {
            return mazeMonsterType;
        }

        public void SetClickCount(int count)
        {
            clickCount = count;
        }

        public void SetCubeIndex(int index)
        {
            CubeIndex = index;
        }

        /// <summary>
        /// 클리커 몬스터 HP 적용
        /// </summary>
        /// <param name="value"></param>
        /// <param name="blowCount"></param>
        /// <param name="isNotDie"></param>
        protected override void ApplyHP(int value, int blowCount, bool isNotDie)
        {
            if (actor)
            {
                actor.Animator.PlayHit();
            }

            int damage = UnityEngine.Mathf.Max(1, MaxHP / clickCount);
            int hp = CurHP - damage;
            if (hp < damage)
            {
                hp = 0;
            }
            SetCurrentHp(hp);
        }
    }
}