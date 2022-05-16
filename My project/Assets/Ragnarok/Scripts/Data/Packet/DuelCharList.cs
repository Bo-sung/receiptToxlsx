namespace Ragnarok
{
    public class DuelCharList : IPacket<Response>, UIDuelPlayerListSlot.IInput
    {
        public UIDuel.State State => UIDuel.State.Chapter;

        public string Name { get; private set; }
        public Job Job { get; private set; }
        public short JobLevel { get; private set; }
        public int BattleScore { get; private set; }
        public int WinCount { get; private set; }
        public int DefeatCount { get; private set; }
        public int DuelChpater1 { get; private set; }
        public int CID { get; private set; }
        public int UID { get; private set; }
        public Gender Gender { get; private set; }
        private int profileId;
        public string ProfileName { get; private set; }
        public int ArenaPoint { get; private set; }

        public void Initialize(Response t)
        {
            Name = t.GetUtfString("1");
            Job = t.GetByte("2").ToEnum<Job>();
            JobLevel = t.GetShort("3");
            BattleScore = t.GetInt("4");
            WinCount = t.GetInt("5");
            DefeatCount = t.GetInt("6");
            DuelChpater1 = t.GetInt("7");
            CID = t.GetInt("8");
            UID = t.GetInt("9");
            Gender = t.GetByte("10").ToEnum<Gender>();
            profileId = t.GetInt("11");
            ArenaPoint = t.GetInt("12");
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