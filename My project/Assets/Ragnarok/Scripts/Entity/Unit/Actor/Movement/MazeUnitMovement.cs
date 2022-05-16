namespace Ragnarok
{
    public class MazeUnitMovement : UnitMovement
    {
        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            SetDefaultSpeed(Constants.Battle.MAZE_MOVE_SPEED); // 강제 미로 이동속도
        }
    }
}