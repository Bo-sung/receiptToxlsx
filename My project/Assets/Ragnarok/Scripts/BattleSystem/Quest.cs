namespace Ragnarok
{
    public static class Quest
    {
        public delegate void ProgressEvent(QuestType type, int conditionValue, int count);

        private static ProgressEvent onProgress;
        public static event ProgressEvent OnProgress
        {
            add { onProgress += value; }
            remove { onProgress -= value; }
        }

        /// <summary>
        /// 진행도 증가
        /// </summary>
        public static void QuestProgress(QuestType type, int conditionValue = 0, int questValue = 1)
        {
            if (DebugUtils.IsLogQuestProgress)
                UnityEngine.Debug.Log($"[퀘스트 진행] {nameof(type)} = ({(int)type}){type}, {nameof(conditionValue)} = {conditionValue}, {nameof(questValue)} = {questValue}");
            onProgress?.Invoke(type, conditionValue, questValue);
        }

        public static bool IsMaxCondition(this QuestType type)
        {
            switch (type)
            {
                case QuestType.ITEM_UPGRADE_MAX:
                case QuestType.CHARACTER_MAX_JOB_LEVEL:
                case QuestType.CHARACTER_MAX_BASIC_LEVEL:
                    return true;
            }

            return false;
        }
    }
}