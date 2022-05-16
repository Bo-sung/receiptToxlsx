namespace Ragnarok
{
    public class EventBannerData : IEventBanner
    {
        public int Seq { get; private set; } // 배너 고유번호
        public string Url { get; private set; }
        public ShortCutType ShortcutType { get; private set; }
        public int ShortcutValue { get; private set; }
        public string Description { get; private set; }
        public System.DateTime StartTime { get; private set; }
        public System.DateTime EndTime { get; private set; }
        public string TextureName { get; private set; }
        public int Pos { get; private set; } // 배너 우선순위 (순서)
        public RemainTime RemainTime { get; private set; } // 남은 시간 (0 아래로 떨어지면 배너 비활성화)
        public string TextureUrl { get; private set; }

        public void Initialize(EventBannerPacket packet, string textureUrl)
        {
            string url;
            string shortcutValueText;

            if (packet.url.Contains("_"))
            {
                string[] strs = packet.url.Split('_');
                url = strs[0];
                shortcutValueText = strs[1];
            }
            else
            {
                url = packet.url;
                shortcutValueText = null;
            }

            int.TryParse(url, out int shortcutType);

            int shortcutValue;
            if (string.IsNullOrEmpty(shortcutValueText))
            {
                shortcutValue = 0;
            }
            else
            {
                int.TryParse(shortcutValueText, out shortcutValue);
            }

            Seq = packet.seq;
            Url = url;
            ShortcutType = shortcutType.ToEnum<ShortCutType>();
            ShortcutValue = shortcutValue;
            Description = packet.description;
            StartTime = packet.startTime.ToDateTime();
            EndTime = packet.endTime.ToDateTime();
            TextureName = packet.textureName;
            Pos = packet.pos.ToIntValue();
            RemainTime = packet.remain_time;
            TextureUrl = textureUrl;
        }
    }
}