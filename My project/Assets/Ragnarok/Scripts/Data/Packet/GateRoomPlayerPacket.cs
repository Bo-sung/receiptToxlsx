namespace Ragnarok
{
    public class GateRoomPlayerPacket : IPacket<Response>
    {
        public int uid;
        public int cid;
        public string name;
        public Job job;
        public Gender gender;
        public int battleScore;
        public int profileId;
        public int jobLevel;
        public int[] skills;

        void IInitializable<Response>.Initialize(Response response)
        {
            name = response.GetUtfString("1");
            job = response.GetByte("2").ToEnum<Job>();
            gender = response.GetByte("3").ToEnum<Gender>();
            battleScore = response.GetInt("4");
            cid = response.GetInt("5");
            jobLevel = response.GetShort("6");
            uid = response.GetInt("7");
            profileId = response.GetInt("8");
            string skillIds = response.GetUtfString("9");
            string[] results = StringUtils.Split(skillIds, StringUtils.SplitType.Comma);
            skills = new int[results.Length];
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i] = StringUtils.IsDigit(results[i]) ? int.Parse(results[i]) : 0;
            }
        }
    }
}