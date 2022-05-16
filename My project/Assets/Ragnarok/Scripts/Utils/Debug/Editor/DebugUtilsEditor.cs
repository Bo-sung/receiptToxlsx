using UnityEditor;

namespace Ragnarok
{
    public static class DebugUtilsEditor
    {
        private const string BASE_PATH = "라그나로크/로그/";
        private const string ZENY_PATH = BASE_PATH + "제니 보기";
        private const string EXP_PATH = BASE_PATH + "경험치 보기";
        private const string BATTLE_SCORE_PATH = BASE_PATH + "전투력 보기";
        private const string SKILL_COOLTIME_PATH = BASE_PATH + "스킬 쿨타임 보기";
        private const string DAMAGE_PATH = BASE_PATH + "서버대미지 비교 보기";
        private const string AUTO_STAGE_DROP_PATH = BASE_PATH + "자동사냥 몬스터드랍 보기";
        private const string VERIFY_DATA_PATH = BASE_PATH + "테이블 검증 보기";
        private const string DAMAGE_PACKET_PATH = BASE_PATH + "대미지패킷 확인";
        private const string RESOURECE_DATA_PATH = BASE_PATH + "테이블 로그";
        private const string BOSS_MONSTER_PATH = BASE_PATH + "보스몬스터 로그";
        private const string USE_SKILL_PATH = BASE_PATH + "스킬사용 로그";
        private const string MONSTER_DROP_KEY_PATH = BASE_PATH + "몬스터 드랍 고유값 로그";
        private const string BOX_REWARD_INFO_PATH = BASE_PATH + "상자 보상 정보 로그";
        private const string QUEST_PROGRESS_PATH = BASE_PATH + "퀘스트 진행도 로그";

        [MenuItem(ZENY_PATH)]
        private static void ToggleZeny()
        {
            DebugUtils.IsLogZeny = !DebugUtils.IsLogZeny;
        }

        [MenuItem(ZENY_PATH, validate = true)]
        private static bool ToggleZenyValidate()
        {
            Menu.SetChecked(ZENY_PATH, DebugUtils.IsLogZeny);
            return true;
        }

        [MenuItem(EXP_PATH)]
        private static void ToggleExp()
        {
            DebugUtils.IsLogExp = !DebugUtils.IsLogExp;
        }

        [MenuItem(EXP_PATH, validate = true)]
        private static bool ToggleExpValidate()
        {
            Menu.SetChecked(EXP_PATH, DebugUtils.IsLogExp);
            return true;
        }

        [MenuItem(BATTLE_SCORE_PATH)]
        private static void ToggleBattleScore()
        {
            DebugUtils.IsLogBattleLog = !DebugUtils.IsLogBattleLog;
        }

        [MenuItem(BATTLE_SCORE_PATH, validate = true)]
        private static bool ToggleBattleScoreValidate()
        {
            Menu.SetChecked(BATTLE_SCORE_PATH, DebugUtils.IsLogBattleLog);
            return true;
        }

        [MenuItem(SKILL_COOLTIME_PATH)]
        private static void ToggleSkillCoolTime()
        {
            DebugUtils.IsLogSkillCoolTime = !DebugUtils.IsLogSkillCoolTime;
        }

        [MenuItem(SKILL_COOLTIME_PATH, validate = true)]
        private static bool ToggleSkillCoolTimeValidate()
        {
            Menu.SetChecked(SKILL_COOLTIME_PATH, DebugUtils.IsLogSkillCoolTime);
            return true;
        }

        [MenuItem(DAMAGE_PATH)]
        private static void ToggleDamage()
        {
            DebugUtils.IsLogDamage = !DebugUtils.IsLogDamage;
        }

        [MenuItem(DAMAGE_PATH, validate = true)]
        private static bool ToggleDamageValidate()
        {
            Menu.SetChecked(DAMAGE_PATH, DebugUtils.IsLogDamage);
            return true;
        }

        [MenuItem(AUTO_STAGE_DROP_PATH)]
        private static void ToggleAutoStageDrop()
        {
            DebugUtils.IsLogAutoStageDrop = !DebugUtils.IsLogAutoStageDrop;
        }

        [MenuItem(AUTO_STAGE_DROP_PATH, validate = true)]
        private static bool ToggleAutoStageDropValidate()
        {
            Menu.SetChecked(AUTO_STAGE_DROP_PATH, DebugUtils.IsLogAutoStageDrop);
            return true;
        }

        [MenuItem(VERIFY_DATA_PATH)]
        private static void ToggleVerifyData()
        {
            DebugUtils.IsLogVerifyData = !DebugUtils.IsLogVerifyData;
        }

        [MenuItem(VERIFY_DATA_PATH, validate = true)]
        private static bool ToggleVerifyDataValidate()
        {
            Menu.SetChecked(VERIFY_DATA_PATH, DebugUtils.IsLogVerifyData);
            return true;
        }

        [MenuItem(DAMAGE_PACKET_PATH)]
        private static void ToggleDamagePacket()
        {
            DebugUtils.IsLogDamagePacket = !DebugUtils.IsLogDamagePacket;
        }

        [MenuItem(DAMAGE_PACKET_PATH, validate = true)]
        private static bool ToggleDamagePacketValidate()
        {
            Menu.SetChecked(DAMAGE_PACKET_PATH, DebugUtils.IsLogDamagePacket);
            return true;
        }

        [MenuItem(RESOURECE_DATA_PATH)]
        private static void ToggleResoureceData()
        {
            DebugUtils.IsLogResoureceData = !DebugUtils.IsLogResoureceData;
        }

        [MenuItem(RESOURECE_DATA_PATH, validate = true)]
        private static bool ToggleResoureceDataValidate()
        {
            Menu.SetChecked(RESOURECE_DATA_PATH, DebugUtils.IsLogResoureceData);
            return true;
        }

        [MenuItem(BOSS_MONSTER_PATH)]
        private static void ToggleBossMonster()
        {
            DebugUtils.IsLogBossMonster = !DebugUtils.IsLogBossMonster;
        }

        [MenuItem(BOSS_MONSTER_PATH, validate = true)]
        private static bool ToggleBossMonsterValidate()
        {
            Menu.SetChecked(BOSS_MONSTER_PATH, DebugUtils.IsLogBossMonster);
            return true;
        }

        [MenuItem(USE_SKILL_PATH)]
        private static void ToggleUseSkill()
        {
            DebugUtils.IsLogUseSkill = !DebugUtils.IsLogUseSkill;
        }

        [MenuItem(USE_SKILL_PATH, validate = true)]
        private static bool ToggleUseSkillValidate()
        {
            Menu.SetChecked(USE_SKILL_PATH, DebugUtils.IsLogUseSkill);
            return true;
        }

        [MenuItem(MONSTER_DROP_KEY_PATH)]
        private static void ToggleMonsterDropKey()
        {
            DebugUtils.IsMonsterDropKey = !DebugUtils.IsMonsterDropKey;
        }

        [MenuItem(MONSTER_DROP_KEY_PATH, validate = true)]
        private static bool ToggleMonsterDropKeyValidate()
        {
            Menu.SetChecked(MONSTER_DROP_KEY_PATH, DebugUtils.IsMonsterDropKey);
            return true;
        }

        [MenuItem(BOX_REWARD_INFO_PATH)]
        private static void ToggleBoxReward()
        {
            DebugUtils.IsLogBoxReward = !DebugUtils.IsLogBoxReward;
        }

        [MenuItem(BOX_REWARD_INFO_PATH, validate = true)]
        private static bool ToggleBoxRewardValidate()
        {
            Menu.SetChecked(BOX_REWARD_INFO_PATH, DebugUtils.IsLogBoxReward);
            return true;
        }

        [MenuItem(QUEST_PROGRESS_PATH)]
        private static void ToggleQuestProgress()
        {
            DebugUtils.IsLogQuestProgress = !DebugUtils.IsLogQuestProgress;
        }

        [MenuItem(QUEST_PROGRESS_PATH, validate = true)]
        private static bool ToggleQuestProgressValidate()
        {
            Menu.SetChecked(QUEST_PROGRESS_PATH, DebugUtils.IsLogQuestProgress);
            return true;
        }
    }
}