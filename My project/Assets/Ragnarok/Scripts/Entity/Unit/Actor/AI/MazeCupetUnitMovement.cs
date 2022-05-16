namespace Ragnarok
{
    public class MazeCupetUnitMovement : MazeUnitMovement
    {
        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            SetDistanceLimit(Constants.Battle.MAZE_SMALL_REMAINING_DISTANCE_LIMIT);
        }
    }
}