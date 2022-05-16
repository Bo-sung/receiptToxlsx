using UnityEngine;

namespace Ragnarok
{
    public interface IDungeonWorldBossImpl : IDungeonImpl
    {
        /// <summary>
        /// 무료입장까지 남은 시간
        /// </summary>
        /// <returns></returns>
        float GetFreeTicketCoolTime();

        bool IsAlarm(int worldBossId);

        /// <summary>
        /// 몬스터 정보
        /// </summary>
        WorldBossMonsterInfo[] GetMonsterInfos((int monsterId, MonsterType type, int monsterLevel)[] monsterInfos);

        /// <summary>
        /// 던전 입장 가능 여부
        /// </summary>
        bool CanEnter(DungeonType dungeonType, int id, bool isShowPopup);

        /// <summary>
        /// 최대 레벨
        /// </summary>
        int GetMaxLevel();
    } 
}
