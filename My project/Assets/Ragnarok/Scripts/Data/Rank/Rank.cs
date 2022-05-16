namespace Ragnarok
{
    public sealed class Rank : IPacket<Response>, IRank
    {
        public int uid;
        public long score;
        public short ranking;
        public string char_name;
        public int job_level;
        public byte gender;
        public byte job;
        public int cid;
        public string cidHex;
        public int battle_score;
        public int profileId;

        public void Initialize(Response response)
        {
            uid = response.GetInt("1");
            score = response.GetInt("2");
            ranking = response.GetShort("3");
            char_name = response.GetUtfString("4");
            job_level = response.GetInt("5");
            gender = response.GetByte("6");
            job = response.GetByte("7");
            cid = response.GetInt("8");
            cidHex = response.GetUtfString("9");
            battle_score = response.GetInt("10");
            profileId = response.GetInt("11");
        }

        /// <summary>
        /// 클라가 집계하는 랭킹
        /// </summary>
        public void SetRanking(short ranking)
        {
            this.ranking = ranking;
        }
    }
}