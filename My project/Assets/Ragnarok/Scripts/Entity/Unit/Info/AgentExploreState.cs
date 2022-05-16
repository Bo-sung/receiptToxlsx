using System.Collections.Generic;

namespace Ragnarok
{
    public class AgentExploreState
    {
        private static int NextGUID = 0;

        public int GUID { get; private set; }
        public int StageID { get; private set; }
        public ExploreType Type { get; private set; }
        public RemainTime RemainTime { get; private set; }
        private readonly List<ExploreAgent> participantList;

        public IEnumerable<ExploreAgent> Participants { get { return participantList; } }
        public int ParticipantCount { get { return participantList.Count; } }

        public AgentExploreState(int stageID, ExploreType exploreType, long remainTime)
        {
            GUID = NextGUID++;
            StageID = stageID;
            Type = exploreType;
            RemainTime = remainTime;
            participantList = new List<ExploreAgent>();
        }

        public void AddParticipant(ExploreAgent exploreAgent)
        {
            participantList.Add(exploreAgent);
        }

        public void UpdateRemainTime(float remainTime)
        {
            RemainTime = remainTime;
        }

        public override string ToString()
        {
            List<string> str = new List<string>();
            for (int i = 0; i < participantList.Count; ++i)
                str.Add(participantList[i].ID.ToString());

            return $"ExploreState {{ GUID : {GUID}, StageID : {StageID}, ExploreType : {Type}, RemainTime : {RemainTime.ToStringTime()}, Participants : {{ { string.Join(", ", str) } }} }}";
        }
    }
}
