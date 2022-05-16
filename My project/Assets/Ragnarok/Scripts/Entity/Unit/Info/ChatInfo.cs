namespace Ragnarok
{
    public class ChatInfo : IInfo, ChatModel.ISimpleChatInput
    {
        bool IInfo.IsInvalidData => false; // 무조건 유효한 데이터

        event System.Action IInfo.OnUpdateEvent { add { } remove { } } // 데이터가 변경 될 일이 음슴
        
        public readonly bool isGMMsg;

        public readonly int cid;
        public readonly string name;
        public readonly string message;
        public readonly int uid;
        public readonly string accountKey;
        public readonly int channel;
        public readonly Job job;
        public readonly Gender gender;
        private readonly int profileId;
        public readonly string thumbnailName;

        int ChatModel.ISimpleChatInput.UID => uid;
        int ChatModel.ISimpleChatInput.CID => cid;
        string ChatModel.ISimpleChatInput.Nickname => name;
        string ChatModel.ISimpleChatInput.Message => message;
        bool ChatModel.ISimpleChatInput.IsGMMsg => isGMMsg;

        public ChatInfo(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl, int cid, string name, string message, int uid, Job job, Gender gender, int profileId, string accountKey, int channel)
        {
            isGMMsg = false;

            this.cid = cid;
            this.name = name;
            this.message = message;
            this.uid = uid;
            this.job = job;
            this.gender = gender;
            this.accountKey = accountKey;
            this.channel = channel;
            this.profileId = profileId;
            thumbnailName = GetThumbnailName(profileDataRepoImpl);
        }

        public ChatInfo(int cid, string name, string message, int uid)
        {
            isGMMsg = false;

            this.cid = cid;
            this.name = name;
            this.message = message;
            this.uid = uid;
        }

        public ChatInfo(string message)
        {
            isGMMsg = true;

            this.message = message;
        }

        /// <summary>
        /// 프로필 (원형)
        /// </summary>
        private string GetThumbnailName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            if (profileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(profileId);

                if (profileData != null)
                    return profileData.ThumbnailName;
            }

            return job.GetThumbnailName(gender);
        }
    }
}