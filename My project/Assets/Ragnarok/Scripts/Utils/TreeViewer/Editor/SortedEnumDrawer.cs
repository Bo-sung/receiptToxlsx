using System.Collections.Generic;

namespace Ragnarok
{
    public class SortedEnumDrawer : EnumDrawer
    {
        private const string ETC = "기타";

        private readonly List<Tuple> tupleList;

        public SortedEnumDrawer(bool isShowId, bool hasTitle = true) : base(isShowId, hasTitle)
        {
            tupleList = new List<Tuple>();
        }

        public override void Ready()
        {
            if (tupleList.Count == 0)
                return;

            tupleList.Sort(); // 정렬

            foreach (var item in tupleList)
            {
                if (Contains(item.id))
                    continue;

                idHashSet.Add(item.id);
                idList.Add(item.id);
                nameList.Add(item.name);
                displayList.Add(item.displayName);
            }

            tupleList.Clear();
        }

        public override void Add(int id, string name)
        {
            string displayName = StringBuilderPool.Get().Append(GetInitialText(name)).Append('/').Append(name).Release();
            Add(id, name, displayName);
        }

        public override void Add(int id, string name, string displayName)
        {
            tupleList.Add(new Tuple(id, name, displayName));
        }

        private string GetInitialText(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length == 0)
                return ETC;

            char ch = name[0];
            if (ch >= '가' && ch <= '깋') return "ㄱ";
            if (ch >= '까' && ch <= '낗') return "ㄲ";
            if (ch >= '나' && ch <= '닣') return "ㄴ";
            if (ch >= '다' && ch <= '딯') return "ㄷ";
            if (ch >= '따' && ch <= '띻') return "ㄸ";
            if (ch >= '라' && ch <= '맇') return "ㄹ";
            if (ch >= '마' && ch <= '밓') return "ㅁ";
            if (ch >= '바' && ch <= '빟') return "ㅂ";
            if (ch >= '빠' && ch <= '삫') return "ㅃ";
            if (ch >= '사' && ch <= '싷') return "ㅅ";
            if (ch >= '싸' && ch <= '앃') return "ㅆ";
            if (ch >= '아' && ch <= '잏') return "ㅇ";
            if (ch >= '자' && ch <= '짛') return "ㅈ";
            if (ch >= '짜' && ch <= '찧') return "ㅉ";
            if (ch >= '차' && ch <= '칳') return "ㅊ";
            if (ch >= '카' && ch <= '킿') return "ㅋ";
            if (ch >= '타' && ch <= '팋') return "ㅌ";
            if (ch >= '파' && ch <= '핗') return "ㅍ";
            if (ch >= '하' && ch <= '힣') return "ㅎ";

            return ETC;
        }

        private class Tuple : System.IComparable<Tuple>
        {
            public readonly int id;
            public readonly string name;
            public readonly string displayName;

            public Tuple(int id, string name, string displayName)
            {
                this.id = id;
                this.name = name;
                this.displayName = displayName;
            }

            public int CompareTo(Tuple other)
            {
                // "기타"로 시작할 경우
                if (displayName.IndexOf(ETC) != -1)
                {
                    if (other.displayName.IndexOf(ETC) != -1)
                        return name.CompareTo(other.name);

                    return 1;
                }

                return displayName.CompareTo(other.displayName);
            }
        }
    }
}