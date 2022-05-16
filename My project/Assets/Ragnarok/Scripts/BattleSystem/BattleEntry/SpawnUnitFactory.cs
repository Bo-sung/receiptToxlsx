using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class SpawnUnitFactory
    {
        private readonly BetterList<SpawnInfo> spawnList;

        public SpawnUnitFactory()
        {
            spawnList = new BetterList<SpawnInfo>();
        }       

        /// <summary>
        /// 일반 몬스터 유닛 소환 정보
        /// </summary>
        public IEnumerable<UnitEntity> CreateNormalMonster(INormalMonsterSpawnData input, int level)
        {
            int maxIndex = input.MaxIndex;
            int waveCost = input.Cost;
            int monsterLevel = level;

            // 몬스터의 cost 세팅
            for (int i = 0; i < maxIndex; i++)
            {
                (int monsterId, int spawnRate) = input.GetSpawnInfo(i);
                if (monsterId == 0 || spawnRate == 0)
                    continue;

                MonsterData data = MonsterDataManager.Instance.Get(monsterId);
                if (data == null)
                {
                    Debug.LogError($"몬스터 데이터가 존재하지 않습니다: {nameof(monsterId)} = {monsterId}");
                    continue;
                }

                spawnList.Add(new SpawnInfo { monsterId = monsterId, spawnRate = spawnRate, cost = data.cost }); // spawnList 추가
            }

            while (waveCost > 0)
            {
                // 몬스터 비용이 큰 값은 제외
                for (int i = spawnList.size - 1; i >= 0; i--)
                {
                    if (waveCost < spawnList[i].cost)
                        spawnList.RemoveAt(i);
                }

                // 체크할 몬스터가 없을 경우
                if (spawnList.size == 0)
                    break;

                // 소환 전체 확률 계산
                int totalSpawnRate = 0;
                for (int i = 0; i < spawnList.size; i++)
                {
                    totalSpawnRate += spawnList[i].spawnRate;
                }

                // 소환할 몬스터 확률 체크
                int spwanRate;
                for (int i = 0; i < spawnList.size; i++)
                {
                    spwanRate = spawnList[i].spawnRate;

                    // 소환 당첨
                    if (Random.Range(0, totalSpawnRate) < spwanRate)
                    {
                        yield return MonsterEntity.Factory.CreateMonster(new SpawnNormalMonster(spawnList[i].monsterId, monsterLevel));
                        waveCost -= spawnList[i].cost; // 비용 감소
                        break;
                    }

                    totalSpawnRate -= spwanRate; // 전체 소환 확률 감소
                }
            }

            spawnList.Clear(); // spawnList 초기화
        }

        public IEnumerable<UnitEntity> CreateSpawnMonster(MonsterEntity bossMonster, int monsterID, int monsterCount)
        {
            for (int i = 0; i < monsterCount; i++)
                yield return MonsterEntity.Factory.CreateMonster(new SpawnNormalMonster(monsterID, bossMonster.Monster.MonsterLevel));
        }

        /// <summary>
        /// 일반 몬스터 유닛 소환 정보
        /// </summary>
        public UnitEntity CreateNormalMonster(int monsterID, int monsterLevel)
        {
            return MonsterEntity.Factory.CreateMonster(new SpawnNormalMonster(monsterID, monsterLevel));
        }

        /// <summary>
        /// 보스 몬스터 유닛 소환 정보
        /// </summary>
        public UnitEntity CreateBossMonster(IBossMonsterSpawnData input)
        {
            return CreateBossMonster(input.BossMonsterId, input.Level, input.Scale);
        }

        /// <summary>
        /// 보스 몬스터 유닛 소환 정보
        /// </summary>
        public UnitEntity CreateBossMonster(int monsterId, int monsterLevel, float scale)
        {
            return MonsterEntity.Factory.CreateMonster(new SpawnBossMonster(monsterId, monsterLevel, scale));
        }

        /// <summary>
        /// 보스 몬스터 유닛 소환 정보
        /// </summary>
        public UnitEntity CreateBossMonster(IBossMonsterSpawnData input, int level)
        {
            return MonsterEntity.Factory.CreateMonster(new SpawnBossMonster(input.BossMonsterId, level, input.Scale));
        }

        /// <summary>
        /// mvp 몬스터 유닛 소환 정보
        /// </summary>
        public UnitEntity CreateMvpMonster(int monsterId, int monsterLevel, float scale)
        {
            return MonsterEntity.Factory.CreateMvpMonster(new SpawnBossMonster(monsterId, monsterLevel, scale));
        }

        /// <summary>
        /// 보스 몬스터 유닛 소환 정보
        /// </summary>
        public UnitEntity CreateBossMonster(ScenarioMazeData mazeData)
        {
            return MonsterEntity.Factory.CreateMonster(new SpawnBossMonster(mazeData.boss_monster_id, mazeData.boss_monster_level, MathUtils.ToPercentValue(mazeData.boss_monster_scale)));
        }

        /// <summary>
        /// 수호자 유닛 소환 정보
        /// </summary>
        public UnitEntity CreateGuardian(int guardianId, int guardianLevel)
        {
            return MonsterEntity.Factory.CreateGuardian(guardianId, guardianLevel);
        }

        /// <summary>
        /// 수호자 파괴 몬스터 유닛 소환 정보
        /// </summary>
        public UnitEntity CreateGuardianDestroyer(int monsterId, int monsterLevel)
        {
            return MonsterEntity.Factory.CreateGuardianDestroyer(monsterId, monsterLevel);
        }

        /// <summary>
        /// 월드보스 몬스터 유닛 소환 정보
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public UnitEntity CreateWorldBoss(IBossMonsterSpawnData input)
        {
            return MonsterEntity.Factory.CreateWorldBoss(new SpawnBossMonster(input.BossMonsterId, input.Level, input.Scale));
        }

        /// <summary>
        /// 미로맵 몬스터 유닛 소환 정보
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public MazeMonsterEntity CreateMazeMonster(int stageId, IBossMonsterSpawnData input)
        {
            return MonsterEntity.Factory.CreateMazeMonster(stageId, input);
        }

        public MazeMonsterEntity CreateMazeMonster(int monsterId, int monsterLevel)
        {
            return MonsterEntity.Factory.CreateMazeMonster(monsterId, monsterLevel);
        }

        /// <summary>
        /// 미로던전용 몬스터 유닛 소환
        /// </summary>
        [System.Obsolete("미로던전용 임시 엔터티")]
        public MazeMonsterEntity CreateMazeDungeonMonster(DungeonType dungeonType)
        {
            var entity = MonsterEntity.Factory.CreateMazeMonster(dungeonType);
            return entity;
        }

        public UnitEntity CreateTurret(int id, int level)
        {
            return MonsterEntity.Factory.CreateTurret(id, level);
        }

        public UnitEntity CreateTurretBoss(int id, int level)
        {
            return MonsterEntity.Factory.CreateTurretBoss(id, level);
        }

        /// <summary>
        /// 일반 몬스터 소환 확률 정보
        /// </summary>
        private struct SpawnInfo
        {
            public int monsterId;
            public int spawnRate;
            public int cost;
        }

        /// <summary>
        /// 소환 몬스터 정보
        /// </summary>
        private abstract class SpawnMonster : ISpawnMonster
        {
            public abstract MonsterType Type { get; }
            public abstract int Id { get; }
            public abstract int Level { get; }
            public abstract float Scale { get; }
        }

        /// <summary>
        /// 일반 몬스터 소환 정보
        /// </summary>
        private class SpawnNormalMonster : SpawnMonster
        {
            private readonly int id;
            private readonly int level;

            public SpawnNormalMonster(int id, int level)
            {
                this.id = id;
                this.level = level;
            }

            public override MonsterType Type => MonsterType.Normal;
            public override int Id => id;
            public override int Level => level;
            public override float Scale => 1f;
        }

        /// <summary>
        /// 보스 몬스터 소환 정보
        /// </summary>
        private class SpawnBossMonster : SpawnMonster
        {
            private readonly int id;
            private readonly int level;
            private readonly float scale;

            public SpawnBossMonster(int id, int level, float scale)
            {
                this.id = id;
                this.level = level;
                this.scale = scale;
            }

            public override MonsterType Type => MonsterType.Boss;
            public override int Id => id;
            public override int Level => level;
            public override float Scale => scale;
        }
    }
}