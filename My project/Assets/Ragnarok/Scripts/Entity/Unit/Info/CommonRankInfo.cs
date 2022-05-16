using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public sealed class CommonRankInfo : RankInfo
    {
        ObscuredInt uid;
        ObscuredString charName;
        ObscuredInt jobLevel;
        ObscuredByte gender;
        ObscuredByte job;
        ObscuredInt cid;
        ObscuredString cidHexCode;
        ObscuredInt battleScore;
        int profileId;

        public override int UID => uid;
        public override string CharName => charName;
        public override int JobLevel => jobLevel;
        public override Gender Gender => gender.ToEnum<Gender>();
        public override Job Job => job.ToEnum<Job>();
        public override int CID => cid;
        public override string CIDHex => cidHexCode;
        public override int BattleScore => battleScore;

        private readonly StageDataManager.IStageDataRepoImpl impl;
        private readonly ProfileDataManager.IProfileDataRepoImpl profileImpl;

        public CommonRankInfo(StageDataManager.IStageDataRepoImpl impl, ProfileDataManager.IProfileDataRepoImpl profileImpl)
        {
            this.impl = impl;
            this.profileImpl = profileImpl;
        }

        public override void SetUId(int uId)
        {
            uid = uId;
        }

        public override void SetCharName(string name)
        {
            charName = name;
        }

        public override void SetJobLevel(int level)
        {
            jobLevel = level;
        }

        public override void SetGender(Gender gender)
        {
            this.gender = gender.ToByteValue();
        }

        public override void SetJob(Job job)
        {
            this.job = job.ToByteValue();
        }

        public override void SetCId(int cid)
        {
            this.cid = cid;
        }

        public override void SetCIdHex(string name)
        {
            cidHexCode = name;
        }

        public override void SetBattleScore(int battleScore)
        {
            this.battleScore = battleScore;
        }

        public override void SetProfileId(int profileId)
        {
            this.profileId = profileId;
        }

        protected override string GetDescription()
        {
            switch (RankType)
            {
                case RankType.All:
                    return LocalizeKey._32009.ToText().Replace(ReplaceKey.LEVEL, JobLevel); // 직업 레벨 : Lv {LEVEL}

                case RankType.StageClear:
                    StageData data = impl.Get((int)Score);
                    if (data == null)
                        return string.Empty;

                    return LocalizeKey._32016.ToText().Replace(ReplaceKey.NAME, data.name_id.ToText()); // 최종 스테이지 : {STAGE_NAME}

                case RankType.BattleScore:
                    return LocalizeKey._32021.ToText().Replace(ReplaceKey.VALUE, Score.ToString()); // 전투력 : {VALUE}

                case RankType.KillMvp:
                    return LocalizeKey._32027.ToText().Replace(ReplaceKey.VALUE, Score.ToString()); // MVP 처치 : {VALUE}

                case RankType.CardUp:
                    return LocalizeKey._32028.ToText().Replace(ReplaceKey.VALUE, Score.ToString()); // 카드 강화 : {VALUE}

                case RankType.ItemMake:
                    return LocalizeKey._32029.ToText().Replace(ReplaceKey.VALUE, Score.ToString()); // 제작 : {VALUE}

                case RankType.ItemSell:
                    return LocalizeKey._32031.ToText().Replace(ReplaceKey.VALUE, Score.ToString()); // 거래소 판매 : {VALUE}

                case RankType.ItemBuy:
                    return LocalizeKey._32030.ToText().Replace(ReplaceKey.VALUE, Score.ToString()); // 거래소 구매 : {VALUE}

                case RankType.RockPaperScissors:
                    return LocalizeKey._32033.ToText().Replace(ReplaceKey.VALUE, Score.ToString()); // 7라운드 클리어 : {VALUE}

                case RankType.EventStagePoint:
                    return LocalizeKey._48219.ToText().Replace(ReplaceKey.POINT, Score.ToString()); // {POINT}점

                case RankType.EventStageKillCount:
                    return LocalizeKey._48220.ToText().Replace(ReplaceKey.COUNT, Score.ToString()); // {COUNT}회

                case RankType.DuelArena:
                    return LocalizeKey._47931.ToText().Replace(ReplaceKey.COUNT, Score.ToString()); // 아레나 깃발 : {COUNT}개
            }

            return "";
        }

        protected override string GetProfileName()
        {
            if (profileId > 0)
            {
                ProfileData profileData = profileImpl.Get(profileId);
                if (profileData != null)
                    return profileData.ProfileName;
            }

            return Job.GetJobProfile(Gender);
        }
    }
}