using MsgPack;

namespace Ragnarok
{
    public class FilterNickDataManager : Singleton<FilterNickDataManager>, IDataManger
    {
        private readonly BetterList<string> list;

        public ResourceType DataType => ResourceType.NickFilterDataDB;

        public FilterNickDataManager()
        {
            list = new BetterList<string>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            list.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    WordData data = new WordData(mpo.AsList());
                    list.Add(data.word);
                }
            }
        }

        /// <summary>
        /// 금칙어 반환
        /// </summary>
        public bool Contains(string text)
        {
            foreach (var item in list)
            {
                if (text.IndexOf(item, System.StringComparison.OrdinalIgnoreCase) != -1) // 대소문자 구분하지 않음
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

#endif
        }
    }
}