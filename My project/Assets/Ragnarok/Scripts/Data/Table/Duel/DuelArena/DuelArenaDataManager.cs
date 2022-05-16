using MsgPack;

namespace Ragnarok
{
    public class DuelArenaDataManager : Singleton<DuelArenaDataManager>, IDataManger
    {
        private readonly BetterList<DuelArenaData> dataList;

        public ResourceType DataType => ResourceType.DuelArenaDataDB;

        public DuelArenaDataManager()
        {
            dataList = new BetterList<DuelArenaData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    DuelArenaData data = new DuelArenaData(mpo.AsList());
                    dataList.Add(data);
                }
            }

            dataList.Sort((a, b) => a.Start.CompareTo(b.Start));
        }

        public DuelArenaData Find(int point)
        {
            int length = dataList.size;
            for (int i = 0; i < length; i++)
            {
                if (point >= dataList[i].Start && point <= dataList[i].Max)
                    return dataList[i];
            }

            return null;
        }

        public DuelArenaData FindNext(int point)
        {
            int index = FindIndex(point);
            int maxIndex = dataList.size - 1;
            return index == maxIndex ? null : dataList[index + 1];
        }

        public DuelArenaData FindLast()
        {
            int maxIndex = dataList.size - 1;
            return maxIndex < 0 ? null : dataList[maxIndex];
        }

        private int FindIndex(int point)
        {
            int length = dataList.size;
            for (int i = 0; i < length; i++)
            {
                if (point >= dataList[i].Start && point <= dataList[i].Max)
                    return i;
            }

            return -1;
        }

        public void Initialize()
        {
            int length = dataList.size;
            int maxIndex = length - 1;
            for (int i = 0; i < length; i++)
            {
                if (i == maxIndex)
                {
                    dataList[i].SetMax(int.MaxValue);
                }
                else
                {
                    dataList[i].SetMax(dataList[i + 1].Start - 1); // 다음 데이터 참조
                }
            }
        }

        public void VerifyData()
        {
        }
    }
}