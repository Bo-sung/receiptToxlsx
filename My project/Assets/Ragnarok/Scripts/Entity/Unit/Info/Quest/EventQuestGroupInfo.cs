using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public interface IEventQuestGroupInfoImpl
    {
        bool IsEventQuestStandByReward(int groupId);
    }

    public interface IEventQuestGroupInfoOnBuffImpl
    {
        long GetOnBuffTotalRemainPoint();
    }

    public class EventQuestGroupInfo : IInfo, IUIData, UIBannerElement.IInput
    {
        IEventQuestGroupInfoImpl impl;
        IEventQuestGroupInfoOnBuffImpl onBuffImpl;

        bool IInfo.IsInvalidData => false; // 무조건 유효한 데이터
        public event System.Action OnUpdateEvent;

        private ObscuredInt id;
        private string name;
        private string descripsion;
        private string imageUrl;
        private ObscuredByte initType; // 0:초기화 없음, 1: 일일 초기화, 2:기간 초기화
        private RemainTime remainTime; // 남은 시간

        public int GroupId => id;

        public RemainTime RemainTime => remainTime;

        public string Name => FilterUtils.GetServerMessage(name);
        public string Description => FilterUtils.GetServerMessage(descripsion);
        public string ImageUrl => imageUrl;
        public ShortCutType ShortcutType { get; private set; }
        public int ShortcutValue { get; private set; }

        public bool IsEventQuestStandByReward()
        {
            return impl.IsEventQuestStandByReward(GroupId);
        }

        public void Initialize(EventQuestGroupPacket packet, string imageUrl)
        {
            id = packet.id;
            name = packet.name;
            descripsion = packet.description;
            initType = packet.initType;
            remainTime = packet.remainTime;
            ShortcutType = packet.shortcutType;
            ShortcutValue = packet.shortcutValue;

            this.imageUrl = imageUrl;

            OnUpdateEvent?.Invoke();
        }

        public void Set(IEventQuestGroupInfoImpl impl)
        {
            this.impl = impl;
        }

        public void Set(IEventQuestGroupInfoOnBuffImpl impl)
        {
            onBuffImpl = impl;
        }

        public long GetOnBuffTotalRemainPoint()
        {
            return onBuffImpl.GetOnBuffTotalRemainPoint();
        }
    }
}