using MsgPack;

namespace Ragnarok
{
    public sealed class GuideDataManager : Singleton<GuideDataManager>, IDataManger
    {
        private readonly BetterList<GuideData> dataList;

        public ResourceType DataType => ResourceType.GuideDataDB;

        public GuideDataManager()
        {
            dataList = new BetterList<GuideData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    dataList.Add(new GuideData(mpo.AsList()));
                }
            }
        }

        public GuideData[] GetArrayData()
        {
            return dataList.ToArray();
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            ScenarioMazeDataManager scenarioMazeDataRepo = ScenarioMazeDataManager.Instance;
            foreach (var item in dataList)
            {
                if (item.ConditionType == DungeonOpenConditionType.ScenarioMaze)
                {
                    ScenarioMazeData data = scenarioMazeDataRepo.Get(item.ConditionValue);
                    if (data == null)
                        continue;

                    item.SetScenarioMazeDataId(data.name_id);
                }
            }
        }

        /// <summary>
        /// 데이터 검증
        /// </summary>
        public void VerifyData()
        {
#if UNITY_EDITOR
#endif
        }
    }
}