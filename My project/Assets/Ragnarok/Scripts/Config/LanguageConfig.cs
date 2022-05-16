namespace Ragnarok
{
    public enum LanguageType
    {
        None        = -1,

        KOREAN      = 0,
        ENGLISH     = 1,
        TAIWAN      = 2,
        CHINA       = 3,
        JAPANESE    = 4,
        THAILAND    = 5,
        INDONESIAN  = 6,
        PHILIPPINES = 7,
        MALAYSIA    = 8,
        FRENCH      = 9,
        GERMAN      = 10,
        PORTUGUESE  = 11,
        SPANISH     = 12,
    }

    /**
     * 언어를 ISO 3166-1 alpha-2로 변환 (국가코드, https://ko.wikipedia.org/wiki/ISO_3166-1)
     * 언어를 ISO 639 alpha-2로 변환 (언어코드, https://ko.wikipedia.org/wiki/ISO_639)
     * 
     */
    public sealed class LanguageConfig : EnumBaseType<LanguageConfig, LanguageType, string>
    {
        public static readonly LanguageConfig KOREAN      = new LanguageConfig(LanguageType.KOREAN, "한국", "KR", "한국어");
        public static readonly LanguageConfig ENGLISH     = new LanguageConfig(LanguageType.ENGLISH, "영어", "EN", "English");
        public static readonly LanguageConfig TAIWAN      = new LanguageConfig(LanguageType.TAIWAN, "대만", "TW", "中文(繁體)");
        public static readonly LanguageConfig CHINA       = new LanguageConfig(LanguageType.CHINA, "중국", "CH", "中文(简体)");
        public static readonly LanguageConfig JAPANESE    = new LanguageConfig(LanguageType.JAPANESE, "일본", "JP", "日本語");
        public static readonly LanguageConfig THAILAND    = new LanguageConfig(LanguageType.THAILAND, "태국", "TH", "ภาษาไทย");
        public static readonly LanguageConfig INDONESIAN  = new LanguageConfig(LanguageType.INDONESIAN, "인도네시아", "ID", "Indonesia");
        public static readonly LanguageConfig PHILIPPINES = new LanguageConfig(LanguageType.PHILIPPINES, "필리핀", "PH", "Filipino");
        public static readonly LanguageConfig MALAYSIA    = new LanguageConfig(LanguageType.MALAYSIA, "말레이시아", "MY", "Bahasa Melayu");
        public static readonly LanguageConfig FRENCH      = new LanguageConfig(LanguageType.FRENCH, "프랑스", "FR", "Français");
        public static readonly LanguageConfig GERMAN      = new LanguageConfig(LanguageType.GERMAN, "독일", "DE", "Deutsch");
        public static readonly LanguageConfig PORTUGUESE  = new LanguageConfig(LanguageType.PORTUGUESE, "포르투갈", "PT", "Português");
        public static readonly LanguageConfig SPANISH     = new LanguageConfig(LanguageType.SPANISH, "스페인", "ES", "Español");

        public readonly string type;
        public readonly string language;

        public LanguageConfig(LanguageType key, string value, string type, string language) : base(key, value)
        {
            this.type = type;
            this.language = language;
        }

        public static LanguageConfig GetBytKey(LanguageType type)
        {
            return GetBaseByKey(type);
        }
    }
}