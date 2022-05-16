namespace Ragnarok
{
    public enum DungeonType
    {
        /// <summary>
        /// 제니 던전
        /// </summary>
        ZenyDungeon = 1,

        /// <summary>
        /// 경험치 던전
        /// </summary>
        ExpDungeon = 2,

        /// <summary>
        /// 월드 보스 던전
        /// </summary>
        WorldBoss = 3,

        /// <summary>
        /// 디펜스 던전
        /// </summary>
        Defence = 4,

        /// <summary>
        /// 멀티 미로
        /// </summary>
        MultiMaze = 5,

        /// <summary>
        /// 중앙실험실
        /// </summary>
        CentralLab = 6,

        /// <summary>
        /// 이벤트 멀티 미로
        /// </summary>
        EventMultiMaze = 7,

        /// <summary>
        /// 엔들리스 타워
        /// </summary>
        EnlessTower = 8,

        /// <summary>
        /// 미궁숲
        /// </summary>
        ForestMaze = 9,

        /// <summary>
        /// 게이트
        /// </summary>
        Gate = 10,
    }

    public static class DungeonTypeExtensions
    {
        public static string ToText(this DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return LocalizeKey._58502.ToText(); // 제니 던전

                case DungeonType.ExpDungeon:
                    return LocalizeKey._58500.ToText(); // 경험치 던전

                case DungeonType.WorldBoss:
                    return LocalizeKey._58504.ToText(); // 무한의 공간

                case DungeonType.Defence:
                    return LocalizeKey._58505.ToText(); // 비공정 습격

                case DungeonType.MultiMaze:
                    return LocalizeKey._58509.ToText(); // 미궁섬[멀티]

                case DungeonType.CentralLab:
                    return LocalizeKey._58506.ToText(); // 중앙실험실

                case DungeonType.EventMultiMaze:
                    return LocalizeKey._58510.ToText(); // 미궁섬[이벤트]

                case DungeonType.EnlessTower:
                    return LocalizeKey._58515.ToText(); // 엔들리스 타워

                case DungeonType.ForestMaze:
                    return LocalizeKey._58516.ToText(); // 미궁숲

                case DungeonType.Gate:
                    return LocalizeKey._58519.ToText(); // 게이트
            }

            return default;
        }
    }
}