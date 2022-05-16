using UnityEngine;

namespace Ragnarok
{
    public static class BasisExtensions
    {
        /// <summary>
        /// 아이템 id 반환
        /// </summary>
        public static int GetID(this BasisItem input)
        {
            return BasisType.REF_ITEM_ID.GetInt((int)input);
        }

        /// <summary>
        /// 상점 id 반환
        /// </summary>
        public static int GetID(this BasisShop input)
        {
            return BasisType.REF_SHOP_ID.GetInt((int)input);
        }

        /// <summary>
        /// 미궁숲 정보 반환
        /// </summary>
        public static int GetInt(this BasisForestMazeInfo input)
        {
            return BasisType.REF_FOREST_MAZE_INFO.GetInt((int)input);
        }

        /// <summary>
        /// 길드전 정보 반환
        /// </summary>
        public static int GetInt(this BasisGuildWarInfo input)
        {
            return BasisType.REF_GUILD_WAR_INFO.GetInt((int)input);
        }

        /// <summary>
        /// 큐펫 정보 반환
        /// </summary>
        public static int GetInt(this BasisCupetInfo input)
        {
            return BasisType.REF_CUPET_INFO.GetInt((int)input);
        }

        /// <summary>
        /// 이벤트미궁:암흑 정보 반환
        /// </summary>
        public static int GetInt(this BasisDarkMazeInfo input)
        {
            return BasisType.REF_DARK_MAZE_INFO.GetInt((int)input);
        }

        /// <summary>
        /// 듀얼:아레나 정보 반환
        /// </summary>
        public static int GetInt(this BasisDuelArenaInfo input)
        {
            return BasisType.REF_DUEL_ARENA_INFO.GetInt((int)input);
        }

        /// <summary>
        /// Url 주소 오픈
        /// </summary>
        public static void OpenUrl(this BasisUrl input)
        {
            string url = GetUrl(input);
            if (string.IsNullOrWhiteSpace(url))
                return;

            Application.OpenURL(url); // url 페이지로 이동
        }

        /// <summary>
        /// Url 주소 반환
        /// </summary>
        private static string GetUrl(this BasisUrl input)
        {
            LanguageConfig config = LanguageConfig.GetBytKey(Language.Current);
            string countryCode = config == null ? string.Empty : config.type;

            return BasisType.REF_URL.GetString((int)input)
                .Replace(ReplaceKey.LANG, countryCode);
        }

        /// <summary>
        /// 뒤에 [자세히 보기] 추가
        /// 상품 철회 관련 => 상품 철회 관련 [u][url={URL_LINK}][자세히 보기][/url][/u]
        /// </summary>
        public static string AppendText(this BasisUrl input, string text, bool useColor = false)
        {
            if (input == default)
                return text;

            int LOCAL_KEY = LocalizeKey._90307; // [자세히 보기]

            switch (input)
            {
                case BasisUrl.KoreanElementInfo:
                    LOCAL_KEY = LocalizeKey._90318; // [속성 상성 확인]
                    break;
                case BasisUrl.KoreanDisassemble:
                    LOCAL_KEY = LocalizeKey._90319; // [분해 아이템 보기]
                    break;
                case BasisUrl.KoreanCardQuality:
                    LOCAL_KEY = LocalizeKey._90320; // [카드 퀄리티 확률]
                    break;
                case BasisUrl.KoreanAgentCompose:
                    LOCAL_KEY = LocalizeKey._90321; // [동료 합성 확률]
                    break;
                case BasisUrl.OnBuffHompage:
                    LOCAL_KEY = LocalizeKey._90325; // [UID 확인하기]
                    break;
            }

            if (useColor)
            {
                return StringBuilderPool.Get()
                    .Append(text)
                    .Append(" [u][url={URL:").Append((int)input).Append("}][c][64A2EE]").Append(LOCAL_KEY.ToText()).Append("[-][/c][/url][/u]")
                    .Release();
            }

            return StringBuilderPool.Get()
                .Append(text)
                .Append(" [u][url={URL:").Append((int)input).Append("}]").Append(LOCAL_KEY.ToText()).Append("[/url][/u]")
                .Release();
        }

        /// <summary>
        /// 컨텐츠 오픈 여부
        /// </summary>
        public static bool IsOpend(this BasisOpenContetsType type)
        {
            const int FLAG_OPEN = 0;
            const int FLAG_UPDATE = 1;
            return BasisType.CONTENT_OPEN.GetInt((int)type) == FLAG_OPEN;
        }

        /// <summary>
        /// 온버프 정보 반환
        /// </summary>
        public static int GetInt(this BasisOnBuffInfo input)
        {
            return BasisType.REF_ONBUFF_INFO.GetInt((int)input);
        }

        /// <summary>
        /// 프로젝트 별 언어 분기
        /// </summary>
        public static int GetInt(this BasisProjectTypeLocalizeKey input)
        {
            return BasisType.PROJECT_TYPE_LOCALIZE_KEY.GetInt((int)input);
        }
    }
}