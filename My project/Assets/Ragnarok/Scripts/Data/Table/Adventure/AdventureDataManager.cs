using MsgPack;

namespace Ragnarok
{
    public sealed class AdventureDataManager : Singleton<AdventureDataManager>, IDataManger
    {
        private readonly BetterList<AdventureData> dataList;
        private readonly Buffer<AdventureData> buffer;

        public ResourceType DataType => ResourceType.AdventureDataDB;

        public AdventureDataManager()
        {
            dataList = new BetterList<AdventureData>();
            buffer = new Buffer<AdventureData>();
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
                    dataList.Add(new AdventureData(mpo.AsList()));
                }
            }
        }

        public AdventureData[] GetArrData()
        {
            return dataList.ToArray();
        }

        public AdventureData GetChapterData(int chapter)
        {
            foreach (var item in dataList)
            {
                if (item.IsChapterData() && item.chapter == chapter)
                    return item;
            }

            return null;
        }

        public AdventureData[] GetChapters(int adventureGroup)
        {
            foreach (var item in dataList)
            {
                if (item.IsChapterData() && item.scenario_id == adventureGroup)
                    buffer.Add(item);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        public AdventureData[] GetStages(int chapter)
        {
            foreach (var item in dataList)
            {
                if (item.IsStageData() && item.chapter == chapter)
                    buffer.Add(item);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 모험 Group 에 해당하는 첫번째 Chatper를 반환
        /// </summary>
        public int GetFirstChapter(int adventureGroup)
        {
            foreach (var item in dataList)
            {
                if (item.IsChapterData() && item.scenario_id == adventureGroup)
                    return item.chapter;
            }

            return 0;
        }

        /// <summary>
        /// 모험 Group 에 해당하는 마지막 Chatper를 반환
        /// </summary>
        public int GetLastChapter(int adventureGroup)
        {
            for (int i = dataList.size - 1; i >= 0; i--)
            {
                if (dataList[i].IsChapterData() && dataList[i].scenario_id == adventureGroup)
                    return dataList[i].chapter;
            }

            return 0;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// 데이터 검증
        /// </summary>
        public void VerifyData()
        {
        }
    }
}