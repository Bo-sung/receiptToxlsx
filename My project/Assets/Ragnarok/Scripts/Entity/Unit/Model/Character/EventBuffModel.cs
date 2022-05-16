using System;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class EventBuffModel : CharacterEntityModel
    {
        //private RemainTime remainEventTime; // 남은 이벤트 시간

        private EventBuffInfo eventBuffInfo; // GM 이벤트 버프
        private EventBuffInfo duelBuffInfo; // 월드 듀얼 보상 버프


        public event Action OnUpdateEventBuff;

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.REQUEST_PING.AddEvent(OnReceiveEventBuff);
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.REQUEST_PING.RemoveEvent(OnReceiveEventBuff);
            }
        }

        internal void Initialize(EventBuffPacket eventBuffPacket)
        {
            if (eventBuffPacket == null)
            {
                //remainEventTime = 0f;
                eventBuffInfo = null;
            }
            else
            {
                //remainEventTime = eventBuffPacket.leftTime;
                eventBuffInfo = eventBuffPacket.eventBuff == null ? null : new EventBuffInfo(eventBuffPacket.eventBuff, eventBuffPacket.leftTime);
            }

            OnUpdateEventBuff?.Invoke();
        }

        internal void Initialize(EventDuelBuffPacket duelBuffPacket)
        {
            if (duelBuffPacket == null)
            {
                duelBuffInfo = null;
            }
            else
            {
                duelBuffInfo = new EventBuffInfo(duelBuffPacket);
            }

            OnUpdateEventBuff?.Invoke();
        }

        private void OnReceiveEventBuff(Response response)
        {
            if (response.isSuccess)
            {
                EventBuffPacket eventBuffPacket = response.ContainsKey("eb") ? response.GetPacket<EventBuffPacket>("eb") : null;
                Notify(eventBuffPacket);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        // 현재 진행중인 이벤트 버프
        public EventBuffInfo[] GetEventBuffList()
        {
            List<EventBuffInfo> buffList = new List<EventBuffInfo>();

            // GM 이벤트버프
            if (eventBuffInfo != null && eventBuffInfo.remainTime > 0)
                buffList.Add(new EventBuffInfo(eventBuffInfo));

            // 듀얼 이벤트버프
            if (duelBuffInfo != null && duelBuffInfo.remainTime > 0)
                buffList.Add(new EventBuffInfo(duelBuffInfo));

            return buffList.ToArray();
        }
    }
}