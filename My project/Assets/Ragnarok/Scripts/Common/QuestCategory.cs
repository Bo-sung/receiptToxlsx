using UnityEngine;

namespace Ragnarok
{
    public enum QuestCategory
    {
        /// <summary>
        /// 업적 퀘스트
        /// </summary>
        Achieve = 1,

        /// <summary>
        /// 메인 퀘스트
        /// </summary>
        Main = 2,

        /// <summary>
        /// 일일 퀘스트
        /// </summary>
        DailyStart = 3,

        /// <summary>
        /// 이벤트 퀘스트
        /// </summary>
        Event = 4,

        /// <summary>
        /// 길드 퀘스트
        /// </summary>
        Guild = 5,

        /// <summary>
        /// 의뢰 퀘스트
        /// </summary>
        Normal = 6,

        /// <summary>
        /// 빙고 퀘스트
        /// </summary>
        Bingo = 7,

        /// <summary>
        /// 타임 패트롤 퀘스트
        /// </summary>
        TimePatrol = 8,

        /// <summary>
        /// 패스 일일 퀘스트
        /// </summary>
        PassDaily = 9,

        /// <summary>
        /// 패스 시즌 퀘스트
        /// </summary>
        PassSeason = 10,

        /// <summary>
        /// 온버프 패스 일일 퀘스트
        /// </summary>
        OnBuffPassDaily = 11,
    }

    public static class QuestCategoryExtensions
    {
        public static string ToText(this QuestCategory category)
        {
            switch (category)
            {
                case QuestCategory.Achieve:
                    return LocalizeKey._52000.ToText(); // 업적

                case QuestCategory.Main:
                    return LocalizeKey._52001.ToText(); // 메인

                case QuestCategory.DailyStart:
                    return LocalizeKey._52002.ToText(); // 일일

                case QuestCategory.Event:
                    return LocalizeKey._52003.ToText(); // 이벤트

                case QuestCategory.Guild:
                    return LocalizeKey._52004.ToText(); // 길드

                case QuestCategory.Normal:
                    return LocalizeKey._52005.ToText(); // 의뢰

                case QuestCategory.PassDaily:
                    return LocalizeKey._52008.ToText(); // 패스

                case QuestCategory.PassSeason:
                    return LocalizeKey._52009.ToText(); // 패스 시즌

                case QuestCategory.OnBuffPassDaily:
                    return LocalizeKey._52010.ToText(); // OnBuff 패스

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(QuestCategory)}] {nameof(category)} = {category}");
                    return string.Empty;
            }
        }
    }
}