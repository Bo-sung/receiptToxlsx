namespace Ragnarok
{
    public static class DebugUtils
    {
        private enum Log
        {
            ZenyLog,
            ExpLog,
            BattleScoreLog,
            SkillCoolTime,
            Damage,
            AutoStageDrop,
            VerifyData,
            DamagePacket,
            ResoureceData,
            BossMonster,
            UseSkillLog,
            MonsterDropKey,
            BoxRewardLog,
            QuestProgress,
        }

        public static bool IsLogZeny
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.ZenyLog), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.ZenyLog), value);
#else
            get => false;
#endif
        }

        public static bool IsLogExp
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.ExpLog), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.ExpLog), value);
#else
            get => false;
#endif
        }

        public static bool IsLogBattleLog
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.BattleScoreLog), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.BattleScoreLog), value);
#else
            get => false;
#endif
        }

        public static bool IsLogSkillCoolTime
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.SkillCoolTime), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.SkillCoolTime), value);
#else
            get => false;
#endif
        }

        public static bool IsLogDamage
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.Damage), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.Damage), value);
#else
            get => false;
#endif
        }

        public static bool IsLogAutoStageDrop
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.AutoStageDrop), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.AutoStageDrop), value);
#else
            get => false;
#endif
        }

        public static bool IsLogVerifyData
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.VerifyData), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.VerifyData), value);
#else
            get => false;
#endif
        }

        public static bool IsLogDamagePacket
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.DamagePacket), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.DamagePacket), value);
#else
            get => false;
#endif
        }

        public static bool IsLogResoureceData
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.ResoureceData), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.ResoureceData), value);
#else
            get => false;
#endif
        }

        public static bool IsLogBossMonster
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.BossMonster), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.BossMonster), value);
#else
            get => false;
#endif
        }

        public static bool IsLogUseSkill
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.UseSkillLog), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.UseSkillLog), value);
#else
            get => false;
#endif
        }

        public static bool IsMonsterDropKey
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.MonsterDropKey), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.MonsterDropKey), value);
#else
            get => false;
#endif
        }

        public static bool IsLogBoxReward
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.BoxRewardLog), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.BoxRewardLog), value);
#else
            get => false;
#endif
        }

        public static bool IsLogQuestProgress
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Log.QuestProgress), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Log.QuestProgress), value);
#else
            get => false;
#endif
        }
    }
}