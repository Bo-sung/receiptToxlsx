namespace Ragnarok
{
    public enum DungeonInfoType
    {
        /// <summary>
        /// 무한의 공간(월드보스)
        /// </summary>
        WorldBoss = 1,

        /// <summary>
        /// 중앙 실험실
        /// </summary>
        CentralLab = 2,

        /// <summary>
        /// 펫 테이밍 던전
        /// </summary>
        TamingMaze = 3,

        /// <summary>
        /// 길드 미로
        /// </summary>
        GuildPvp = 4,

        /// <summary>
        /// 대전 (PVP)
        /// </summary>
        League = 5,

        /// <summary>
        /// 듀얼
        /// </summary>
        Duel = 6,

        /// <summary>
        /// 스테이지 (이벤트모드)
        /// </summary>
        EventStage = 7,

        /// <summary>
        /// 스테이지 (챌린지모드)
        /// </summary>
        StageChallenge = 8,

        /// <summary>
        /// 엔들리스 타워
        /// </summary>
        EndlessTower = 9,

        /// <summary>
        /// 미궁숲
        /// </summary>
        ForestMaze = 10,

        /// <summary>
        /// 타임패트롤
        /// </summary>
        TimePatrol = 11,

        /// <summary>
        /// 2세대 셰어바이스
        /// </summary>
        ShareVice2nd = 12,

        // 제니던전, 경험치던전, 멀티 미로는 각 테이블에 dungeon_info_id 항목이 있음

        /// <summary>
        /// 제니 던전
        /// </summary>
        ZenyDungeon = 100,

        /// <summary>
        /// 경험치 던전
        /// </summary>
        ExpDungeon = 101,
        
        /// <summary>
        /// 멀티 미로
        /// </summary>
        MultiMaze = 102,

        /// <summary>
        /// 비공정 습격
        /// </summary>
        Defence = 103,
    }

    public static class DungeonInfoTypeExtension
    {
        /// <summary>
        /// 던전 안내 Info_id 반환
        /// </summary>
        public static int GetDungeonInfoId(this DungeonInfoType dungeonInfoType, int dungeonId = 0)
        {
            switch (dungeonInfoType)
            {
                case DungeonInfoType.ZenyDungeon:
                case DungeonInfoType.ExpDungeon:
                    int clickerDungeonInfoId = ClickerDungeonDataManager.Instance.Get(dungeonId).dungeon_info_id;
                    return clickerDungeonInfoId;

                case DungeonInfoType.MultiMaze:
                    int multiMazeInfoId = MultiMazeDataManager.Instance.Get(dungeonId).dungeon_info_id;
                    return multiMazeInfoId;

                case DungeonInfoType.Defence:
                    int defenceDungeonInfoId = DefenceDungeonDataManager.Instance.Get(dungeonId).dungeon_info_id;
                    return defenceDungeonInfoId;
            }

            return dungeonInfoType.ToIntValue();
        }
    }
}