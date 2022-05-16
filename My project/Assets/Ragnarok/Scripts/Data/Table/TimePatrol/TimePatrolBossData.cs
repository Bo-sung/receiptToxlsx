using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="TimePatrolBossDataManager"/>
    /// </summary>
    public class TimePatrolBossData : IData, IBossMonsterSpawnData, UIBossComing.IInput
    {
        public ObscuredInt id;
        public ObscuredInt level;
        public ObscuredInt boss_monster_scale;
        public ObscuredInt boss_monster_level;
        public ObscuredInt boss_monster_cooltime;
        public ObscuredInt boss_id;
        public ObscuredInt boss_drop_rate;
        public ObscuredInt boss_drop;

        int IBossMonsterSpawnData.BossMonsterId => boss_id;
        int IBossMonsterSpawnData.Level => boss_monster_level;
        float IBossMonsterSpawnData.Scale => MathUtils.ToPercentValue(boss_monster_scale);

        public TimePatrolBossData(IList<MessagePackObject> data)
        {
            int index = 0;
            id                    = data[index++].AsInt32();
            level                 = data[index++].AsInt32();
            boss_monster_scale    = data[index++].AsInt32();
            boss_monster_level    = data[index++].AsInt32();
            boss_monster_cooltime = data[index++].AsInt32();
            boss_id               = data[index++].AsInt32();
            boss_drop_rate        = data[index++].AsInt32();
            boss_drop             = data[index++].AsInt32();
        }

        int UIBossComing.IInput.GetMonsterId()
        {
            return boss_id;
        }

        string UIBossComing.IInput.GetDescription()
        {
            return LocalizeKey._39302.ToText(); // 보스 출현!
        }

        string UIBossComing.IInput.GetSpriteName()
        {
            return "Ui_Common_Icon_Boss4";
        }
    }
}