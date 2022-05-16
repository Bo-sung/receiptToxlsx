using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleForestMaze"/>
    /// </summary>
    public class BattleForestMazePresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly ForestBaseDataManager forestBaseDataRepo;
        private readonly ForestMonDataManager forestMonDataRepo;
        private readonly int needBossBattleEmperiumCount;
        private readonly int maxEmperiumCount;
        private readonly int levelDecreaseValue;

        public BattleForestMazePresenter()
        {
            forestBaseDataRepo = ForestBaseDataManager.Instance;
            forestMonDataRepo = ForestMonDataManager.Instance;
            needBossBattleEmperiumCount = BasisForestMazeInfo.NeedEmperiumCount.GetInt();
            maxEmperiumCount = BasisForestMazeInfo.MaxEmperiumCount.GetInt();
            levelDecreaseValue = BasisForestMazeInfo.LevelDecreaseValue.GetInt();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 보스 전투에 필요한 엠펠리움 수
        /// </summary>
        public int GetNeedCount()
        {
            return needBossBattleEmperiumCount;
        }

        /// <summary>
        /// 최대 얻을 수 있는 엠펠리움 수
        /// </summary>
        public int GetMaxCount()
        {
            return maxEmperiumCount;
        }

        /// <summary>
        /// 보스 최고 레벨
        /// </summary>
        public int GetBossMaxLevel(int groupId)
        {
            ForestBaseData[] arrData = forestBaseDataRepo.Get(groupId);
            if (arrData == null || arrData.Length == 0)
                return 0;

            for (int i = 0; i < arrData.Length; i++)
            {
                int monsterGroupId = arrData[i].monster_group;
                int bossLevel = forestMonDataRepo.GetBossMonsterLevel(monsterGroupId);

                // 보스 레벨 존재
                if (bossLevel > 0)
                    return bossLevel;
            }

            return 0;
        }

        /// <summary>
        /// 보스 최저 레벨
        /// </summary>
        public int GetBossMinLevel(int groupId)
        {
            int maxLevel = GetBossMaxLevel(groupId);
            int decreaseLevelValue = (maxEmperiumCount - needBossBattleEmperiumCount) * levelDecreaseValue;
            return maxLevel - decreaseLevelValue;
        }

        /// <summary>
        /// 현재 보스 레벨
        /// </summary>
        public int GetCurrentBossLevel(int groupId, int count)
        {
            int maxLevel = GetBossMaxLevel(groupId);
            int minLevel = GetBossMinLevel(groupId);

            int decreaseLevelValue = (count - needBossBattleEmperiumCount) * levelDecreaseValue;
            int level = maxLevel - decreaseLevelValue;
            return Mathf.Max(minLevel, level);
        }
    }
}