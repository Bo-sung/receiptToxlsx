#define USE_NICKNAME_DATA

using System.Text.RegularExpressions;
using UnityEngine;

namespace Ragnarok
{
    public static class FilterUtils
    {
        /// <summary>
        /// 태국 사용 불가능 닉네임
        /// </summary>
        private static readonly char[] thaiNameFilter = { (char)0x0E50, (char)0x0E51, (char)0x0E52, (char)0x0E53, (char)0x0E54, (char)0x0E55, (char)0x0E56, (char)0x0E57, (char)0x0E58, (char)0x0E59, (char)0x0E5A, (char)0x0E5B };

        /// <summary>
        /// 랜덤 이름 반환
        /// </summary>
        public static string GetAutoNickname()
        {
#if USE_NICKNAME_DATA
            return AutoNickDataManager.Instance.RandomPop();
#else
            return System.Guid.NewGuid().ToString("N").Substring(0, 8);
#endif
        }

        public static string GetAutoNickname(LanguageType languageType)
        {
#if USE_NICKNAME_DATA
            return AutoNickDataManager.Instance.RandomPop(languageType);
#else
            return System.Guid.NewGuid().ToString("N").Substring(0, 8);
#endif
        }

        /// <summary>
        /// 닉네임 유효성 (유효하지 않으면 ErrorMessage 반환)
        /// </summary>
        public static string CheckCharacterName(string text)
        {
            // 빈 문자 사용 불가능
            if (string.IsNullOrEmpty(text))
                return LocalizeKey._90184.ToText(); // 빈 문자열은 사용할 수 없습니다.

            // 공백 사용 불가능
            if (text.Contains(" "))
                return LocalizeKey._90185.ToText(); // 공백이 포함되어 있습니다.

            // 불가능한 문자열
            string invalidText = ContainInvalidText(text);
            if (!string.IsNullOrEmpty(invalidText))
            {
                return LocalizeKey._90186.ToText() // 사용할 수 없는 문자가 포함되어 있습니다.\n{VALUE}
                    .Replace(ReplaceKey.VALUE, invalidText);
            }

            // [\u3040-\u30FC] // Japanese 
            // [\u4E00-\u9FFF] // Unified Hanja (Traditional/Simplify Chinese, Japanese, Korean) 
            // [\u0E00-\u0E7f] // 타이문자
            // [\u00C0-\u00FF] // Latin-1 Supplement
            // [\u0100-\u017F] // Latin Extended-A
            // [\u0180-\u024F] // Latin Extended-B
            // [\u1E00-\u1EFF] // Latin Extended Additional
            // [0-9a-zA-Z]     // 영문, 숫자
            // [가-힣]         // 완성형 한글
            if (!Regex.IsMatch(text, @"^[0-9a-zA-Z가-힣\u4E00-\u9FFF\u3040-\u30FC\u0E00-\u0E7f\u00C0-\u017F]+$"))
                return LocalizeKey._90187.ToText(); // 해당 이름은 사용할 수 없습니다.

            return string.Empty;
        }

        /// <summary>
        /// 채팅 필터링
        /// </summary>
        public static string ReplaceChat(string message)
        {
            return FilterChatDataManager.Instance.Replace(message);
        }

        /// <summary>
        /// 문자열에 "<T>"가 있을경우 언어테이블ID 참조
        /// ex) {0}-{1}hello world!{2}<T>1000,1001,10002
        /// </summary>
        public static string GetServerMessage(string message)
        {
            const string SEPARATOR = "<T>";

            string msg = message;

            // 원본 그대로 출력
            if (!msg.Contains(SEPARATOR))
                return msg;

            // 언어테이블ID 참조해 문자열 조합
            int start = msg.IndexOf(SEPARATOR);
            string format = msg.Substring(0, start);
            string[] args = msg.Substring(start + SEPARATOR.Length).Split(',');

            for (int i = 0; i < args.Length; i++)
            {
                // 방어코드 추가 (텍스트의 경우 함수 실행이 중단됨..)
                int resultID;
                bool isInt = int.TryParse(args[i], out resultID);

                if (isInt)
                {
                    args[i] = resultID.ToText();
                }
                else
                {
                    Debug.LogError($"언어테이블 참조방식인데, 키값대신 텍스트가 들어가있음. Message = {msg}");
                }
            }

            return string.Format(format, args);
        }

        private static string ContainInvalidText(string text)
        {
            // 특수기호 몇가지 사용 불가능
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] < 0x30)
                    return text[i].ToString();

                if (text[i] >= 0x3A && text[i] <= 0x40)
                    return text[i].ToString();

                if (text[i] >= 0x5B && text[i] <= 0x60)
                    return text[i].ToString();

                if (text[i] >= 0x7B && text[i] <= 0x7E)
                    return text[i].ToString();
            }

            // 태국 사용 불가능 닉네임
            foreach (char item in text)
            {
                for (int i = 0; i < thaiNameFilter.Length; i++)
                {
                    if (item.Equals(thaiNameFilter[i]))
                        return item.ToString();
                }
            }

            // 필터링 체크
            if (FilterNickDataManager.Instance.Contains(text))
                return text;

            return string.Empty;
        }
    }
}