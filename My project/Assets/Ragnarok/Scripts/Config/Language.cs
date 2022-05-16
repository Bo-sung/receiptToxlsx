using UnityEngine;

namespace Ragnarok
{
    public static class Language
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            LanguageType langType;
            if (PlayerPrefs.HasKey(Config.LANGUAGE_TYPE))
            {
                langType = PlayerPrefs.GetInt(Config.LANGUAGE_TYPE).ToEnum<LanguageType>();
            }
            else
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Korean:
                        langType = LanguageType.KOREAN;
                        break;

                    default:
                    case SystemLanguage.English:
                        langType = LanguageType.ENGLISH;
                        break;

                    case SystemLanguage.ChineseTraditional:
                        langType = LanguageType.TAIWAN;
                        break;

                    case SystemLanguage.Chinese:
                    case SystemLanguage.ChineseSimplified:
                        langType = LanguageType.CHINA;
                        break;

                    case SystemLanguage.Japanese:
                        langType = LanguageType.JAPANESE;
                        break;

                    case SystemLanguage.Thai:
                        langType = LanguageType.THAILAND;
                        break;

                    case SystemLanguage.Indonesian:
                        langType = LanguageType.INDONESIAN;
                        break;

                    case SystemLanguage.French:
                        langType = LanguageType.FRENCH;
                        break;

                    case SystemLanguage.German:
                        langType = LanguageType.GERMAN;
                        break;

                    case SystemLanguage.Portuguese:
                        langType = LanguageType.PORTUGUESE;
                        break;

                    case SystemLanguage.Spanish:
                        langType = LanguageType.SPANISH;
                        break;
                }

                SaveLanguageType(langType);
            }

            Current = langType;
        }

        public static LanguageType Current { get; private set; }

        public static void SetLanguageType(LanguageType type)
        {
            Current = type;
            SaveLanguageType(type);

            UIManager.Instance.Localize();
        }

        private static void SaveLanguageType(LanguageType type)
        {
            PlayerPrefs.SetInt(Config.LANGUAGE_TYPE, (int)type);
        }
    }
}