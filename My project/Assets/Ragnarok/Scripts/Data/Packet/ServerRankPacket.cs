namespace Ragnarok
{
    public class ServerRankPacket : IPacket<Response>
    {
        public int cid;
        public string name;
        public Job job;
        public Gender gender;
        public int jobLevel;
        public int battleScore;
        public int rankValue;
        public int serverGroupId;

        public int rank;
        public string hexCid;
        public int profileId;

        // 클라이언트 전용
        private string profileName;

        void IInitializable<Response>.Initialize(Response response)
        {
            cid = response.GetInt("1");
            name = response.GetUtfString("2");
            job = response.GetByte("3").ToEnum<Job>();
            gender = response.GetByte("4").ToEnum<Gender>();
            jobLevel = response.GetShort("5");
            battleScore = response.GetInt("6");
            rankValue = response.GetInt("7");
            hexCid = MathUtils.CidToHexCode(cid);

            short serverGroupId = response.GetShort("8");
            SetServerGroupId(serverGroupId);

            profileId = response.GetInt("9");
        }

        public void Initialize(int cid, string hexCid, string name, Job job, Gender gender, int jobLevel, int battleScore, int profileId)
        {
            this.cid = cid;
            this.hexCid = hexCid;
            this.name = name;
            this.job = job;
            this.gender = gender;
            this.jobLevel = jobLevel;
            this.battleScore = battleScore;
            this.profileId = profileId;
        }

        public void Initialize(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            profileName = GetProfileName(profileDataRepoImpl);
        }

        public void ResetData()
        {
            rank = 0;
            rankValue = 0;
        }

        public void SetRank(int rank)
        {
            this.rank = rank;
        }

        public void SetRankValue(int rankValue)
        {
            this.rankValue = rankValue;
        }

        public virtual void SetServerGroupId(int serverGroupId)
        {
            this.serverGroupId = serverGroupId;
        }

        public Job GetJob()
        {
            return job;
        }

        public Gender GetGender()
        {
            return gender;
        }

        public string GetNameText()
        {
            const string NAME_FORMAT = "[5A575B]{NAME}[-] [BEBEBE]({VALUE})[-]";
            return NAME_FORMAT
                .Replace(ReplaceKey.NAME, name)
                .Replace(ReplaceKey.VALUE, hexCid);
        }

        public string GetServerName()
        {
            return BasisType.SERVER_NAME_ID.GetInt(serverGroupId).ToText();
        }

        public string GetJobLevelText()
        {
            return LocalizeKey._47925.ToText() // JOB Lv.{LEVEL}
                .Replace(ReplaceKey.LEVEL, jobLevel);
        }

        public int GetBattleScore()
        {
            return battleScore;
        }

        public virtual string GetRankValueText()
        {
            return LocalizeKey._47923.ToText() // {COUNT}승
                .Replace(ReplaceKey.COUNT, rankValue);
        }

        public int GetRank()
        {
            return rank;
        }

        public int GetServerGroupId()
        {
            return serverGroupId;
        }

        public string GetProfileName()
        {
            return profileName;
        }

        private string GetProfileName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            if (profileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(profileId);
                if (profileData != null)
                    return profileData.ProfileName;
            }

            return job.GetJobProfile(gender);
        }
    }
}