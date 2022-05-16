namespace Ragnarok
{
    public sealed class QuestPacket : IPacket<Response>
    {
        /// <summary>일퀘 퀘스트 목록 및 보상 수령 정보</summary>
        public CharQuestDaily[] charQuestDailies;
        /// <summary>일일 퀘스트 진행 정보</summary>
        public CharQuestDailyProgress[] charQuestDailyProgresses;
        /// <summary>업적 퀘스트 진행 정보</summary>
        public CharAchieveProgress[] charAchieveProgresses;
        /// <summary>업적 퀘스트 보상 수령 퀘스트 아이디</summary>
        public int[] charAchieveIds;
        /// 퀘스트 그룹정보(이벤트 배너)</summary>
        public EventQuestGroupPacket[] eventquestGroupInfos;
        /// <summary>이벤트 퀘스트 정보</summary>
        public EventQuestInfo[] eventQuestInfos;
        /// <summary>진행중인 메인 퀘스트 시퀀스</summary>
        public short mainQuestSeq;
        /// <summary>메인 퀘스트 목록 및 보상 수령 정보</summary>        
        public CharMainQuest[] charMainQuests;
        /// <summary>의뢰 퀘스트 목록</summary>
        public CharNormalProgress[] charNormalProgresses;
        /// <summary>다음 의뢰 퀘스트까지 남은 시간</summary>
        public long nextNormalQuestTime;
        /// <summary> 길드 퀘스트 정보 </summary>
        public CharGuildQuest[] charQuestGuilds;
        /// <summary>
        /// 진행중인 타임패트롤 퀘스트 시퀀스
        /// </summary>
        public int timePatrolSeq;
        /// <summary>
        /// 타임패트롤 퀘스트 정보 및 보상 수령 정보
        /// </summary>
        public TimePatrolQuest timePatrolQuest;
        /// <summary>
        /// 패스 일일 목록 및 보상 수령 정보
        /// </summary>
        public PassDailyQuest[] passDailyQuests;
        /// <summary>
        /// 패스 일일 퀘스트 진행 정보
        /// </summary>
        public PassDailyQuestProgress[] passDailyQuestProgresses;
        /// <summary>
        /// 패스 시즌 정보
        /// </summary>
        public PassSeasonQuest[] passSeasonQuests;
        /// <summary>
        /// 온버프패스 일일 목록 및 보상 수령 정보
        /// </summary>
        public PassDailyQuest[] onBuffPassDailyQuests;
        /// <summary>
        /// 온버프패스 일일 퀘스트 진행 정보
        /// </summary>
        public PassDailyQuestProgress[] onBuffPassDailyQuestProgresses;

        void IInitializable<Response>.Initialize(Response response)
        {
            if (response.ContainsKey("1"))
                charQuestDailies = response.GetPacketArray<CharQuestDaily>("1");

            if (response.ContainsKey("2"))
                charQuestDailyProgresses = response.GetPacketArray<CharQuestDailyProgress>("2");

            if (response.ContainsKey("3"))
                charAchieveProgresses = response.GetPacketArray<CharAchieveProgress>("3");

            if (response.ContainsKey("4"))
                charAchieveIds = response.GetIntArray("4");

            if (response.ContainsKey("eg"))
            {
                eventquestGroupInfos = response.GetPacketArray<EventQuestGroupPacket>("eg");
                System.Array.Sort(eventquestGroupInfos, (a, b) => a.sort.CompareTo(b.sort));
            }

            if (response.ContainsKey("5"))
                eventQuestInfos = response.GetPacketArray<EventQuestInfo>("5");

            if (response.ContainsKey("6"))
                mainQuestSeq = response.GetShort("6");

            if (response.ContainsKey("7"))
                charMainQuests = response.GetPacketArray<CharMainQuest>("7");

            if (response.ContainsKey("8"))
                charNormalProgresses = response.GetPacketArray<CharNormalProgress>("8");

            if (response.ContainsKey("9"))
                nextNormalQuestTime = response.GetLong("9");

            if (response.ContainsKey("10"))
                charQuestGuilds = response.GetPacketArray<CharGuildQuest>("10");

            if (response.ContainsKey("12"))
                timePatrolSeq = response.GetInt("12");

            if (response.ContainsKey("13"))
                timePatrolQuest = response.GetPacket<TimePatrolQuest>("13");

            if (response.ContainsKey("14"))
                passDailyQuests = response.GetPacketArray<PassDailyQuest>("14");

            if (response.ContainsKey("15"))
                passDailyQuestProgresses = response.GetPacketArray<PassDailyQuestProgress>("15");

            if (response.ContainsKey("16"))
                passSeasonQuests = response.GetPacketArray<PassSeasonQuest>("16");

            if (response.ContainsKey("17"))
                onBuffPassDailyQuests = response.GetPacketArray<PassDailyQuest>("17");

            if (response.ContainsKey("18"))
                onBuffPassDailyQuestProgresses = response.GetPacketArray<PassDailyQuestProgress>("18");
        }
    }
}