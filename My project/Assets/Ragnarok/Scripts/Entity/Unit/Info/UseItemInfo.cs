using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    /// <summary>
    /// 사용중인 아이템 정보
    /// </summary>
    public class UseItemInfo : DataInfo<ItemData>, IInitializable<BuffItemListModel.IInputBuffValue>, BattleBuffItemInfo.ISettings, IBuff, BuffItemListModel.IInputBuffValue
    {
        public ObscuredInt CID { get; private set; }
        private RemainTime coolDown;
        private RemainTime duration;

        /// <summary>
        /// 끝나는 시간 (버프 종료 시간)
        /// </summary>
        private RemainTime endTime;

        public int ItemId => data.id;

        /// <summary>
        /// 재사용 대기시간
        /// </summary>
        public float CoolDown => data.cooldown;

        /// <summary>
        /// 남은 재사용 대기시간
        /// </summary>
        public float RemainCoolDown => coolDown.ToRemainTime();

        /// <summary>
        /// 남은 지속시간
        /// </summary>
        public float RemainDuration => duration.ToRemainTime();

        /// <summary>
        /// 아이콘 이름
        /// </summary>
        public string IconName => data.icon_name;        

        /// <summary>
        /// 데이터
        /// </summary>
        ItemData BattleBuffItemInfo.ISettings.Data => data;

        /// <summary>
        /// 시간 제한 없는 버프
        /// </summary>
        public bool IsInfinity;

        int BuffItemListModel.IInputBuffValue.Cid => CID;
        long BuffItemListModel.IInputBuffValue.CoolDown => (long)RemainCoolDown;
        long BuffItemListModel.IInputBuffValue.Duration => (long)RemainDuration;

        public void Initialize(BuffItemListModel.IInputBuffValue packet)
        {
            CID = packet.Cid;
            UpdateTime(packet.CoolDown, packet.Duration);
        }

        public void UpdateTime(long coolDown, long duration)
        {
            this.coolDown = coolDown;
            this.duration = duration;
            endTime = duration;
        }

        /// <summary>
        /// 유효성
        /// </summary>
        public bool IsValid()
        {
            // 남아있는 쿨타임 시간이 존재하거나, 남아있는 지속 시간이 존재할 경우
            return coolDown.ToRemainTime() > 0 || duration.ToRemainTime() > 0;
        }

        /// <summary>
        /// 진행도
        /// </summary>
        public float GetProgress()
        {
            if (IsInfinity)
                return 0f;

            float duration = GetDuration();
            if (duration == 0f)
                return 1f;

            float remainTime = endTime.ToRemainTime() * 0.001f;
            if (remainTime == 0f)
                return 1f;

            return 1 - (remainTime / duration);
        }

        /// <summary>
        /// 지속시간 반환
        /// </summary>
        private float GetDuration()
        {
            if (data.duration == 0)
                return 0f;

            return data.duration * 0.001f;
        }
    }
}