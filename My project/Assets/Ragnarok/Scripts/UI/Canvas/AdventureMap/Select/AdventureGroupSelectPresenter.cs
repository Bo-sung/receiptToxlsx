namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIAdventureGroupSelect"/>
    /// </summary>
    public sealed class AdventureGroupSelectPresenter : ViewPresenter
    {
        public AdventureGroupSelectPresenter()
        {
            int lastStageId = Entity.player.Dungeon.LastEnterStageId; // 마지막 입장 stage
            StageData stageData = StageDataManager.Instance.Get(lastStageId);
            int chapter = stageData == null ? 1 : stageData.Chapter; // 마지막 입장한 chapter
            AdventureData adventureData = AdventureDataManager.Instance.GetChapterData(chapter);
            int currentGroup = adventureData == null ? 0 : adventureData.scenario_id; // 마지막 입장한 시나리오 그룹
            foreach (var item in AdventureGroup.DATA_LIST)
            {
                item.SetCurrentGroup(currentGroup);
            }
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public AdventureGroupElement.IInput[] GetData()
        {
            return AdventureGroup.DATA_LIST;
        }
    }
}