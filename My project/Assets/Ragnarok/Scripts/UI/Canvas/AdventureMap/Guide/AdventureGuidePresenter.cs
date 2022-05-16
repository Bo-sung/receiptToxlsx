namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIAdventureGuide"/>
    /// </summary>
    public sealed class AdventureGuidePresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        public readonly UIDuelReward.IInput[] scoreRankData;
        public readonly UIDuelReward.IInput[] killRankData;

        public AdventureGuidePresenter()
        {
            ChallengeRewardDataManager challengeRewardDataRepo = ChallengeRewardDataManager.Instance;
            BoxDataManager boxDataRepo = BoxDataManager.Instance;

            ChallengeRewardData[] scoreData = challengeRewardDataRepo.GetScoreRankData();
            ChallengeRewardData[] killData = challengeRewardDataRepo.GetKillRankData();
            scoreRankData = new AdventureGuideRankData[scoreData.Length];
            killRankData = new AdventureGuideRankData[killData.Length];

            for (int i = 0; i < scoreRankData.Length; i++)
            {
                scoreRankData[i] = new AdventureGuideRankData(scoreData[i].startRank, scoreData[i].endRank, boxDataRepo.ToBoxRewards(scoreData[i].reward));
            }

            for (int i = 0; i < killRankData.Length; i++)
            {
                killRankData[i] = new AdventureGuideRankData(killData[i].startRank, killData[i].endRank, boxDataRepo.ToBoxRewards(killData[i].reward));
            }
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        private class AdventureGuideRankData : UIDuelReward.IInput
        {
            private readonly int startRank;
            private readonly int endRank;
            private readonly RewardData[] rewards;

            public AdventureGuideRankData(int startRank, int endRank, RewardData[] rewards)
            {
                this.startRank = startRank;
                this.endRank = endRank;
                this.rewards = rewards;
            }

            public string GetTitle()
            {
                if (endRank == 0)
                    return LocalizeKey._47915.ToText(); // 참여 보상

                if (startRank == endRank)
                    return LocalizeKey._47914.ToText().Replace(ReplaceKey.RANK, startRank); // {RANK}위

                return StringBuilderPool.Get()
                    .Append(LocalizeKey._47914.ToText().Replace(ReplaceKey.RANK, startRank)) // {RANK}위
                    .Append(" ~ ")
                    .Append(LocalizeKey._47914.ToText().Replace(ReplaceKey.RANK, endRank)) // {RANK}위
                    .Release();
            }

            public RewardData[] GetRewards()
            {
                return rewards;
            }
        }
    }
}