namespace Ragnarok
{
    public sealed class EventQuestGroupPacket : IPacket<Response>
    {
        private static readonly char[] SEPARATOR = { ',' };

        public int id;
        public string name;
        public string description;
        public string imageName;
        public byte initType; // 0:초기화 없음, 1: 일일 초기화, 2:기간 초기화, 3: UILink
        public long remainTime; // 남은 시간
        public ShortCutType shortcutType;
        public int shortcutValue;
        public int sort;

        void IInitializable<Response>.Initialize(Response response)
        {
            id = response.GetInt("1");
            name = response.GetUtfString("2");
            description = response.GetUtfString("3");
            imageName = response.GetUtfString("4");
            initType = response.GetByte("5");
            remainTime = response.GetLong("6");

            if (response.ContainsKey("7"))
            {
                string link = response.GetUtfString("7");
                string[] results = link.Split(SEPARATOR, System.StringSplitOptions.RemoveEmptyEntries);

                if (results.Length > 0)
                    shortcutType = int.Parse(results[0]).ToEnum<ShortCutType>();

                if (results.Length > 1)
                    shortcutValue = int.Parse(results[1]);
            }

            sort = response.GetInt("8");
        }
    }
}