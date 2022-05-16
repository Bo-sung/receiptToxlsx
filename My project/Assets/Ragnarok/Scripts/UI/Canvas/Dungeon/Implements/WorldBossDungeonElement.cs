using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok.View
{
    public class WorldBossDungeonElement : DungeonElement
    {
        private readonly IDungeonWorldBossImpl impl;

        WorldBossMonsterInfo monsterInfo;
        (RewardInfo info, bool isBoss)[] rewardInfos;

        ObscuredInt curBossHp;
        ObscuredInt maxBossHp;
        ObscuredInt joinCharCount;
        RemainTime remainTime;

        public WorldBossDungeonElement(IDungeonGroup dungeonGroup, IDungeonWorldBossImpl impl) : base(dungeonGroup, impl)
        {
            this.impl = impl;
        }

        public WorldBossMonsterInfo GetMonsterInfo()
        {
            return monsterInfo ?? (monsterInfo = impl.GetMonsterInfos(dungeonGroup.GetMonsterInfos())[0]);
        }

        public (RewardInfo info, bool isBoss)[] GetRewardInfos()
        {
            return rewardInfos ?? (rewardInfos = dungeonGroup.GetRewardInfos());
        }

        public float GetBossHpProgress()
        {
            return curBossHp / (float)maxBossHp;
        }

        public bool IsOpen()
        {
            return remainTime.ToRemainTime() <= 0;
        }

        public int JoinCharCount => joinCharCount;

        public string RemainTimeText => remainTime.ToStringTime();

        public bool IsAlarm { get; private set; }

        public int MaxHp => maxBossHp;

        public string FreeTicketCoolTimeText => impl.GetFreeTicketCoolTime().ToStringTime();

        public void SetData(WorldBossInfoPacket packet)
        {
            curBossHp = packet.current_hp;
            maxBossHp = packet.max_hp;
            joinCharCount = packet.join_char_count;
            remainTime = packet.remain_time;
            InvokeUpdateEvent();
        }

        public bool IsSelect(int id)
        {
            return Id == id;
        }

        public void SetAlarm(bool isAlarm)
        {
            this.IsAlarm = isAlarm;
        }

        public bool IsAlarmWorldBoss()
        {
            return impl.IsAlarm(Id);
        }

        public bool CanEnter(bool isShowNotice)
        {
            return impl.CanEnter(DungeonType, Id, isShowNotice);
        }

        public int GetMaxLevel()
        {
            return impl.GetMaxLevel();
        }
    }
}