using Ragnarok.View;
using System;

namespace Ragnarok
{
    public class CharDuelHistory : IPacket<Response>, UIDuelArenaHistoryElement.IInput
    {
        public int CID { get; private set; }
        public int TargetCID { get; private set; }
        public int TargetUID { get; private set; }
        public string TargetName { get; private set; }
        public Job TargetJob { get; private set; }
        public byte Result { get; private set; }
        public int Chapter { get; private set; }
        public int DuelPieceNameID { get; private set; }
        public int BattleScore { get; private set; }
        public int WinCount { get; private set; }
        public int DefeatCount { get; private set; }
        public DateTime InsertDate { get; private set; }
        public Gender TargetGender { get; private set; }
        public int JobLevel { get; private set; }
        private int profileId;
        
        public string ProfileName { get; private set; }

        public void Initialize(Response t)
        {
            CID = t.GetInt("1");
            TargetCID = t.GetInt("2");
            TargetUID = t.GetInt("3");
            TargetName = t.GetUtfString("4");
            TargetJob = t.GetByte("5").ToEnum<Job>();
            Result = t.GetByte("6");
            Chapter = t.GetInt("7");
            DuelPieceNameID = t.GetInt("8");
            BattleScore = t.GetInt("9");
            WinCount = t.GetInt("10");
            DefeatCount = t.GetInt("11");
            InsertDate = t.GetLong("12").ToDateTime();
            TargetGender = t.GetByte("13").ToEnum<Gender>();
            JobLevel = t.GetShort("14");
            profileId = t.GetInt("15");
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

            return TargetJob.GetJobProfile(TargetGender);
        }
    }
}