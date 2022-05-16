using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BookDataManager : Singleton<BookDataManager>, IDataManger
    {
        private readonly Dictionary<int, List<BookData>> dataDic;

        public ResourceType DataType => ResourceType.BookDataDB;

        public BookDataManager()
        {
            dataDic = new Dictionary<int, List<BookData>>(IntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    BookData data = new BookData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.type))
                    {
                        dataDic.Add(data.type, new List<BookData>());
                    }

                    dataDic[data.type].Add(data);
                }
            }

            foreach (var item in dataDic.Values)
            {
                item.Sort(SortByLevel);
            }
        }

        public BookData GetBookRewardData(BookTabType tabType, int level)
        {
            int key = tabType.ToIntValue();

            if (!dataDic.ContainsKey(key))
                return null;

            var list = dataDic[key];

            if (level - 1 < 0 || list.Count <= level - 1)
                return null;

            return list[level - 1];
        }

        public IEnumerable<BookData> GetBookRewardDatas(BookTabType tabType)
        {
            int key = tabType.ToIntValue();

            if (!dataDic.ContainsKey(key))
                return null;

            return dataDic[key];
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
        }

        int SortByLevel(BookData x, BookData y)
        {
            return x.Level.CompareTo(y.Level);
        }
    }
}
