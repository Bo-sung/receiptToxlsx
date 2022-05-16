namespace Ragnarok
{
    public enum MazeMode
    {
        /// <summary>
        /// 1. 충돌한 일반몬스터는 죽음 (1초 뒤 랜덤 리젠)
        /// 2. 충돌당한 플레이어는 15% 대미지 입음
        /// </summary>
        Mode1,
        /// <summary>
        /// 1. 충돌한 일반몬스터는 사라지지 않음
        /// 2. 충돌당한 플레이어는 34% 대미지 입고 시작 지점으로 워프
        /// </summary>
        Mode2,
        /// <summary>
        /// 1. 충돌한 일반몬스터는 죽음 (1초 뒤 랜덤 리젠)
        /// 2. 충돌당한 플레이어는 15% 대미지 입고 행동불능 상태
        /// 3. 행동불능 상태일 때 다른 플레이어와 충돌할 경우 큐브 조각을 뺏김
        /// </summary>
        Mode3,

        /// <summary>
        /// 이벤트 매칭 (눈싸움)
        /// </summary>
        EventMatch = 5,
    }
}