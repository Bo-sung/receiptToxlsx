namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIResultClear"/>
    /// </summary>
    public sealed class ResultClearPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;

        private readonly DungeonModel dungeonModel;
        private readonly DailyModel dailyModel;

        public ResultClearPresenter(IView view)
        {
            this.view = view;
            dungeonModel = Entity.player.Dungeon;
            dailyModel = Entity.player.Daily;
        }

        public override void AddEvent()
        {
            dailyModel.OnResetDailyEvent += view.Refresh;
        }

        public override void RemoveEvent()
        {
            dailyModel.OnResetDailyEvent -= view.Refresh;
        }       

        /// <summary>
        /// 던전 무료입장 남은 횟수
        /// </summary>
        public int GetDungeonFreeEntryCount(DungeonType dungeonType)
        {
            return dungeonModel.GetFreeEntryCount(dungeonType);
        }

        /// <summary>
        /// 던전 무료입장 최대 횟수
        /// </summary>       
        public int GetFreeEntryMaxCount(DungeonType dungeonType)
        {
            return dungeonModel.GetFreeEntryMaxCount(dungeonType);
        }       
    }
}