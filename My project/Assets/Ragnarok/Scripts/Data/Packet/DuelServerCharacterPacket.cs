namespace Ragnarok
{
    public class DuelServerCharacterPacket : IPacket<Response>, UIDuelPlayerListSlot.IInput
    {
        public UIDuel.State State => UIDuel.State.Event;

        public int CID { get; private set; }
        public string Name { get; private set; }
        public Job Job { get; private set; }
        public Gender Gender { get; private set; }
        public short JobLevel { get; private set; }
        public int BattleScore { get; private set; }
        public string useSkillIds;
        public int UID { get; private set; }
        private int profileId;
        public string ProfileName { get; private set; }
        public int ArenaPoint => 0;

        public int WinCount => throw new System.NotImplementedException();
        public int DefeatCount => throw new System.NotImplementedException();

        void IInitializable<Response>.Initialize(Response response)
        {
            CID = response.GetInt("1");
            Name = response.GetUtfString("2");
            Job = response.GetByte("3").ToEnum<Job>();
            Gender = response.GetByte("4").ToEnum<Gender>();
            JobLevel = response.GetShort("5");
            BattleScore = response.GetInt("6");
            useSkillIds = response.GetUtfString("7");
            UID = response.GetInt("8");
            profileId = response.GetInt("9");
        }

        public void Initialize(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            ProfileName = GetProfileName(profileDataRepoImpl);
        }

        private string GetProfileName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            if (profileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(profileId);
                if (profileData != null)
                    return profileData.ProfileName;
            }

            return Job.GetJobProfile(Gender);
        }
    }
}