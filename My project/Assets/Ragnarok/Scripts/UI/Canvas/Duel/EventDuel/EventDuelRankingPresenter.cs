namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIEventDuelRanking"/>
    /// </summary>
    public class EventDuelRankingPresenter : ViewPresenter
    {
        public const int WORLD_SERVER_RANK_REQUEST_FLAG = 1;
        public const int MY_SERVER_RANK_REQUEST_FLAG = 2;

        // <!-- Models --!>
        private readonly CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly ProfileDataManager profileDataRepo;
        public readonly int myServerGroupId;

        // <!-- Event --!>
        public event System.Action OnUpateData;

        // <!-- Temp --!>
        private readonly EventDuelRankPacket worldServerMyRank;
        private readonly BetterList<EventDuelRankPacket> worldServerRankList;
        private readonly EventDuelRankPacket myServerMyRank;
        private readonly BetterList<EventDuelRankPacket> myServerRankList;

        private bool isSuccessRequestWorldServerRank;
        private bool isSuccessRequestServerRank;

        private int serverFlag;

        public EventDuelRankingPresenter()
        {
            characterModel = Entity.player.Character;

            profileDataRepo = ProfileDataManager.Instance;
            myServerGroupId = ConnectionManager.Instance.GetSelectServerGroupId();

            worldServerMyRank = new EventDuelRankPacket();
            worldServerRankList = new BetterList<EventDuelRankPacket>();
            myServerMyRank = new EventDuelRankPacket();
            myServerRankList = new BetterList<EventDuelRankPacket>();
        }

        public override void AddEvent()
        {
            Protocol.REQUEST_DUELWORLD_RANKING.AddEvent(OnRequestDuelWorldRanking);
        }

        public override void RemoveEvent()
        {
            Protocol.REQUEST_DUELWORLD_RANKING.RemoveEvent(OnRequestDuelWorldRanking);
        }

        public void Initialize()
        {
            int cid = characterModel.Cid;
            string hexCid = characterModel.CidHex;
            string name = characterModel.Name;
            Job job = characterModel.Job;
            Gender gender = characterModel.Gender;
            int jobLevel = characterModel.JobLevel;
            int battleScore = Entity.player.GetTotalAttackPower();
            int profileId = characterModel.ProfileId;

            worldServerMyRank.Initialize(cid, hexCid, name, job, gender, jobLevel, battleScore, profileId);
            worldServerMyRank.Initialize(profileDataRepo);
            worldServerMyRank.SetServerGroupId(myServerGroupId);

            myServerMyRank.Initialize(cid, hexCid, name, job, gender, jobLevel, battleScore, profileId);
            myServerMyRank.Initialize(profileDataRepo);
            myServerMyRank.SetServerGroupId(myServerGroupId);
        }

        public void ResetData()
        {
            worldServerMyRank.ResetData();
            worldServerRankList.Release();
            myServerMyRank.ResetData();
            myServerRankList.Release();

            isSuccessRequestWorldServerRank = false;
            isSuccessRequestServerRank = false;
        }

        public void SetServerFlag(int serverFlag)
        {
            this.serverFlag = serverFlag;
        }

        public void RequestDuelRank()
        {
            // 이미 호출한 정보
            if (serverFlag == WORLD_SERVER_RANK_REQUEST_FLAG && isSuccessRequestWorldServerRank)
                return;

            // 이미 호출한 정보
            if (serverFlag == MY_SERVER_RANK_REQUEST_FLAG && isSuccessRequestServerRank)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", serverFlag);
            Protocol.REQUEST_DUELWORLD_RANKING.SendAsync(sfs).WrapNetworkErrors();
        }

        public EventDuelRankPacket GetMyRank()
        {
            switch (serverFlag)
            {
                case WORLD_SERVER_RANK_REQUEST_FLAG:
                    return worldServerMyRank;

                case MY_SERVER_RANK_REQUEST_FLAG:
                    return myServerMyRank;
            }

            return null;
        }

        public EventDuelRankPacket[] GetRanks()
        {
            switch (serverFlag)
            {
                case WORLD_SERVER_RANK_REQUEST_FLAG:
                    return worldServerRankList.ToArray();

                case MY_SERVER_RANK_REQUEST_FLAG:
                    return myServerRankList.ToArray();
            }

            return null;
        }

        void OnRequestDuelWorldRanking(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            bool isWorldServerRank = serverFlag == WORLD_SERVER_RANK_REQUEST_FLAG;
            EventDuelRankPacket[] packets = response.GetPacketArray<EventDuelRankPacket>("1");
            int savedWinCount = int.MaxValue;
            int rank = -1;
            for (int i = 0; i < packets.Length; i++)
            {
                packets[i].Initialize(profileDataRepo);

                if (packets[i].rankValue < savedWinCount)
                {
                    savedWinCount = packets[i].rankValue;
                    rank = i + 1;
                }

                packets[i].SetRank(rank);

                if (isWorldServerRank)
                {
                    worldServerRankList.Add(packets[i]);
                }
                else
                {
                    packets[i].SetServerGroupId(myServerGroupId); // 내 서버 정보의 경우에는 내 서버 GroupId 로 세팅
                    myServerRankList.Add(packets[i]);
                }
            }

            int myRank = response.GetInt("2");
            int myWinCount = response.GetInt("3");
            if (isWorldServerRank)
            {
                isSuccessRequestWorldServerRank = true;
                worldServerMyRank.SetRank(myRank);
                worldServerMyRank.SetRankValue(myWinCount);
            }
            else
            {
                isSuccessRequestServerRank = true;
                myServerMyRank.SetRank(myRank);
                myServerMyRank.SetRankValue(myWinCount);
            }

            OnUpateData?.Invoke();
        }
    }
}