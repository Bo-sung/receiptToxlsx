namespace Ragnarok
{
    public static class GenderExtensions
    {
        private const string MALE_TEXT = "M";
        private const string FEMALE_TEXT = "F";

        /// <summary>
        /// 접두사 추가
        /// "Text" => "MText" or "FText"
        /// </summary>
        public static string AddPrefix(this string text, Gender gender)
        {
            return string.Concat(gender.ToText(), text);
        }

        /// <summary>
        /// 접미사 추가
        /// "Text" => "TextM" or "TextF"
        /// </summary>
        public static string AddPostfix(this string text, Gender gender)
        {
            return string.Concat(text, gender.ToText());
        }

        public static string ToText(this Gender gender)
        {
            switch (gender)
            {
                case Gender.Male: return MALE_TEXT;
                case Gender.Female: return FEMALE_TEXT;
            }

            throw new System.ArgumentException($"정의되지 않은 타입: {nameof(gender)} = {gender}");
        }
    }
}