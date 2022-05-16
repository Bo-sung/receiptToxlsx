namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuide"/>
    /// </summary>
    public class GuidePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly GuideDataManager guideDataRepo;

        private readonly Buffer<GuideData> buffer;

        public GuidePresenter()
        {
            dungeonModel = Entity.player.Dungeon;
            guideDataRepo = GuideDataManager.Instance;

            buffer = new Buffer<GuideData>();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void ResetData()
        {
        }

        public UIGuideElement.IInput[] GetArrayInfo()
        {
            GuideData[] arrayData = guideDataRepo.GetArrayData();
            if (arrayData == null)
                return null;

            int index = 1;
            foreach (var item in arrayData)
            {
                // 이미 클리어한 것은 제외
                if (dungeonModel.IsOpend(item, isShowPopup: false))
                    continue;

                item.SetIndex(index++);
                buffer.Add(item);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }
    }
}