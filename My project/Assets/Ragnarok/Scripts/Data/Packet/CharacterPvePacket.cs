namespace Ragnarok
{
    public class CharacterPvePacket : IPacket<Response>
    {
        /// <summary>
        /// 현 시즌 No
        /// </summary>
        public int pve_season_no;

        /// <summary>
        /// 현 시즌 점수
        /// </summary>
        public int pve_season_score;

        /// <summary>
        /// 현 시즌 티어 등급
        /// </summary>
        public int pve_tier;

        /// <summary>
        /// 현 시즌 승리 횟수
        /// </summary>
        public int pve_season_win_count;

        /// <summary>
        /// 현 시즌 패배 횟수
        /// </summary>
        public int pve_season_lose_count;

        /// <summary>
        /// 보상 받을 시즌 티어 등급
        /// </summary>
        public int reward_tier;

        /// <summary>
        /// 보상 받을 시즌 랭킹 (0일 때에는 순위 밖)
        /// </summary>
        public int reward_rank;

        /// <summary>
        /// 보상 수려 여부 플래그
        /// </summary>
        public bool isReward;

        /// <summary>
        /// 이전 시즌 티어 등급
        /// </summary>
        public int old_tier;

        /// <summary>
        /// 이전 시즌 랭킹 (0일 때에는 순위 밖)
        /// </summary>
        public int old_rank;

        /// <summary>
        /// 싱글 대전 점수
        /// </summary>
        public int single_score;

        /// <summary>
        /// 싱글 대전 랭크
        /// </summary>
        public int single_rank;

        void IInitializable<Response>.Initialize(Response response)
        {
            const int REWARD_STANDBY = 0; // 보상 대기 중

            pve_season_no = response.GetInt("1");
            pve_season_score = response.GetInt("2");
            pve_tier = response.GetByte("3");
            pve_season_win_count = response.GetInt("4");
            pve_season_lose_count = response.GetInt("5");

            reward_tier = response.GetByte("6");
            reward_rank = response.GetInt("7");
            isReward = response.GetInt("8") == REWARD_STANDBY;
            old_tier = response.GetByte("9");
            old_rank = response.GetInt("10");
            single_score = response.GetInt("11");
            single_rank = response.GetInt("14");
        }
    }
}