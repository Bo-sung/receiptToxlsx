using Ragnarok.View;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGate"/>
    /// </summary>
    public sealed class GatePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly UserModel userModel;
        private readonly CharacterModel characterModel;
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly GateDataManager gateDataRepo;
        private readonly MonsterDataManager monsterDataRepo;
        private readonly SkillDataManager skillDataRepo;
        private readonly ProfileDataManager profileDataRepo;
        private readonly BetterList<GatePartyRoomSlot> partyList;
        private readonly int buyTicketCatCoin; // 유료 입장 비용
        private readonly int buyTicketIncCatCoin; // 유료 입장 비용 가중치

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Event --!>
        public event System.Action OnPartyReady;
        public event System.Action OnPartyList;
        public event System.Action OnPartyExit;
        public event System.Action OnUpdateMultiMazeTicket
        {
            add { dungeonModel.OnUpdateMultiMazeTicket += value; }
            remove { dungeonModel.OnUpdateMultiMazeTicket -= value; }
        }

        private GateData currentData;
        private GatePartyReadySlot[] myPartyInfos;
        private int myPartyChannel;
        private int myPartyLeaderCid;

        public GatePresenter()
        {
            userModel = Entity.player.User;
            characterModel = Entity.player.Character;
            dungeonModel = Entity.player.Dungeon;

            gateDataRepo = GateDataManager.Instance;
            monsterDataRepo = MonsterDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;
            partyList = new BetterList<GatePartyRoomSlot>();
            buyTicketCatCoin = BasisType.MULTI_MAZE_CAT_COIN_JOIN.GetInt();
            buyTicketIncCatCoin = BasisType.MULTI_MAZE_CAT_COIN_INC.GetInt();

            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            Protocol.RECEIVE_GATE_GETMYROOMLIST.AddEvent(OnReceiveGateGetMyRoomList);
            Protocol.REQUEST_GATE_USEROUT.AddEvent(OnRequestGateUserOut);
            Protocol.REQUEST_GATE_GETALLROOMLIST.AddEvent(OnRequestGateGetAllRoomList);
            Protocol.REQUEST_GATE_JOINROOM.AddEvent(OnRequestGateJoinRoom);
            Protocol.REQUEST_GATE_BANUSER.AddEvent(OnRequestGateBanUser);
            Protocol.REQUEST_GATE_GAMESTART.AddEvent(OnRequestGateGameStart);
        }

        public override void RemoveEvent()
        {
            Protocol.RECEIVE_GATE_GETMYROOMLIST.RemoveEvent(OnReceiveGateGetMyRoomList);
            Protocol.REQUEST_GATE_USEROUT.RemoveEvent(OnRequestGateUserOut);
            Protocol.REQUEST_GATE_GETALLROOMLIST.RemoveEvent(OnRequestGateGetAllRoomList);
            Protocol.REQUEST_GATE_JOINROOM.RemoveEvent(OnRequestGateJoinRoom);
            Protocol.REQUEST_GATE_BANUSER.RemoveEvent(OnRequestGateBanUser);
            Protocol.REQUEST_GATE_GAMESTART.RemoveEvent(OnRequestGateGameStart);
        }

        public void SetGateId(int id)
        {
            currentData = gateDataRepo.Get(id);
            myPartyInfos = new GatePartyReadySlot[currentData.max_user];
            for (int i = 0; i < myPartyInfos.Length; i++)
            {
                myPartyInfos[i] = new GatePartyReadySlot(profileDataRepo, skillDataRepo);
            }
        }

        public int GetGateId()
        {
            return currentData.id;
        }

        public int GetNameId()
        {
            return currentData.name_id;
        }

        public int GetDescriptionId()
        {
            return currentData.GetDescriptionId();
        }

        public string GetStageIcon()
        {
            return currentData.GetIcon();
        }

        public string GetMonsterIcon()
        {
            MonsterData monsterData = monsterDataRepo.Get(currentData.GetBossId());
            if (monsterData == null)
                return string.Empty;

            return monsterData.icon_name;
        }

        public UIGatePartyReadySlot.IInput[] GetMyPartyInfos()
        {
            return myPartyInfos;
        }

        public bool IsLeader()
        {
            return myPartyLeaderCid == characterModel.Cid;
        }

        public UIGatePartyJoinElement.IInput[] GetPartyInfos()
        {
            return partyList.ToArray();
        }

        public int GetTicketCount()
        {
            return dungeonModel.GetFreeEntryCount(DungeonType.Gate);
        }

        public int GetTicketMaxCount()
        {
            return dungeonModel.GetFreeEntryMaxCount(DungeonType.Gate);
        }

        public int GetNeedCatCoint()
        {
            int entryCount = dungeonModel.GetEntryCount(DungeonType.Gate); // 실제 입장 횟수
            int overCount = entryCount - GetTicketMaxCount(); // 추가 입장 하려는 횟수
            int needCoin = buyTicketCatCoin + (buyTicketIncCatCoin * overCount); // 필요 냥다래
            return needCoin;
        }

        public void RequestCreateParty()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", GetGateId());
            Protocol.REQUEST_GATE_MAKEROOM.SendAsync(sfs).WrapNetworkErrors();
        }

        public void RequestGetAllRoomList()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", GetGateId());
            Protocol.REQUEST_GATE_GETALLROOMLIST.SendAsync(sfs).WrapNetworkErrors();
        }

        public void RequestJoinParty(int channelId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", channelId);
            Protocol.REQUEST_GATE_JOINROOM.SendAsync(sfs).WrapNetworkErrors();
        }

        public void RequestUserBan(int cid)
        {
            if (!HasCharacter(cid))
                return;

            RequestUserBanAsync(cid).WrapUIErrors();
        }

        public void RequestExitParty()
        {
            Protocol.REQUEST_GATE_USEROUT.SendAsync().WrapNetworkErrors();
        }

        public void ShowOtherUserInfo(int uid, int cid)
        {
            userModel.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }

        public void RequestStartParty()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", myPartyChannel);
            Protocol.REQUEST_GATE_GAMESTART.SendAsync(sfs).WrapNetworkErrors();
        }

        void OnReceiveGateGetMyRoomList(Response response)
        {
            GateRoomPacket room = response.GetPacket<GateRoomPacket>("1");

            myPartyChannel = room.channelId;
            myPartyLeaderCid = room.leaderCid;

            int length = room.players.Length;
            for (int i = 0; i < myPartyInfos.Length; i++)
            {
                if (i < length)
                {
                    bool isLeader = room.players[i].cid == myPartyLeaderCid;
                    myPartyInfos[i].Set(isLeader, room.players[i]);
                }
                else
                {
                    myPartyInfos[i].Reset();
                }
            }

            OnPartyReady?.Invoke();
        }

        void OnRequestGateUserOut(Response response)
        {
            for (int i = 0; i < myPartyInfos.Length; i++)
            {
                myPartyInfos[i].Reset();
            }
            myPartyChannel = 0;
            myPartyLeaderCid = 0;

            OnPartyExit?.Invoke();
        }

        void OnRequestGateGetAllRoomList(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                ClearPartyList();
                return;
            }

            GateRoomPacket[] rooms = response.ContainsKey("1") ? response.GetPacketArray<GateRoomPacket>("1") : System.Array.Empty<GateRoomPacket>();
            for (int i = 0; i < rooms.Length; i++)
            {
                GatePartyRoomSlot slot = FindParty(rooms[i].channelId);
                slot.Set(rooms[i]);
            }

            OnPartyList?.Invoke();
        }

        void OnRequestGateJoinRoom(Response response)
        {
            if (!response.isSuccess)
            {
                ClearPartyList();
                RequestGetAllRoomList(); // 실패 시 리스트 다시 요청
                return;
            }
        }

        void OnRequestGateBanUser(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }
        }

        void OnRequestGateGameStart(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            battleManager.StartBattle(BattleMode.GateMaze, response.GetPacket<GateMultiMazePacket>());
        }

        private void ClearPartyList()
        {
            partyList.Clear();
            OnPartyList?.Invoke();
        }

        private GatePartyRoomSlot FindParty(int channelId)
        {
            for (int i = 0; i < partyList.size; i++)
            {
                if (partyList[i].channelId == channelId)
                    return partyList[i];
            }

            GatePartyRoomSlot instance = new GatePartyRoomSlot(profileDataRepo, currentData.max_user);
            partyList.Add(instance);
            return instance;
        }

        private string FindName(int cid)
        {
            for (int i = 0; i < myPartyInfos.Length; i++)
            {
                if (myPartyInfos[i].Cid == cid)
                    return myPartyInfos[i].Name;
            }

            return string.Empty;
        }

        private bool HasCharacter(int cid)
        {
            for (int i = 0; i < myPartyInfos.Length; i++)
            {
                if (myPartyInfos[i].Cid == cid)
                    return true;
            }

            return false;
        }

        private async Task RequestUserBanAsync(int cid)
        {
            string name = FindName(cid);
            string message = LocalizeKey._6916.ToText() // {NAME}님을 탈퇴 시키겠습니까?
                .Replace(ReplaceKey.NAME, name);

            if (!await UI.SelectPopup(message))
                return;

            // 팝업 도중 캐릭터가 퇴장할 수 있음
            if (!HasCharacter(cid))
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", cid);
            Protocol.REQUEST_GATE_BANUSER.SendAsync(sfs).WrapNetworkErrors();
        }

        private abstract class PlayerSlot
        {
            private readonly ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl;

            public bool HasCharacter { get; protected set; }

            public bool IsLeader { get; protected set; }
            public int Uid { get; protected set; }
            public int Cid { get; protected set; }
            public string ProfileName { get; protected set; }
            public string JobIconName { get; protected set; }
            public int Level { get; protected set; }
            public string Name { get; protected set; }
            public int BattleScore { get; protected set; }

            public event System.Action OnUpdate;

            public PlayerSlot(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
            {
                this.profileDataRepoImpl = profileDataRepoImpl;
            }

            public virtual void Reset()
            {
                HasCharacter = false;
                IsLeader = false;
                Uid = 0;
                Cid = 0;
                ProfileName = string.Empty;
                JobIconName = string.Empty;
                Level = 0;
                Name = string.Empty;
                BattleScore = 0;

                OnUpdate?.Invoke();
            }

            public virtual void Set(bool isLeader, GateRoomPlayerPacket packet)
            {
                Set(isLeader, packet.uid, packet.cid, packet.job, packet.gender, packet.profileId, packet.jobLevel, packet.name, packet.battleScore);
            }

            protected void Set(bool isLeader, int uid, int cid, Job job, Gender gender, int profileId, int level, string name, int battleScore)
            {
                HasCharacter = true;
                IsLeader = isLeader;
                Uid = uid;
                Cid = cid;
                ProfileName = GetProfileName(job, gender, profileId);
                JobIconName = GetJobIconName(job);
                Level = level;
                Name = name;
                BattleScore = battleScore;

                OnUpdate?.Invoke();
            }

            protected string GetJobIconName(Job job)
            {
                return job.GetJobIcon();
            }

            protected string GetProfileName(Job job, Gender gender, int profileId)
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

        private class GatePartyReadySlot : PlayerSlot, UIGatePartyReadySlot.IInput
        {
            private readonly SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl;
            private readonly string[] skillIcons;

            public GatePartyReadySlot(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl, SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl) : base(profileDataRepoImpl)
            {
                this.skillDataRepoImpl = skillDataRepoImpl;
                skillIcons = new string[Constants.Size.MAX_SKILL_SLOT_SIZE];

                Reset();
            }

            public override void Reset()
            {
                for (int i = 0; i < skillIcons.Length; i++)
                {
                    skillIcons[i] = string.Empty;
                }

                base.Reset();
            }

            public void Set(bool isLeader, PlayerEntity player)
            {
                for (int i = 0; i < player.Skill.SkillSlotCount; i++)
                {
                    int skillId = player.Skill.GetSkillId(i); // 실제 skillId
                    int battleSkillId = player.Skill.GetBattleSkillId(skillId); // 전투 skillId
                    if (battleSkillId == 0)
                        continue;

                    int skillLevel = player.Skill.GetSkillLevel(skillId);
                    SkillData skillData = skillDataRepoImpl.Get(battleSkillId, skillLevel);
                    if (skillData == null)
                        continue;

                    skillIcons[i] = skillData.icon_name;
                }

                CharacterModel character = player.Character;
                Set(isLeader, player.User.UID, character.Cid, character.Job, character.Gender, character.ProfileId, character.JobLevel, character.Name, player.SavedBattleStatusData.AP);
            }

            public override void Set(bool isLeader, GateRoomPlayerPacket packet)
            {
                int skillLength = packet.skills == null ? 0 : packet.skills.Length;
                SkillData skillData;
                for (int i = 0; i < skillLength; i++)
                {
                    skillData = skillDataRepoImpl.Get(packet.skills[i], level: 1);
                    if (skillData == null)
                        continue;

                    skillIcons[i] = skillData.icon_name;
                }

                base.Set(isLeader, packet);
            }

            public string GetSkillIconName(int index)
            {
                int skillLength = skillIcons == null ? 0 : skillIcons.Length;
                return index < skillLength ? skillIcons[index] : string.Empty;
            }
        }

        private class GatePartyRoomSlot : UIGatePartyJoinElement.IInput
        {
            private class RoomSlot : PlayerSlot, UIGatePartyJoinSlot.IInput
            {
                public int ChannelId { get; private set; }

                public RoomSlot(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl) : base(profileDataRepoImpl)
                {
                }

                public void SetChannelId(int channelId)
                {
                    ChannelId = channelId;
                }
            }

            private readonly RoomSlot[] inputs;

            public UIGatePartyJoinSlot.IInput[] Inputs => inputs;
            public int channelId;

            public GatePartyRoomSlot(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl, int maxUserCount)
            {
                inputs = new RoomSlot[maxUserCount];

                for (int i = 0; i < maxUserCount; i++)
                {
                    inputs[i] = new RoomSlot(profileDataRepoImpl);
                }
            }

            public void Set(GateRoomPacket packet)
            {
                channelId = packet.channelId;
                int leaderCid = packet.leaderCid;

                int playerLength = packet.players == null ? 0 : packet.players.Length;
                for (int i = 0; i < inputs.Length; i++)
                {
                    inputs[i].SetChannelId(channelId); // 채널은 무조건 존재 필요

                    if (i < playerLength)
                    {
                        bool isLeader = leaderCid == packet.players[i].cid;
                        inputs[i].Set(isLeader, packet.players[i]);
                    }
                    else
                    {
                        inputs[i].Reset();
                    }
                }
            }
        }
    }
}