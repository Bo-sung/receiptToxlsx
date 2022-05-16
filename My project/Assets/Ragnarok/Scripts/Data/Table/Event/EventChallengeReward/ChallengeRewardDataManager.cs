using MsgPack;

namespace Ragnarok
{
    public sealed class ChallengeRewardDataManager : Singleton<ChallengeRewardDataManager>, IDataManger
    {
        private readonly BetterList<ChallengeRewardData> scoreDataList;
        private readonly BetterList<ChallengeRewardData> killDataList;

        public ResourceType DataType => ResourceType.ChallengeRewardDataDB;

        public ChallengeRewardDataManager()
        {
            scoreDataList = new BetterList<ChallengeRewardData>();
            killDataList = new BetterList<ChallengeRewardData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            scoreDataList.Release();
            killDataList.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ChallengeRewardData data = new ChallengeRewardData(mpo.AsList());

                    switch (data.groupId)
                    {
                        case ChallengeRewardData.SCORE_RANK_GROUP:
                            scoreDataList.Add(data);
                            break;

                        case ChallengeRewardData.KILL_RANK_GROUP:
                            killDataList.Add(data);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 점수 랭킹 데이터 반환
        /// </summary>
        public ChallengeRewardData[] GetScoreRankData()
        {
            return scoreDataList.ToArray();
        }

        /// <summary>
        /// 처치 랭킹 데이터 반환
        /// </summary>
        public ChallengeRewardData[] GetKillRankData()
        {
            return killDataList.ToArray();
        }

        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
#endif
        }
    }
}