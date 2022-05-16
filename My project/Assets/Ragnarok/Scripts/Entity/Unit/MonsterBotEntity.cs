using UnityEngine;

namespace Ragnarok
{
    public sealed class MonsterBotEntity : MonsterEntity, IPoolObject<MonsterBotEntity>, IBattleInput
    {
        public readonly static IMonsterBotInput DEFAULT = new DefaultMonsterBotInput();
        public readonly static ITamingMonsterPotInput TAMING = new TamingMonsterBotInput();

        private class DefaultMonsterBotInput : IMonsterBotInput
        {
            MonsterType ISpawnMonster.Type => default;
            int ISpawnMonster.Id => 0;
            int ISpawnMonster.Level => 0;
            float ISpawnMonster.Scale => 1f;

            int IMonsterBotInput.Index => -1;
            byte IMonsterBotInput.State => 0;
            float IMonsterBotInput.PosX => 0f;
            float IMonsterBotInput.PosY => 0f;
            float IMonsterBotInput.PosZ => 0f;
            bool IMonsterBotInput.HasTargetPos => false;
            float IMonsterBotInput.SavedTargetingTime => 0f;
            float IMonsterBotInput.TargetPosX => 0f;
            float IMonsterBotInput.TargetPosY => 0f;
            float IMonsterBotInput.TargetPosZ => 0f;
            bool IMonsterBotInput.HasMaxHp => false;
            int IMonsterBotInput.MaxHp => 0;
            bool IMonsterBotInput.HasCurHp => false;
            int IMonsterBotInput.CurHp => 0;
            CrowdControlType IMonsterBotInput.CrowdControl => default;
            float? IMonsterBotInput.MoveSpeed => null;
        }

        private class TamingMonsterBotInput : ITamingMonsterPotInput
        {
            MonsterType ISpawnMonster.Type => default;
            int ISpawnMonster.Id => 0;
            int ISpawnMonster.Level => 0;
            float ISpawnMonster.Scale => 1f;

            int ITamingMonsterPotInput.Index => -1;
            byte ITamingMonsterPotInput.State => default;
        }

        public override UnitEntityType type => UnitEntityType.MonsterBot;

        public int BotServerIndex { get; private set; }
        public byte BotState { get; private set; }
        public Vector3 BotPosition { get; private set; }
        public int? BotMaxHp { get; private set; }
        public int? BotCurHp { get; private set; }
        public float BotSavedTargetingTime { get; private set; }
        public Vector3? BotTargetPosition { get; private set; }

        public MonsterType MonsterType { get; private set; }
        public WayPointZone LastWayPoint { get; set; } // 최근 웨이포인트
        public WayPointZone DestWayPoint { get; set; } // 목표 웨이포인트
        public float? MoveSpeed { get; private set; }
        public float Scale => scale;

        private IPoolDespawner<MonsterBotEntity> despawner;

        public void Initialize(IPoolDespawner<MonsterBotEntity> despawner)
        {
            this.despawner = despawner;
            LastWayPoint = null;
            DestWayPoint = null;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        public void Initialize(IMonsterBotInput input)
        {
            LastWayPoint = null;
            DestWayPoint = null;

            SetBotServerIndex(input.Index);
            SetBotState(input.State);

            if (input.HasMaxHp)
            {
                SetBotMaxHp(input.MaxHp);
            }
            else
            {
                SetBotMaxHp(null);
            }

            if (input.HasCurHp)
            {
                SetBotCurHp(input.CurHp);
            }
            else
            {
                SetBotCurHp(null);
            }

            Vector3 pos = new Vector3(input.PosX, input.PosY, input.PosZ);
            if (input.HasTargetPos)
            {
                SetBotPosition(pos, new Vector3(input.TargetPosX, input.TargetPosY, input.TargetPosZ), input.SavedTargetingTime);
            }
            else
            {
                SetBotPosition(pos);
            }

            SetMonsterType(input.Type);
            SetBotMonster(input.Id, input.Level, input.Scale);
            SetMoveSpeed(input.MoveSpeed);
        }

        public void Initialize(int id, int level, MonsterType monType = MonsterType.Normal, float scale = 1f)
        {
            LastWayPoint = null;
            DestWayPoint = null;

            SetBotServerIndex(-1);
            SetBotState(0);
            SetBotMaxHp(null);
            SetBotCurHp(null);
            SetBotPosition(Vector3.zero);

            SetMonsterType(monType);
            SetBotMonster(id, level, scale);
            SetMoveSpeed(null);
        }

        public void Initialize(ITamingMonsterPotInput input)
        {
            LastWayPoint = null;
            DestWayPoint = null;

            SetBotServerIndex(input.Index);
            SetBotState(input.State);
            SetBotMaxHp(null);
            SetBotCurHp(null);
            SetBotPosition(Vector3.zero);

            SetMonsterType(input.Type);
            SetScale(input.Scale);
            SetBotMonster(input.Id, input.Level, input.Scale);
            SetMoveSpeed(null);
        }

        public void SetBotServerIndex(int serverIndex)
        {
            BotServerIndex = serverIndex;
        }

        public void SetBotState(byte state)
        {
            BotState = state;
        }

        public void SetBotMaxHp(int? maxHp)
        {
            BotMaxHp = maxHp;
        }

        public void SetBotCurHp(int? curHp)
        {
            BotCurHp = curHp;
        }

        public void SetBotPosition(Vector3 pos)
        {
            SetBotPosition(pos, null, 0f);
        }

        public void SetBotPosition(Vector3 pos, Vector3 targetPos)
        {
            SetBotPosition(pos, targetPos, Time.realtimeSinceStartup);
        }

        public void SetBotMonster(int id, int level, float scale)
        {
            SetScale(scale);
            Monster.Initialize(id, level); // 몬스터 세팅
        }

        private void SetBotPosition(Vector3 pos, Vector3? targetPos, float savedTargetingTime)
        {
            BotPosition = pos;
            BotTargetPosition = targetPos;
            BotSavedTargetingTime = savedTargetingTime;
        }

        private void SetMonsterType(MonsterType monsterType)
        {
            MonsterType = monsterType;
        }

        private void SetScale(float scale)
        {
            base.scale = scale;
        }

        private void SetMoveSpeed(float? moveSpeed)
        {
            MoveSpeed = moveSpeed;
        }

        protected override UnitActor SpawnEntityActor()
        {
            UnitActor actor = unitActorPool.SpawnMonsterBot();
            actor.CachedTransform.localScale = Vector3.one * scale; // 스케일 적용
            return actor;
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.NormalMonster, Monster.MonsterID, Monster.MonsterLevel);
        }
    }
}