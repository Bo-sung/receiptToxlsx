using Ragnarok.View;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildBattleEnter"/>
    /// </summary>
    public sealed class GuildBattleEnterPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly PlayerEntity player;
        private readonly GoodsModel goodsModel;
        private readonly GuildModel guildModel;
        private readonly CharacterModel characterModel;
        private readonly CupetListModel cupetListModel;
        private readonly UserModel userModel;
        private readonly RankModel rankModel;

        // <!-- Repositories --!>
        private readonly ExpDataManager expDataRepo;
        public readonly int requestGuildListDelay;
        public readonly int dailyEntryCount;
        private readonly int maxEmperiumHp;

        // <!-- Data --!>
        private readonly GuildBattlePositionInfo guildBattlePositionInfo;
        private readonly MyGuildBattleInfo myGuildBattleInfo;
        private readonly TargetGuildBattleInfo targetGuildBattleInfo;
        private readonly Buffer<CupetModel> cupetBuffer;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Event --!>
        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public event System.Action OnRequestGuildList
        {
            add { guildModel.OnUpdateGuildBattleList += value; }
            remove { guildModel.OnUpdateGuildBattleList -= value; }
        }

        public event System.Action OnRequestGuildBattleAttackPositionInfo;
        public event System.Action OnRequestGuildBattleCupetSettings;
        public event System.Action OnRequestGuildBattleDefenseInfo;
        public event System.Action OnRequestGuildBattleListDetailInfo;
        public event System.Action OnSelectAgentInfo;
        public event System.Action OnRequestMyGuildDefenseInfo;
        public event System.Action OnRequestMyGuildRankInfo;

        private CupetModel[] myLeftCupets, myRightCupets;
        private bool isRequestMyGuildRankInfo;

        public GuildBattleEnterPresenter()
        {
            player = Entity.player;
            goodsModel = player.Goods;
            guildModel = player.Guild;
            characterModel = player.Character;
            cupetListModel = player.CupetList;
            userModel = player.User;
            rankModel = player.RankModel;

            expDataRepo = ExpDataManager.Instance;
            requestGuildListDelay = (int)MathUtils.ToPermilleValue(BasisType.DUEL_LIST_CHANGE_TIME.GetInt());
            dailyEntryCount = BasisGuildWarInfo.DailyEntryCount.GetInt();

            maxEmperiumHp = guildModel.GetMaxEmperiumHp();
            guildBattlePositionInfo = new GuildBattlePositionInfo(cupetListModel, SkillDataManager.Instance, BasisGuildWarInfo.BuffNeedLevelUpExp.GetInt());
            myGuildBattleInfo = new MyGuildBattleInfo(guildModel.GuildId, guildModel.GuildName, guildModel.EmblemId, maxEmperiumHp);
            targetGuildBattleInfo = new TargetGuildBattleInfo(maxEmperiumHp);
            cupetBuffer = new Buffer<CupetModel>();

            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            Protocol.REQUEST_GUILD_BATTLE_START_CHECK.AddEvent(OnGuildBattleStartCheck);
            rankModel.OnUpdateRankList += OnUpdateRankList;
        }

        public override void RemoveEvent()
        {
            Protocol.REQUEST_GUILD_BATTLE_START_CHECK.RemoveEvent(OnGuildBattleStartCheck);
            rankModel.OnUpdateRankList -= OnUpdateRankList;

            rankModel.ClearRankInfo(RankType.MyGuildBattle);
        }

        /// <summary>
        /// 길드전 시작 이벤트
        /// </summary>
        void OnGuildBattleStartCheck(Response response)
        {
            if (response.isSuccess)
                return;

            // 상대 길드의 엠펠리움 파괴 시 길드 리스트 새로 요청
            if (response.resultCode == ResultCode.EMPAL_HP_ZERO)
            {
                RequestGuildList();
            }
        }

        /// <summary>
        /// 랭킹 정보 이벤트
        /// </summary>
        void OnUpdateRankList((RankType rankType, int page) info)
        {
            if (info.rankType != RankType.MyGuildBattle)
                return;

            OnRequestMyGuildRankInfo?.Invoke();
        }

        public string GetProfileName()
        {
            return characterModel.GetProfileName();
        }

        public string GetJobIconName()
        {
            return characterModel.Job.GetJobIcon();
        }

        public int GetJobLevel()
        {
            return characterModel.JobLevel;
        }

        public string GetCharacterName()
        {
            return characterModel.Name;
        }

        public int GetBattleScore()
        {
            return player.GetTotalAttackPower();
        }

        public long GetTotalDamage()
        {
            return guildModel.GuildBattleAccrueDamage;
        }

        public int GetRemainCount()
        {
            return guildModel.GuildBattleEnterRemainCount;
        }

        /// <summary>
        /// 길드전 목록
        /// </summary>
        public UIGuildBattleElement.IInput[] GetGuildList()
        {
            return guildModel.GetGuildBattleOpponents();
        }

        /// <summary>
        /// 길드전 남은시간
        /// </summary>
        public RemainTime GetRemainTimeGuildBattle()
        {
            return guildModel.GuildBattleSeasonRemainTime;
        }

        /// <summary>
        /// 큐펫 정보 반환
        /// </summary>
        public CupetModel[] GetCupets()
        {
            return guildBattlePositionInfo.GetCupets();
        }

        /// <summary>
        /// 버프 정보 반환
        /// </summary>
        public GuildBattleBuffElement.IInput[] GetBuffs()
        {
            return guildBattlePositionInfo.GetBuffs();
        }

        /// <summary>
        /// 내 길드 정보 반환
        /// </summary>
        public UISingleGuildBattleElement.IInput GetMyGuildInfo()
        {
            return myGuildBattleInfo;
        }

        /// <summary>
        /// 길드 수비 내역 반환
        /// </summary>
        public UIGuildHistoryElement.IInput[] GetHistories()
        {
            return myGuildBattleInfo.GetHistories();
        }

        /// <summary>
        /// 타겟 길드 반환
        /// </summary>
        public UISingleGuildBattleElement.IInput GetTargetGuild()
        {
            return targetGuildBattleInfo;
        }

        /// <summary>
        /// 타겟 길드 포링포탑 (좌)
        /// </summary>
        public CupetModel[] GetLeftCupets()
        {
            return targetGuildBattleInfo.GetLeftCupets();
        }

        /// <summary>
        /// 타겟 길드 포링포탑 (우)
        /// </summary>
        public CupetModel[] GetRightCupets()
        {
            return targetGuildBattleInfo.GetRightCupets();
        }

        /// <summary>
        /// 선택 동료 존재 여부
        /// </summary>
        public bool GetHasAgent()
        {
            return targetGuildBattleInfo.AgentCid > 0;
        }

        /// <summary>
        /// 동료 프로필 반환
        /// </summary>
        public string GetAgentProfileName()
        {
            return targetGuildBattleInfo.GetAgentProfileName();
        }

        /// <summary>
        /// 동료 아이콘 반환
        /// </summary>
        public string GetAgentJobIconName()
        {
            return targetGuildBattleInfo.GetAgentJobIconName();
        }

        /// <summary>
        /// 내 길드 포링포탑 (좌)
        /// </summary>
        public CupetModel[] GetMyLeftCupets()
        {
            return myLeftCupets;
        }

        /// <summary>
        /// 내 길드 포링포탑 (우)
        /// </summary>
        public CupetModel[] GetMyRightCupets()
        {
            return myRightCupets;
        }

        /// <summary>
        /// 내 길드원 랭킹 정보
        /// </summary>
        public UIGuildBattleMyRankElement.IInput[] GetMyRanks()
        {
            return rankModel.GetRankInfos(RankType.MyGuildBattle);
        }

        /// <summary>
        /// 길드 선택
        /// </summary>
        public void SelectGuild(UIGuildBattleElement.IInput selected)
        {
            targetGuildBattleInfo.SelectGuild(selected);
            AsyncRequestGuildBattleTargetDetailInfo(selected.GuildId).WrapNetworkErrors();
        }

        /// <summary>
        /// 동료 선택
        /// </summary>
        public void SelectAgent(UIGuildSupportSelectElement.IInput selected)
        {
            targetGuildBattleInfo.SelectAgent(selected);
            OnSelectAgentInfo?.Invoke();
        }

        /// <summary>
        /// 동료 선택 취소
        /// </summary>
        public void UnselectAgent()
        {
            SelectAgent(null);
        }

        /// <summary>
        /// 큐펫 변경
        /// </summary>
        public void ChangeTurretCupet(ICupetModel cupet)
        {
            UI.Show<UICupetSelect>().Set(guildBattlePositionInfo.GetCupetIds(), RequestGuildBattleSupportCupetSetting, guildModel.UsedGuildBattleDefenseTurretCupetIds.ToArray());
        }

        /// <summary>
        /// 타 유저 큐펫 정보 보기
        /// </summary>
        /// <param name="cupet"></param>
        public void ShowOtherCupetInfo(ICupetModel cupet)
        {
            if (cupet == null)
                return;

            UI.Show<UIOtherCupetInfo>().SetEntity(cupet.CupetID, cupet.Rank, cupet.Level);
        }

        /// <summary>
        /// 타 유저 정보보기
        /// </summary>
        public void ShowCharacterInfo(int uid, int cid)
        {
            userModel.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }

        /// <summary>
        /// 큐펫 정보 목록 요청
        /// </summary>
        public void RequestGuildCupetInfo()
        {
            cupetListModel.RequestGuildCupetInfo().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드전 목록 호출
        /// </summary>
        public void RequestGuildList()
        {
            guildModel.RequestGuildBattleList().WrapNetworkErrors();
        }

        /// <summary>
        /// 전투 진형 호출
        /// </summary>
        public void RequestGuildBattleAttackPosition()
        {
            if (guildBattlePositionInfo.IsInitialize)
                return;

            AsyncRequestGuildBattleAttackPosition().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드전 전투정보 호출
        /// </summary>
        public void RequestGuildBattleDefInfo()
        {
            if (myGuildBattleInfo.IsInitialize)
                return;

            AsyncRequestGuildBattleDefInfo().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드전 수비 큐펫 정보 호출
        /// </summary>
        public void RequestGuildBattleDefCupetInfo()
        {
            if (myLeftCupets != null && myRightCupets != null)
                return;

            AsyncRequestGuildBattleDefCupetInfo().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드전 내 길드원 대미지 랭킹
        /// </summary>
        public void RequestGuildBattleGuildCharRank()
        {
            if (isRequestMyGuildRankInfo)
                return;

            isRequestMyGuildRankInfo = true;
            rankModel.RequestRankList(1, RankType.MyGuildBattle).WrapNetworkErrors();
        }

        /// <summary>
        /// 길드전 전투 큐펫 세팅 호출
        /// </summary>
        private void RequestGuildBattleSupportCupetSetting(int[] cupetIds)
        {
            AsyncRequestGuildBattleSupportCupetSetting(cupetIds).WrapNetworkErrors();
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void RequestStartBattle()
        {
            if (GetRemainCount() == 0)
            {
                UI.ShowToastPopup(LocalizeKey._33735.ToText()); // 하루 도전 횟수를 초과하였습니다.
                return;
            }

            AsyncStartBattle().WrapUIErrors();
        }

        /// <summary>
        /// 길드 레벨 반환
        /// </summary>
        private int GetGuildLevel(int guildExp)
        {
            return guildModel.GetGuildLevel(guildExp);
        }

        /// <summary>
        /// 큐펫 레벨 반환
        /// </summary>
        private int GetCupetLevel(int rank, int cupetExp)
        {
            int maxLevel = BasisType.MAX_CUPET_LEVEL.GetInt(rank);
            return Mathf.Min(maxLevel, expDataRepo.Get(cupetExp, ExpDataManager.ExpType.Cupet).level);
        }

        /// <summary>
        /// 길드전 전투진형정보 호출
        /// </summary>
        private async Task AsyncRequestGuildBattleAttackPosition()
        {
            Response response = await Protocol.REQUEST_GUILD_BATTLE_ATTACK_POSITION.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            GuildCupetPacket[] cupetPackets = response.ContainsKey("1") ? response.GetPacketArray<GuildCupetPacket>("1") : System.Array.Empty<GuildCupetPacket>();
            GuildBattleBuffPacket[] buffPackets = response.ContainsKey("2") ? response.GetPacketArray<GuildBattleBuffPacket>("2") : System.Array.Empty<GuildBattleBuffPacket>();
            cupetListModel.UpdateData(cupetPackets); // Cupet Update

            guildBattlePositionInfo.Initialize(cupetPackets, buffPackets);
            OnRequestGuildBattleAttackPositionInfo?.Invoke();
        }

        /// <summary>
        /// 길드전 전투정보 호출
        /// </summary>
        private async Task AsyncRequestGuildBattleDefInfo()
        {
            Response response = await Protocol.REQUEST_GUILD_BATTLE_DEF_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            int emperiumHp = response.GetInt("1");
            GuildBattleHistoryPacket[] historyPackets = response.ContainsKey("2") ? response.GetPacketArray<GuildBattleHistoryPacket>("2") : System.Array.Empty<GuildBattleHistoryPacket>();
            int exp = response.GetInt("3");

            foreach (var item in historyPackets)
            {
                item.SetMaxHp(maxEmperiumHp);
                item.SetGuildLevel(GetGuildLevel(item.exp));
            }

            myGuildBattleInfo.Initialize(emperiumHp, GetGuildLevel(exp), historyPackets);

            OnRequestGuildBattleDefenseInfo?.Invoke();
        }

        /// <summary>
        /// 길드전 전투 큐펫 세팅 호출
        /// </summary>
        private async Task AsyncRequestGuildBattleSupportCupetSetting(int[] cupetIds)
        {
            var sfs = Protocol.NewInstance();
            int length = cupetIds == null ? 0 : cupetIds.Length;

            int[] curCupetIds = guildBattlePositionInfo.GetCupetIds();
            int curLength = curCupetIds == null ? 0 : curCupetIds.Length;

            // 기존 세팅 중복 체크
            bool isDuplicate = length == curLength;
            if (isDuplicate)
            {
                for (int i = 0; i < length; i++)
                {
                    if (cupetIds[i] != curCupetIds[i])
                    {
                        isDuplicate = false;
                        break;
                    }
                }
            }

            // 기존 데이터와 동일
            if (isDuplicate)
            {
                Debug.Log("공격포탑 기존 세팅과 동일하다.");
                return;
            }

            if (length > 0)
            {
                sfs.PutIntArray("1", cupetIds);
            }

            Response response = await Protocol.REQUEST_GUILD_BATTLE_SUPPORT_CUPET_SETTING.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            guildBattlePositionInfo.UpdateCupets(cupetIds);
            OnRequestGuildBattleCupetSettings?.Invoke();
        }

        /// <summary>
        /// 길드전 상세 정보 호출
        /// </summary>
        private async Task AsyncRequestGuildBattleTargetDetailInfo(int guildId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", guildId);
            Response response = await Protocol.REQUEST_GUILD_BATTLE_TARGET_DETAIL_INFO.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            GuildCupetPacket[] targetLeftCupets = response.GetPacketArray<GuildCupetPacket>("1");
            GuildCupetPacket[] targetRightCupets = response.GetPacketArray<GuildCupetPacket>("2");
            GuildCupetPacket[] cupetPackets = response.ContainsKey("3") ? response.GetPacketArray<GuildCupetPacket>("3") : System.Array.Empty<GuildCupetPacket>();
            string usedAgent = response.GetUtfString("4");

            foreach (var item in targetLeftCupets)
            {
                item.SetCupetLevel(GetCupetLevel(item.CupetRank, item.exp));
            }

            foreach (var item in targetRightCupets)
            {
                item.SetCupetLevel(GetCupetLevel(item.CupetRank, item.exp));
            }

            cupetListModel.UpdateData(cupetPackets); // Cupet Update

            targetGuildBattleInfo.UpdateLeftCupets(targetLeftCupets);
            targetGuildBattleInfo.UpdateRightCupets(targetRightCupets);
            guildBattlePositionInfo.UpdateCupets(cupetPackets);
            OnRequestGuildBattleAttackPositionInfo?.Invoke();
            OnRequestGuildBattleListDetailInfo?.Invoke();
        }

        /// <summary>
        /// 길드전 수비 큐펫 정보 호출
        /// </summary>
        private async Task AsyncRequestGuildBattleDefCupetInfo()
        {
            Response response = await Protocol.REQUEST_GUILD_BATTLE_DEF_CUPET_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            GuildCupetPacket[] leftCupets = response.GetPacketArray<GuildCupetPacket>("1");
            GuildCupetPacket[] rightCupets = response.GetPacketArray<GuildCupetPacket>("2");
            foreach (var item in leftCupets)
            {
                cupetBuffer.Add(cupetListModel.Get(item.CupetId).Cupet);
            }
            myLeftCupets = cupetBuffer.GetBuffer(isAutoRelease: true);

            foreach (var item in rightCupets)
            {
                cupetBuffer.Add(cupetListModel.Get(item.CupetId).Cupet);
            }
            myRightCupets = cupetBuffer.GetBuffer(isAutoRelease: true);

            OnRequestMyGuildDefenseInfo?.Invoke();
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        private async Task AsyncStartBattle()
        {
            string message;
            if (guildBattlePositionInfo.HasCupetSettings())
            {
                message = LocalizeKey._33733.ToText(); // 전투를 시작하시겠습니까?
            }
            else
            {
                message = LocalizeKey._33734.ToText(); // 공격 큐펫을 선택하지 않았습니다.\n그래도 전투를 시작하시겠습니까?
            }

            if (!await UI.SelectPopup(message))
                return;

            battleManager.StartBattle(BattleMode.GuildBattle, targetGuildBattleInfo);
        }

        private class GuildBattlePositionInfo
        {
            public bool IsInitialize { get; private set; }

            private readonly CupetListModel.ICupetListModelImpl cupetListModelImpl;
            private readonly SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl;
            private readonly int levelUpExp;

            private CupetModel[] cupets;
            private int[] cupetIds;
            private BuffSkillInfo[] buffs;

            public GuildBattlePositionInfo(CupetListModel.ICupetListModelImpl cupetListModelImpl, SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl, int levelUpExp)
            {
                this.cupetListModelImpl = cupetListModelImpl;
                this.skillDataRepoImpl = skillDataRepoImpl;
                this.levelUpExp = levelUpExp;
            }

            public void Initialize(GuildCupetPacket[] cupetPackets, GuildBattleBuffPacket[] buffPackets)
            {
                IsInitialize = true;

                UpdateCupets(cupetPackets);

                int buffLength = buffPackets.Length;
                buffs = new BuffSkillInfo[buffLength];
                for (int i = 0; i < buffLength; i++)
                {
                    int skillLevel = buffPackets[i].TotalExp / levelUpExp;
                    SkillData skillData = skillDataRepoImpl.Get(buffPackets[i].SkillId, skillLevel);
                    buffs[i] = skillData == null ? null : new BuffSkillInfo(skillData);
                }
            }

            public void UpdateCupets(GuildCupetPacket[] cupetPackets)
            {
                int cupetLength = cupetPackets.Length;
                cupets = new CupetModel[cupetLength];
                cupetIds = new int[cupetLength];
                for (int i = 0; i < cupetLength; i++)
                {
                    int cupetId = cupetPackets[i].CupetId;
                    cupetIds[i] = cupetId;
                    CupetEntity entity = cupetListModelImpl.Get(cupetId);
                    cupets[i] = entity == null ? null : entity.Cupet;
                }
            }

            public void UpdateCupets(int[] ids)
            {
                int cupetLength = ids.Length;
                cupets = new CupetModel[cupetLength];
                cupetIds = new int[cupetLength];
                for (int i = 0; i < cupetLength; i++)
                {
                    int cupetId = ids[i];
                    cupetIds[i] = cupetId;
                    CupetEntity entity = cupetListModelImpl.Get(cupetId);
                    cupets[i] = entity == null ? null : entity.Cupet;
                }
            }

            public CupetModel[] GetCupets()
            {
                return cupets;
            }

            public int[] GetCupetIds()
            {
                return cupetIds;
            }

            /// <summary>
            /// 큐펫 세팅 여부
            /// </summary>
            public bool HasCupetSettings()
            {
                if (cupetIds != null)
                {
                    for (int i = 0; i < cupetIds.Length; i++)
                    {
                        if (cupetIds[i] > 0)
                            return true;
                    }
                }

                return false;
            }

            public GuildBattleBuffElement.IInput[] GetBuffs()
            {
                return buffs;
            }

            private class BuffSkillInfo : GuildBattleBuffElement.IInput
            {
                private readonly SkillData skillData;
                private readonly BattleOption battleOption;

                public int SkillId => skillData.id;
                public UISkillInfo.IInfo Skill => skillData;
                public string SkillName => GetSkillName();
                public string OptionTitle => battleOption.GetTitleText();
                public string OptionValue => battleOption.GetValueText();

                public BuffSkillInfo(SkillData data)
                {
                    skillData = data;
                    battleOption = new BattleOption(skillData.battle_option_type_1, skillData.value1_b1, skillData.value2_b1);
                }

                private string GetSkillName()
                {
                    return LocalizeKey._33811.ToText() // LV.{LEVEL} {NAME}
                        .Replace(ReplaceKey.LEVEL, skillData.lv)
                        .Replace(ReplaceKey.NAME, skillData.name_id.ToText());
                }
            }
        }

        private class MyGuildBattleInfo : UISingleGuildBattleElement.IInput
        {
            public bool IsInitialize { get; private set; }

            public int GuildId { get; private set; }
            public int GuildLevel { get; private set; }
            public string GuildName { get; private set; }
            public int Emblem { get; private set; }
            public int CurHp { get; private set; }
            public int MaxHp { get; private set; }

            private GuildBattleHistoryPacket[] histories;

            public MyGuildBattleInfo(int id, string name, int emblem, int maxHp)
            {
                GuildId = id;
                GuildName = name;
                Emblem = emblem;
                MaxHp = maxHp;
            }

            public void Initialize(int curHp, int level, GuildBattleHistoryPacket[] packets)
            {
                IsInitialize = true;

                CurHp = curHp;
                GuildLevel = level;
                histories = packets;
            }

            public UIGuildHistoryElement.IInput[] GetHistories()
            {
                return histories;
            }
        }

        private class TargetGuildBattleInfo : UISingleGuildBattleElement.IInput, GuildBattleEntry.IGuildBattleInput
        {
            private CupetModel[] leftCupets, rightCupets;

            public int CurHp { get; private set; }
            public int MaxHp { get; private set; }
            public int GuildId { get; private set; }
            public int GuildLevel { get; private set; }
            public string GuildName { get; private set; }
            public int Emblem { get; private set; }

            public int AgentCid { get; private set; }
            public int AgentUid { get; private set; }
            private string agentProfileName;
            private string agentJobIconName;

            public TargetGuildBattleInfo(int maxHp)
            {
                MaxHp = maxHp;
            }

            public void SelectGuild(UIGuildBattleElement.IInput selectedGuild)
            {
                CurHp = selectedGuild.CurHp;
                GuildId = selectedGuild.GuildId;
                GuildLevel = selectedGuild.GuildLevel;
                GuildName = selectedGuild.GuildName;
                Emblem = selectedGuild.Emblem;
            }

            public void SelectAgent(UIGuildSupportSelectElement.IInput selectedAgent)
            {
                if (selectedAgent == null)
                {
                    agentProfileName = string.Empty;
                    agentJobIconName = string.Empty;
                    AgentCid = 0;
                    AgentUid = 0;
                }
                else
                {
                    agentProfileName = selectedAgent.ProfileName;
                    agentJobIconName = selectedAgent.JobIconName;
                    AgentCid = selectedAgent.Cid;
                    AgentUid = selectedAgent.Uid;
                }
            }

            public void UpdateLeftCupets(GuildCupetPacket[] cupetPackets)
            {
                int cupetLength = cupetPackets.Length;
                leftCupets = new CupetModel[cupetLength];
                for (int i = 0; i < cupetLength; i++)
                {
                    CupetEntity entity = CupetEntity.Factory.CreateDummyCupet(cupetPackets[i].CupetId, cupetPackets[i].CupetRank, cupetPackets[i].CupetLevel);
                    leftCupets[i] = entity.Cupet;
                }
            }

            public void UpdateRightCupets(GuildCupetPacket[] cupetPackets)
            {
                int cupetLength = cupetPackets.Length;
                rightCupets = new CupetModel[cupetLength];
                for (int i = 0; i < cupetLength; i++)
                {
                    CupetEntity entity = CupetEntity.Factory.CreateDummyCupet(cupetPackets[i].CupetId, cupetPackets[i].CupetRank, cupetPackets[i].CupetLevel);
                    rightCupets[i] = entity.Cupet;
                }
            }

            public CupetModel[] GetLeftCupets()
            {
                return leftCupets;
            }

            public CupetModel[] GetRightCupets()
            {
                return rightCupets;
            }

            public string GetAgentProfileName()
            {
                return agentProfileName;
            }

            public string GetAgentJobIconName()
            {
                return agentJobIconName;
            }
        }
    }
}