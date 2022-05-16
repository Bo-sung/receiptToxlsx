using UnityEngine;

namespace Ragnarok
{
    public enum ChatMode
    {
        All,
        Channel,
        Guild,
        Whisper,
        Lobby,
        PrivateStoreChat, // 개인상점 내 미니채팅
    }

    public static class ChatModeExtensions
    {
        /// <summary>
        /// 챗모드 정렬 순서 (높을 수록 우선)
        /// </summary>
        public static int GetSortOrder(this ChatMode mode)
        {
            switch (mode)
            {
                case ChatMode.Lobby: return 4;
                case ChatMode.Guild: return 3;
                case ChatMode.Channel: return 2;
                case ChatMode.Whisper: return -1;
            }

            return 0;
        }

        public static string GetName(this ChatMode mode)
        {
            switch (mode)
            {
                case ChatMode.Channel: return LocalizeKey._29004.ToText(); // 자유 채팅 / 채널 {VALUE}
                case ChatMode.Guild: return LocalizeKey._29005.ToText(); // 길드 채팅 / {NAME}
                case ChatMode.Lobby: return LocalizeKey._29006.ToText(); // 거래소 채팅 / 채널 {VALUE}
            }

            Debug.LogError($"챗모드 이름 지정 필요 : {mode}");
            return nameof(mode);
        }
    }
}