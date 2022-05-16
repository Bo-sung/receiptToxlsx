using MsgPack;
using System.Text;

namespace Ragnarok
{
    public class FilterChatDataManager : Singleton<FilterChatDataManager>, IDataManger
    {
        /// <summary>
        /// 채팅 욕설 대체 언어
        /// </summary>
        private const char REPLACE_FILTER = '*';

        private readonly StringBuilder sb;
        private readonly BetterList<string> list;

        public ResourceType DataType => ResourceType.ChatFilterDataDB;

        public FilterChatDataManager()
        {
            sb = new StringBuilder();
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

                    // Replace Key 는 제외
                    if (data.word.Length == 1 && data.word[0].Equals(REPLACE_FILTER))
                        continue;

                    list.Add(data.word);
                }
            }
        }

        /// <summary>
        /// 포함 여부
        /// </summary>
        public string Replace(string text)
        {
            sb.Append(text);

            foreach (string item in list)
            {
                while (true)
                {
                    int index = sb.ToString().IndexOf(item, System.StringComparison.OrdinalIgnoreCase); // 대소문자 구분하지 않음
                    if (index == -1)
                        break;

                    int length = item.Length;

                    sb.Remove(index, length); // 해당 문제 제거
                    for (int i = 0; i < length; i++)
                    {
                        sb.Insert(index, REPLACE_FILTER); // 대체 문자 삽입
                    }
                }
            }

            string output = sb.ToString();
            sb.Length = 0; // 초기화

            return output;
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