using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class RankModel : CharacterEntityModel, IEqualityComparer<RankType>
    {
        private readonly Dictionary<RankType, List<RankInfo>> rankInfos;
        private readonly Dictionary<RankType, RankInfo> myRankInfo;
        private readonly Dictionary<RankType, int?> nextPage;
        private const int MAX_PAGE = 20;

        public event System.Action<(RankType rankType, int page)> OnUpdateRankList;

        private readonly StageDataManager stageDataRepo;
        private readonly ProfileDataManager profileDataRepo;

        public RankModel()
        {
            rankInfos = new Dictionary<RankType, List<RankInfo>>(this);
            myRankInfo = new Dictionary<RankType, RankInfo>(this);
            nextPage = new Dictionary<RankType, int?>(this);
            stageDataRepo = StageDataManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;
            foreach (RankType item in Enum.GetValues(typeof(RankType)))
            {
                rankInfos.Add(item, new List<RankInfo>());
                myRankInfo.Add(item, null);
                nextPage.Add(item, null);
            }
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        public bool HasNextPage(RankType rankType)
        {
            return nextPage[rankType].HasValue;
        }

        public RankInfo[] GetRankInfos(RankType rankType)
        {
            return rankInfos[rankType].OrderBy(x => x.Rank).ToArray();
        }

        public RankInfo GetMyRankInfo(RankType rankType)
        {
            return myRankInfo[rankType];
        }

        public void ClearAllRankInfos()
        {
            foreach (RankType type in Enum.GetValues(typeof(RankType)))
            {
                rankInfos[type].Clear();
                myRankInfo[type] = null;
                nextPage[type] = null;
            }
        }

        public void ClearRankInfo(RankType type)
        {
            rankInfos[type].Clear();
            myRankInfo[type] = null;
            nextPage[type] = null;
        }

        /// <summary>
        /// 다음페이지 직업랭킹 요청
        /// </summary>
        public async Task RequestNextRankList(RankType rankType, bool myRank = true)
        {
            // 다음페이지가 없는 경우
            if (!nextPage[rankType].HasValue)
                return;

            await RequestRankList(nextPage[rankType].Value, rankType, myRank);
        }

        public async Task RequestRankList(int page, RankType rankType, bool myRank = true)
        {
            var sfs = Protocol.NewInstance();

            if (rankType == RankType.MyGuildBattle)
            {
                // Do Nothing
            }
            else
            {
                sfs.PutInt("1", page);

                if (rankType == RankType.GuildBattle || rankType == RankType.EventGuildBattle)
                {
                    // Do Nothing
                }
                else
                {
                    sfs.PutByte("2", rankType.ToByteValue());
                    sfs.PutBool("3", myRank);
                }
            }

            Response response;
            if (rankType == RankType.MyGuildBattle)
            {
                response = await Protocol.REQUEST_GUILD_BATTLE_GUILD_CHAR_RANK.SendAsync(sfs);
            }
            else if (rankType == RankType.GuildBattle)
            {
                response = await Protocol.REQUEST_GUILD_BATTLE_RANK.SendAsync(sfs);
            }
            else if (rankType == RankType.EventGuildBattle)
            {
                response = await Protocol.REQUEST_GUILD_BATTLE_EVENT_RANK.SendAsync(sfs);
            }
            else
            {
                response = await Protocol.REQUEST_RANK_LIST.SendAsync(sfs);
            }

            if (response.isSuccess)
            {
                RankPacket packet = new RankPacket();
                packet.Initialize(response, rankType);

                if (rankType == RankType.Guild || rankType == RankType.GuildBattle || rankType == RankType.EventGuildBattle)
                {
                    var guildModel = Entity.Guild;

                    if (rankType == RankType.GuildBattle || rankType == RankType.EventGuildBattle)
                    {
                        if (guildModel)
                        {
                            guildModel.UpdateGuildInfo(packet.masterName, packet.memberCount, packet.maxMemberCount, packet.myGuildExp);
                        }
                    }

                    foreach (var item in packet.ranks)
                    {
                        GuildRank data = item as GuildRank;

                        // 길드 랭킹의 경우 Score 가 Exp
                        int expPoint = rankType == RankType.Guild ? (int)data.guildScore : data.guildExp;

                        GuildRankInfo info = new GuildRankInfo();
                        info.SetRankType(rankType);
                        info.SetRank(data.guildRank);
                        info.SetScore(data.guildScore);
                        info.SetGuildId(data.guildId);
                        info.SetMaxMemberCount(data.maxMemberCount);
                        info.SetCurMemberCount(data.curMemberCount);
                        info.SetEmblem(data.emblem);
                        info.SetGuildName(data.guildName);
                        info.SetGuildMasterName(data.guildMasterName);
                        info.SetGuildLevel(guildModel.GetGuildLevel(expPoint));
                        rankInfos[rankType].Add(info);
                    }

                    if (guildModel.HaveGuild)
                    {
                        myRankInfo[rankType] = new GuildRankInfo();
                        myRankInfo[rankType].SetRankType(rankType);
                        myRankInfo[rankType].SetRank(packet.myRank);
                        myRankInfo[rankType].SetScore(packet.myScore);
                        myRankInfo[rankType].SetGuildId(guildModel.GuildId);
                        myRankInfo[rankType].SetMaxMemberCount(guildModel.MaxMemberCount);
                        myRankInfo[rankType].SetCurMemberCount(guildModel.MemberCount);
                        myRankInfo[rankType].SetEmblem(guildModel.EmblemId);
                        myRankInfo[rankType].SetGuildName(guildModel.GuildName);
                        myRankInfo[rankType].SetGuildMasterName(guildModel.MasterName);
                        myRankInfo[rankType].SetGuildLevel(guildModel.GuildLevel);
                    }
                }
                else
                {
                    short ranking = 0;
                    short sameRankingCount = 1;
                    long score = long.MaxValue;
                    foreach (var item in packet.ranks)
                    {
                        Rank data = item as Rank;

                        // 랭크 직접 집계
                        if (rankType == RankType.MyGuildBattle)
                        {
                            if (score > data.score)
                            {
                                score = data.score;

                                ranking += sameRankingCount; // 랭킹 세팅
                                sameRankingCount = 1; // 랭킹카운트 초기화
                            }
                            else if (data.score == score)
                            {
                                ++sameRankingCount;
                            }
                            else
                            {
#if UNITY_EDITOR
                                Debug.LogError("Sort 필요");
#endif
                            }

                            item.SetRanking(ranking);
                        }

                        CommonRankInfo info = new CommonRankInfo(stageDataRepo, profileDataRepo);
                        info.SetRankType(rankType);
                        info.SetRank(data.ranking);
                        info.SetScore(data.score);
                        info.SetUId(data.uid);
                        info.SetCharName(data.char_name);
                        info.SetJobLevel(data.job_level);
                        info.SetGender(data.gender.ToEnum<Gender>());
                        info.SetJob(data.job.ToEnum<Job>());
                        info.SetCId(data.cid);
                        info.SetCIdHex(data.cidHex);
                        info.SetBattleScore(data.battle_score);
                        info.SetProfileId(data.profileId);
                        rankInfos[rankType].Add(info);
                    }

                    // 내 랭킹
                    var charModel = Entity.Character;
                    var userModel = Entity.User;
                    myRankInfo[rankType] = new CommonRankInfo(stageDataRepo, profileDataRepo);
                    myRankInfo[rankType].SetRankType(rankType);
                    myRankInfo[rankType].SetRank(packet.myRank);
                    myRankInfo[rankType].SetScore(packet.myScore);
                    myRankInfo[rankType].SetBattleScore(Entity.GetTotalAttackPower());
                    myRankInfo[rankType].SetUId(userModel.UID);
                    myRankInfo[rankType].SetCharName(charModel.Name);
                    myRankInfo[rankType].SetJobLevel(charModel.JobLevel);
                    myRankInfo[rankType].SetGender(charModel.Gender);
                    myRankInfo[rankType].SetJob(charModel.Job);
                    myRankInfo[rankType].SetCId(charModel.Cid);
                    myRankInfo[rankType].SetCIdHex(charModel.CidHex);
                    myRankInfo[rankType].SetProfileId(charModel.ProfileId);
                }

                OnUpdateRankList?.Invoke((rankType, page));

                // 다음 페이지 없음
                if (packet.ranks.Length == 0)
                {
                    nextPage[rankType] = null;
                    return;
                }

                // 다음 페이지 없음 최대 페이지
                if (page == MAX_PAGE)
                {
                    nextPage[rankType] = null;
                    return;
                }

                nextPage[rankType] = page + 1;
            }
            else
            {
                response.ShowResultCode();
            }
        }

        public async Task RequestNextMazeRankList(int mazeMapId)
        {
            RankType rankType = RankType.MazeClear;

            // 다음페이지가 없는 경우
            if (!nextPage[rankType].HasValue)
                return;

            await RequestMazeRankList(nextPage[rankType].Value, mazeMapId);
        }

        public async Task RequestMazeRankList(int page, int mazeMapId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", page);
            sfs.PutInt("2", mazeMapId);
            RankType rankType = RankType.MazeClear;
            ClearRankInfo(rankType);

            var response = await Protocol.REQUEST_MAZE_RANK_LIST.SendAsync(sfs);
            if (response.isSuccess)
            {
                RankPacket packet = new RankPacket();
                packet.Initialize(response, rankType);

                foreach (var item in packet.ranks)
                {
                    Rank data = item as Rank;

                    CommonRankInfo info = new CommonRankInfo(stageDataRepo, profileDataRepo);
                    info.SetRankType(rankType);
                    info.SetRank(data.ranking);
                    info.SetScore(data.score);
                    info.SetUId(data.uid);
                    info.SetCharName(data.char_name);
                    info.SetJobLevel(data.job_level);
                    info.SetGender(data.gender.ToEnum<Gender>());
                    info.SetJob(data.job.ToEnum<Job>());
                    info.SetCId(data.cid);
                    info.SetCIdHex(data.cidHex);
                    info.SetBattleScore(data.battle_score);
                    info.SetProfileId(data.profileId);
                    rankInfos[rankType].Add(info);
                }

                // 내 랭킹
                var charModel = Entity.Character;
                var userModel = Entity.User;
                myRankInfo[rankType] = new CommonRankInfo(stageDataRepo, profileDataRepo);
                myRankInfo[rankType].SetRankType(rankType);
                myRankInfo[rankType].SetRank(packet.myRank);
                myRankInfo[rankType].SetScore(packet.myScore);
                myRankInfo[rankType].SetBattleScore((int)packet.myScore.Value);
                myRankInfo[rankType].SetUId(userModel.UID);
                myRankInfo[rankType].SetCharName(charModel.Name);
                myRankInfo[rankType].SetJobLevel(charModel.JobLevel);
                myRankInfo[rankType].SetGender(charModel.Gender);
                myRankInfo[rankType].SetJob(charModel.Job);
                myRankInfo[rankType].SetCId(charModel.Cid);
                myRankInfo[rankType].SetCIdHex(charModel.CidHex);
                myRankInfo[rankType].SetProfileId(charModel.ProfileId);

                // 다음 페이지 없음
                if (packet.ranks.Length == 0)
                {
                    nextPage[rankType] = null;
                    return;
                }

                // 다음 페이지 없음 최대 페이지
                if (page == MAX_PAGE)
                {
                    nextPage[rankType] = null;
                    return;
                }

                nextPage[rankType] = page + 1;
            }
            else
            {
                response.ShowResultCode();
            }
        }

        bool IEqualityComparer<RankType>.Equals(RankType x, RankType y)
        {
            return x == y;
        }

        int IEqualityComparer<RankType>.GetHashCode(RankType obj)
        {
            return obj.GetHashCode();
        }
    }
}