using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// 언어테이블 데이터
    /// </summary>
    public class LanguageData : IData
    {
        public readonly int id;

        private readonly string korean;
        private readonly string english;
        private readonly string taiwan;
        private readonly string china;
        private readonly string japanese;
        private readonly string thailand;
        private readonly string indonesian;
        private readonly string philippines;
        private readonly string malaysia;
        private readonly string french;
        private readonly string german;
        private readonly string portuguese;
        private readonly string spanish;

        public LanguageData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            korean = data[index++].AsString().Replace("\\n", "\n");
            english = data[index++].AsString().Replace("\\n", "\n");
            taiwan = data[index++].AsString().Replace("\\n", "\n");
            china = data[index++].AsString().Replace("\\n", "\n");
            japanese = data[index++].AsString().Replace("\\n", "\n");
            thailand = data[index++].AsString().Replace("\\n", "\n");
            indonesian = data[index++].AsString().Replace("\\n", "\n");
            philippines = data[index++].AsString().Replace("\\n", "\n");
            malaysia = data[index++].AsString().Replace("\\n", "\n");
            french = data[index++].AsString().Replace("\\n", "\n");
            german = data[index++].AsString().Replace("\\n", "\n");
            portuguese = data[index++].AsString().Replace("\\n", "\n");
            spanish = data[index++].AsString().Replace("\\n", "\n");
        }

        public string GetString(LanguageType languageType)
        {
            switch (languageType)
            {
                case LanguageType.KOREAN: return korean;
                case LanguageType.ENGLISH: return english;
                case LanguageType.TAIWAN: return taiwan;
                case LanguageType.CHINA: return china;
                case LanguageType.JAPANESE: return japanese;
                case LanguageType.THAILAND: return thailand;
                case LanguageType.INDONESIAN: return indonesian;
                case LanguageType.PHILIPPINES: return philippines;
                case LanguageType.MALAYSIA: return malaysia;
                case LanguageType.FRENCH: return french;
                case LanguageType.GERMAN: return german;
                case LanguageType.PORTUGUESE: return portuguese;
                case LanguageType.SPANISH: return spanish;
            }

            return string.Empty;
        }

#if UNITY_EDITOR
        public class EditorTuple
        {
            [MsgPack.Serialization.MessagePackMember(0)] public int id;
            [MsgPack.Serialization.MessagePackMember(1)] public string korean;
            [MsgPack.Serialization.MessagePackMember(2)] public string english;
            [MsgPack.Serialization.MessagePackMember(3)] public string taiwan;
            [MsgPack.Serialization.MessagePackMember(4)] public string china;
            [MsgPack.Serialization.MessagePackMember(5)] public string japanese;
            [MsgPack.Serialization.MessagePackMember(6)] public string thailand;
            [MsgPack.Serialization.MessagePackMember(7)] public string indonesian;
            [MsgPack.Serialization.MessagePackMember(8)] public string philippines;
            [MsgPack.Serialization.MessagePackMember(9)] public string malaysia;
            [MsgPack.Serialization.MessagePackMember(10)] public string french;
            [MsgPack.Serialization.MessagePackMember(11)] public string german;
            [MsgPack.Serialization.MessagePackMember(12)] public string portuguese;
            [MsgPack.Serialization.MessagePackMember(13)] public string spanish;

            public EditorTuple(int id, string[] arrText)
            {
                this.id = id;

                int length = arrText.Length;
                int index = 0;

                ++index; // 1부터 시작
                korean = length > index ? arrText[index] : string.Empty;

                ++index;
                english = length > index ? arrText[index] : string.Empty;

                ++index;
                taiwan = length > index ? arrText[index] : string.Empty;

                ++index;
                china = length > index ? arrText[index] : string.Empty;

                ++index;
                japanese = length > index ? arrText[index] : string.Empty;

                ++index;
                thailand = length > index ? arrText[index] : string.Empty;

                ++index;
                indonesian = length > index ? arrText[index] : string.Empty;

                ++index;
                philippines = length > index ? arrText[index] : string.Empty;

                ++index;
                malaysia = length > index ? arrText[index] : string.Empty;

                ++index;
                french = length > index ? arrText[index] : string.Empty;

                ++index;
                german = length > index ? arrText[index] : string.Empty;

                ++index;
                portuguese = length > index ? arrText[index] : string.Empty;

                ++index;
                spanish = length > index ? arrText[index] : string.Empty;
            }
        }
#endif
    }
}