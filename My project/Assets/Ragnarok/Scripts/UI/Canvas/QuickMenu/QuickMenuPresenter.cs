namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIQuickMenu"/>
    /// </summary>
    public sealed class QuickMenuPresenter : ViewPresenter
    {
        private readonly int maxMazePoint; // 미로맵 입장 포인트 최대치

        public QuickMenuPresenter()
        {
            maxMazePoint = BasisType.DUEL_POINT_DROP_MAX.GetInt();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public int GetMaxMazePoint()
        {
            return maxMazePoint;
        }
    }
}