namespace Ragnarok
{
    public class MultiMatchChatInfo : ChatInfo
    {
        private int stage;

        public MultiMatchChatInfo(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl, int cid, string name, string message, int uid, Job job, Gender gender, int profileId, string accountKey, int channel)
            : base(profileDataRepoImpl, cid, name, message, uid, job, gender, profileId, accountKey, channel)
        {
        }

        public void SetStage(int stage)
        {
            this.stage = stage;
        }

        public int GetStage()
        {
            return stage;
        }
    }
}