namespace Ragnarok
{
    public class ServerInfoPacket : IPacket<Response>
    {
        public int Cid { get; private set; }
        public string Name { get; private set; }
        public byte Job { get; private set; }
        public int JobLevel { get; private set; }
        public byte Gender { get; private set; }
        public int ServerGroupId { get; private set; }
        public int ServerNameKey { get; private set; }
        public bool ServerExist { get; private set; }
        private int profileId;

        public ServerComplex Complex { get; private set; }
        public ServerState State { get; private set; }
        public bool AvailableCreate { get; private set; }
        public string ProfileName { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            Cid = response.GetInt("1");
            Name = response.GetUtfString("2");
            Job = response.GetByte("3");
            JobLevel = response.GetShort("4");
            Gender = response.GetByte("5");

            if (response.ContainsKey("6")) ServerGroupId = response.GetByte("6");
            else ServerGroupId = -1;

            ServerNameKey = BasisType.SERVER_NAME_ID.GetInt(ServerGroupId);

            ServerExist = response.ContainsKey("7");
            if (ServerExist)
            {
                Complex = (ServerComplex)response.GetByte("7");
                AvailableCreate = response.GetByte("8") == 1;
                State = (ServerState)response.GetByte("9");

                // 서버가 매우혼잡 상태면, 생성할 수 없음.
                if (Complex == ServerComplex.VERY_CROWDED)
                {
                    AvailableCreate = false;
                }
            }

            profileId = response.GetInt("10");
        }

        public void Initialize(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            ProfileName = GetProfileName(profileDataRepoImpl);
        }

        /// <summary>
        /// 현재 접속중인 캐릭터의 정보는 변경될 수 있기때문에 갱신해줌..
        /// </summary>
        public void UpdateCharacterInfo(string name, byte job, int jobLevel, byte gender)
        {
            Name = name;
            Job = job;
            JobLevel = jobLevel;
            Gender = gender;
        }

        public bool HasCharacter()
        {
            return Cid >= 0;
        }

        private string GetProfileName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            if (profileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(profileId);
                if (profileData != null)
                    return profileData.ProfileName;
            }

            return Job.ToEnum<Job>().GetJobProfile(Gender.ToEnum<Gender>());
        }
    }
}