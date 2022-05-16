namespace Ragnarok
{
    public class EventQuestInfo : IPacket<Response>
    {
        /// <summary>
        /// 이벤트 퀘스트 정보
        /// </summary>
        public EventInfo eventInfo;

        /// <summary>
        /// 진행도
        /// </summary>
        public int progress;

        /// <summary>
        /// 보상 수령 정보
        /// </summary>
        public bool receive;        


        void IInitializable<Response>.Initialize(Response response)
        {
            eventInfo = response.GetPacket<EventInfo>("1");
            progress = response.GetInt("2");
            receive = response.GetBool("3");           
        }

        public class EventInfo : IPacket<Response>
        {
            public int event_no;
            public long end_dt;
            public int quest_id;
            /// <summary>
            /// 이벤트 그룹ID
            /// 배너랑 매칭
            /// </summary>
            public int eventGroupId;

            /// <summary>
            /// 초기화 타입
            /// </summary>
            public byte initType; // 1일결우 일일 초기화

            void IInitializable<Response>.Initialize(Response response)
            {
                event_no = response.GetInt("1");
                end_dt = response.GetLong("2");
                quest_id = response.GetInt("3");
                eventGroupId = response.GetInt("4");
                initType = response.GetByte("5");
            }
        }
    }
}