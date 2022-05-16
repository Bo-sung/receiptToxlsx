using UnityEngine;

namespace Ragnarok.SceneComposition
{
    public class DefenceSpawnZone : SpawnZone
    {
        [Tooltip("시작 지연 시간")]
        public float startDelay;

        [Tooltip("한 사이클에 소환 되는 몬스터 수")]
        public int spawnCount = 25;

        [Tooltip("한꺼번에 소환될 수")]
        public int spawnSize = 1;

        [Tooltip("소환 딜레이")]
        public float spawnDelay = 1f;

        [Tooltip("사이클 소환 쿨타임")]
        public float spawnCooldown = 25f;
    }
}