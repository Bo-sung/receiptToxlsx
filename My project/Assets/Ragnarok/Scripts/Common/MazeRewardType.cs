namespace Ragnarok
{
    /// <summary>
    /// 미로맵 드롭 아이템 보상 타입
    /// </summary>
    public enum MazeRewardType
    {
        None = 0,
        /// <summary>
        /// 제니
        /// </summary>
        Zeny = 1,

        /// <summary>
        /// 의뢰 퀘스트
        /// </summary>
        NormalQuest = 2,

        /// <summary>
        /// 랜덤 박스
        /// </summary>
        RandomBox = 3,

        /// <summary>
        /// 큐브 조각
        /// </summary>
        CubePiece = 4,

        /// <summary>
        /// 미로맵 폭탄(맵 시야 감소)
        /// </summary>
        BombCamera = 5,

        /// <summary>
        /// 미로맵 폭탄(HP 감소)
        /// </summary>
        BombHP = 6,

        /// <summary>
        /// 미로맵 폭탄(컨트롤 반대로)
        /// </summary>
        BombControl = 7,

        /// <summary>
        /// 시야 방해
        /// </summary>
        HindranceCamera = 8,

        /// <summary>
        /// 조작 방해
        /// </summary>
        HindranceControl = 9,

        /// <summary>
        /// 이속 포션
        /// </summary>
        SpeedItem = 98,

        /// <summary>
        /// 보물상자
        /// </summary>
        Treasure = 99,

        /// <summary>
        /// [멀티던전] 큐브 조각
        /// </summary>
        MultiMazeCube = 100,

        /// <summary>
        /// [이벤트멀티던전] 눈덩이
        /// </summary>
        Snowball = 101,

        /// <summary>
        /// [이벤트멀티던전] 루돌프
        /// </summary>
        Rudolph = 102,

        /// <summary>
        /// [이벤트멀티던전] 강탈물약
        /// </summary>
        PowerUpPotion = 103,

        /// <summary>
        /// [미궁숲] 체력포션
        /// </summary>
        HpPotion = 104,

        /// <summary>
        /// [미궁숲] 엠펠리움 조각
        /// </summary>
        Emperium = 105,
    }

    public static class MazeRewardTypeExtensions
    {
        /// <summary>
        /// 폭탄 아이템 여부
        /// </summary>
        public static bool IsBomb(this MazeRewardType type)
        {
            switch (type)
            {
                case MazeRewardType.BombCamera:
                case MazeRewardType.BombHP:
                case MazeRewardType.BombControl:
                case MazeRewardType.HindranceCamera:
                case MazeRewardType.HindranceControl:
                    return true;
            }
            return false;
        }
    }
}
