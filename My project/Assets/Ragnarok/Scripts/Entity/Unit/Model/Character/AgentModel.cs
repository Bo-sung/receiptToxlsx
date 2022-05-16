using Sfs2X.Entities.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 동료 관리
    /// </summary>
    public class AgentModel : CharacterEntityModel
    {
        public interface IAgentValue
        {
            int CID { get; }
            int DuplicationCount { get; }
            int ID { get; }
            ExploreType ExploreType { get; }
            long ExploreRewardRemainTime { get; }
            int ExploreStageID { get; }
            bool IsExploring { get; }
        }

        public interface IAgentBookValue
        {
            int Id { get; }
        }

        [System.Serializable]
        public struct ComposeRequestData
        {
            public int composeIndex;
            public int agentID;
            public int count;
        }

        private readonly AgentDataManager agentDataRepo;
        private readonly AgentBookDataManager agentBookDataRepo;

        private readonly List<CombatAgent> combatAgentList;
        private readonly List<ExploreAgent> exploreAgentList;

        private readonly List<AgentSlotInfoPacket> slotStateList;
        private readonly List<AgentExploreCountInfo> exploreCountInfoList;
        public readonly List<AgentBookState> bookStateList;

        private readonly Dictionary<int, AgentExploreState> exploreStateDic;

        private readonly List<(int, AgentExploreState)> updateBuffer = new List<(int, AgentExploreState)>();
        private readonly List<int> lastNewAgentIDs = new List<int>();

        /// <summary>
        /// 동료 목록 변경 시 호출 (전직 후 동료목록 변경, 동료 장착 등)
        /// </summary>
        public event System.Action OnUpdateAgentEquipmentState;
        public event System.Action OnStatusReloadRequired;
        public event System.Action OnGetNewAgent;
        public event System.Action OnMessageNoticeUpdate;
        public event System.Action<int, AgentExploreState> OnExploreStateChanged;
        public event System.Action OnUpdateRemainTradeCount; // 교역 & 생산 횟수 변경 이벤트
        public event System.Action OnResetTradeCount; // 교역 & 생산 초기화 이벤트
        public event System.Action<ExploreType> OnExploreStart; // 파견 시작 이벤트
        public event System.Action<int> OnAgentBookEnable; // 파견 인연 도감 받기 이벤트
        public event System.Action<AgentExploreState> OnAgentExploreReward; // 파견 보상 요청 완료 이벤트
        public event System.Action OnAgentExploreCancel; // 파견 포기 이벤트

        public int EquippedAgentCount { get; private set; }
        public int CombatAgentSlotCount { get { return slotStateList.Count; } }

        // 남은시간이 가장 짧은 파견?
        public AgentExploreState FastestExploreState { get; private set; }

        public AgentModel()
        {
            agentDataRepo = AgentDataManager.Instance;
            agentBookDataRepo = AgentBookDataManager.Instance;

            combatAgentList = new List<CombatAgent>();
            exploreAgentList = new List<ExploreAgent>();
            slotStateList = new List<AgentSlotInfoPacket>();
            exploreCountInfoList = new List<AgentExploreCountInfo>();
            bookStateList = new List<AgentBookState>();
            exploreStateDic = new Dictionary<int, AgentExploreState>(IntEqualityComparer.Default);
        }

        public override void ResetData()
        {
            base.ResetData();

            CombatAgent.ResetNewCount();
            ExploreAgent.ResetNewCount();

            combatAgentList.Clear();
            exploreAgentList.Clear();
            slotStateList.Clear();
            exploreCountInfoList.Clear();
            bookStateList.Clear();
            exploreStateDic.Clear();
            EquippedAgentCount = 0;
            FastestExploreState = null;
        }

        public IAgentValue[] GetAllAgentValues()
        {
            return GetAllAgents().ToArray();
        }

        public IEnumerable<IAgent> GetAllAgents()
        {
            for (int i = 0; i < combatAgentList.Count; ++i)
            {
                yield return combatAgentList[i];
            }

            for (int i = 0; i < exploreAgentList.Count; ++i)
            {
                yield return exploreAgentList[i];
            }

            yield break;
        }

        public IEnumerable<CombatAgent> GetEquipedCombatAgents()
        {
            for (int i = 0; i < combatAgentList.Count; ++i)
            {
                if (combatAgentList[i].IsUsingAgent)
                {
                    yield return combatAgentList[i];
                }
            }

            yield break;
        }

        public bool HasEquippedCombatAgentCharacters()
        {
            for (int i = 0; i < combatAgentList.Count; ++i)
            {
                if (combatAgentList[i].IsUsingAgent)
                    return true;
            }

            return false;
        }

        public IEnumerable<CombatAgent> GetCombatAgents() => combatAgentList;

        public IEnumerable<ExploreAgent> GetExploreAgents() => exploreAgentList;

        public IEnumerable<AgentSlotInfoPacket> GetSlotStates() => slotStateList;

        public IEnumerable<AgentExploreCountInfo> GetExploreCountInfos() => exploreCountInfoList;

        public IEnumerable<AgentExploreState> GetExplores() => exploreStateDic.Values;

        public IEnumerable<AgentBookState> GetBookStates() => bookStateList;

        public IEnumerable<AgentBookState> GetEnabledBookStates()
        {
            for (int i = 0; i < bookStateList.Count; ++i)
            {
                if (bookStateList[i].IsRewarded)
                {
                    yield return bookStateList[i];
                }
            }
        }

        public bool HasNewAgent(AgentType agentType = AgentType.None)
        {
            if (agentType == AgentType.CombatAgent)
                return CombatAgent.NewAgentCount > 0;
            if (agentType == AgentType.ExploreAgent)
                return ExploreAgent.NewAgentCount > 0;
            return CombatAgent.NewAgentCount > 0 || ExploreAgent.NewAgentCount > 0;
        }

        public bool CanCompleteBook()
        {
            return bookStateList.Find(v => v.CanComplete()) != null;
        }

        /// <summary>
        /// 동료 장착 가능 여부
        /// </summary>
        public bool CanEquipAgent()
        {
            return HasEmptyEquipSlot() && HasUnusingAgent();
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (Entity.Daily != null)
            {
                Entity.Daily.OnResetDailyEvent += ResetTradeCounts;
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (Entity.Daily != null)
            {
                Entity.Daily.OnResetDailyEvent -= ResetTradeCounts;
            }
        }

        internal void Initialize(IAgentValue[] agentPackets, IAgentBookValue[] agentBookIDs)
        {
            ResetData();

            List<AgentData> agentDataList = new List<AgentData>();

            foreach (var each in agentBookDataRepo.GetWholeBookDatas())
            {
                foreach (int eachAgentID in each.GetAgentIDs())
                {
                    agentDataList.Add(agentDataRepo.Get(eachAgentID));
                }

                bookStateList.Add(new AgentBookState(each, agentDataList.ToArray()));
                agentDataList.Clear();
            }

            bookStateList.Sort((a, b) => a.BookData.id - b.BookData.id);

            foreach (var item in agentBookIDs)
            {
                bookStateList.Find(v => v.BookData.id == item.Id)?.SetEnabled();
            }

            if (agentPackets != null)
            {
                for (int i = 0; i < agentPackets.Length; ++i)
                {
                    var agent = AddAgent(agentPackets[i]);
                    UpdateRelatedExploreIfNeed(agent, agentPackets[i]);
                }
            }

            UpdateFastestExploreState();

            updateBuffer.Clear();
            OnStatusReloadRequired?.Invoke();
        }

        internal void UpdateSlotState(AgentSlotInfoPacket[] equipmentStates)
        {
            EquippedAgentCount = 0;

            slotStateList.Clear();
            if (equipmentStates != null)
                slotStateList.AddRange(equipmentStates);

            slotStateList.Sort((a, b) => a.OrderID - b.OrderID);

            for (int i = 0; i < combatAgentList.Count; ++i)
            {
                combatAgentList[i].SetUsingSlot(-1);
            }

            for (int i = 0; i < slotStateList.Count; ++i)
            {
                if (slotStateList[i].IsUsingSlot)
                {
                    ++EquippedAgentCount;
                    combatAgentList.Find(v => v.ID == slotStateList[i].AgentID).SetUsingSlot(slotStateList[i].SlotNo);
                }
            }

            OnUpdateAgentEquipmentState?.Invoke();
        }

        internal void InitializeExploreCountInfo(AgentExploreCountInfo[] countInfos)
        {
            exploreCountInfoList.Clear();
            exploreCountInfoList.AddRange(countInfos);
        }

        internal void UpdateAgentData(UpdateCharAgentPacket[] updatePackets)
        {
            if (updatePackets == null || updatePackets.Length == 0)
                return;

            lastNewAgentIDs.Clear();

            bool needStatusReload = false;
            bool thereIsNewAgent = false;

            for (int i = 0; i < updatePackets.Length; ++i)
            {
                var each = updatePackets[i];
                var packet = each.Packet;

                Debug.Log($"[AgentModel] DirtyType : {each.DirtyType}, CID : {packet.CID}, Count : {packet.DuplicationCount}, AgentID : {packet.ID}, {AgentDataManager.Instance.Get(packet.ID).name_id.ToText()}");
                IAgent agent = null;

                if (each.DirtyType == DirtyType.Insert)
                {
                    // Update이지만 Add로 들어올 수 있다 
                    agent = GetAllAgents().FirstOrDefault(v => v.ID == packet.ID);
                    if (agent != null)
                    {
                        Debug.Log($"[AgentModel] Update처리 DirtyType : {each.DirtyType}, CID : {packet.CID}, Count : {packet.DuplicationCount}, AgentID : {packet.ID}");
                        int questValue = packet.DuplicationCount - agent.DuplicationCount;
                        Quest.QuestProgress(QuestType.AGENT_COUNT, questValue: questValue); // 동료 획득 횟수
                        agent.UpdateData(packet);
                    }
                    else
                    {
                        agent = AddAgent(packet);
                        agent.IsNew = true;
                        lastNewAgentIDs.Add(agent.ID);
                        needStatusReload = true;
                        thereIsNewAgent = true;

                        if (agent == null)
                        {
                            Debug.LogError($"[AgentModel] Agent Add 에 실패하였습니다.");
                            continue;
                        }

                        // 동료 획득                
                        Quest.QuestProgress(QuestType.AGENT_COUNT); // 동료 획득 횟수
                    }
                }
                else if (each.DirtyType == DirtyType.Update)
                {
                    agent = GetAllAgents().FirstOrDefault(v => v.ID == packet.ID);

                    if (agent == null)
                    {
                        Debug.LogError($"[AgentModel] Update 할 Agent 를 찾지 못했습니다.");
                        continue;
                    }

                    agent.UpdateData(packet);
                }
                else if (each.DirtyType == DirtyType.Delete)
                {
                    Debug.LogError($"[AgentModel] Agent 는 Delete 될 수 없습니다.");
                    continue;
                }

                UpdateRelatedExploreIfNeed(agent, packet);
            }

            UpdateFastestExploreState();

            if (needStatusReload)
            {
                OnStatusReloadRequired?.Invoke();
            }

            if (thereIsNewAgent)
            {
                OnGetNewAgent?.Invoke();
                OnMessageNoticeUpdate?.Invoke();
            }

            for (int i = 0; i < updateBuffer.Count; ++i)
            {
                OnExploreStateChanged?.Invoke(updateBuffer[i].Item1, updateBuffer[i].Item2);
            }

            updateBuffer.Clear();
        }

        private void UpdateRelatedExploreIfNeed(IAgent agent, IAgentValue packet)
        {
            if (agent is ExploreAgent exploreAgent)
            {
                if (exploreAgent.IsExploring != packet.IsExploring)
                {
                    if (packet.IsExploring)
                    {
                        exploreStateDic.TryGetValue(packet.ExploreStageID, out AgentExploreState progressingExplore);

                        if (progressingExplore == null)
                        {
                            progressingExplore = new AgentExploreState(packet.ExploreStageID, packet.ExploreType, packet.ExploreRewardRemainTime);
                            exploreStateDic.Add(progressingExplore.StageID, progressingExplore);

                            if (updateBuffer.FindIndex(v => v.Item1 == progressingExplore.StageID) == -1)
                                updateBuffer.Add((progressingExplore.StageID, progressingExplore));
                        }

                        //Debug.LogError($"{packet.ExploreStageID} {exploreAgent.AgentData.id} {exploreAgent.AgentData.name_id.ToText()}");
                        progressingExplore.AddParticipant(exploreAgent);
                        exploreAgent.SetProgressingExplore(progressingExplore);
                    }
                    else
                    {
                        var progressingExplore = exploreAgent.ProgressingExplore;

                        if (exploreStateDic.ContainsKey(progressingExplore.StageID))
                            exploreStateDic.Remove(progressingExplore.StageID);

                        exploreAgent.SetProgressingExplore(null);

                        if (updateBuffer.FindIndex(v => v.Item1 == progressingExplore.StageID) == -1)
                            updateBuffer.Add((progressingExplore.StageID, null));
                    }
                }
                else
                {
                    if (exploreAgent.IsExploring)
                    {
                        exploreAgent.ProgressingExplore.UpdateRemainTime(packet.ExploreRewardRemainTime);
                    }
                }
            }
        }

        private void UpdateFastestExploreState()
        {
            FastestExploreState = null;

            foreach (var each in exploreStateDic)
            {
                if (FastestExploreState == null || each.Value.RemainTime < FastestExploreState.RemainTime)
                {
                    FastestExploreState = each.Value;
                }
            }
        }

        public AgentExploreState GetExploreState(int stageID)
        {
            exploreStateDic.TryGetValue(stageID, out AgentExploreState ret);
            return ret;
        }

        public bool IsNewAgent(int agentID)
        {
            return lastNewAgentIDs.Contains(agentID);
        }

        public async Task RequestAgentCompose(ComposeRequestData[] composeDatas, bool combatAgentCompose)
        {
            SFSObject sfs = SFSObject.NewInstance();
            SFSArray array = new SFSArray();

            for (int i = 0; i < composeDatas.Length; ++i)
            {
                SFSObject each = SFSObject.NewInstance();
                each.PutInt("1", composeDatas[i].agentID);
                each.PutInt("2", composeDatas[i].count);
                array.AddSFSObject(each);
            }

            sfs.PutSFSArray("1", array);
            sfs.PutInt("2", combatAgentCompose ? 1 : 2);
            Response response = await Protocol.REQUEST_AGENT_COMPOSE.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                var update = response.GetPacket<CharUpdateData>("cud");
                Notify(update);
                UI.RewardInfo(update.rewards);

                // 퀘스트 처리
                Quest.QuestProgress(QuestType.AGENT_COMPOSE_COUNT); // 동료 합성 횟수
            }
        }

        private IAgent AddAgent(IAgentValue packet)
        {
            var agentData = agentDataRepo.Get(packet.ID);

            if (agentData == null)
            {
                Debug.LogError($"[AgentModel] AgentData 를 찾을 수 없습니다. : {packet.ID}");
                return null;
            }

            IAgent result = null;
            AgentType agentType = (AgentType)(int)agentData.agent_type;

            if (agentType == AgentType.CombatAgent)
            {
                CombatAgent agent = new CombatAgent(packet, agentData);
                combatAgentList.Add(agent);
                result = agent;
            }
            else if (agentType == AgentType.ExploreAgent)
            {
                ExploreAgent agent = new ExploreAgent(packet, agentData);
                exploreAgentList.Add(agent);
                result = agent;
            }

            for (int i = 0; i < bookStateList.Count; ++i)
            {
                if (bookStateList[i].IsRequireAgent(packet.ID))
                {
                    bookStateList[i].AddOwningAgent(packet.ID);
                }
            }

            return result;
        }

        public async Task RequestAgentSlotUpdate(long slotNo, int agentID)
        {
            SFSObject sfs = SFSObject.NewInstance();
            sfs.PutLong("1", slotNo);
            sfs.PutInt("2", agentID);
            Response response = await Protocol.REQUEST_AGENT_SLOT_UPDATE.SendAsync(sfs);

            if (response.isSuccess)
            {
                var equipmentState = response.GetPacketArray<AgentSlotInfoPacket>("1");
                UpdateSlotState(equipmentState);
            }
        }

        /// <summary>
        /// 파견 시작 요청
        /// </summary>
        public async Task RequestAgentExploreStart(ExploreAgent[] exploreAgent, int stageID, ExploreType exploreType)
        {
            AgentExploreCountInfo countInfo = exploreCountInfoList.Find(v => v.StageID == stageID);

            if (countInfo == null)
            {
                Debug.LogError("[AgentModel] 클리어하지 않은 스테이지로 파견을 보내려고 하였습니다.");
                return;
            }

            if (exploreType == ExploreType.Trade && (countInfo != null && countInfo.remainTradeCount == 0))
            {
                Debug.LogError("[AgentModel] 남은 교역 횟수가 0 입니다.");
                return;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", stageID);
            int[] agentIDs = new int[exploreAgent.Length];
            for (int i = 0; i < agentIDs.Length; ++i)
                agentIDs[i] = exploreAgent[i].ID;
            sfs.PutIntArray("2", agentIDs);
            sfs.PutInt("3", (int)exploreType);

            var response = await Protocol.REQUEST_AGENT_EXPLORE_START.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            var update = response.GetPacket<CharUpdateData>("cud");
            Notify(update);

            if (exploreType == ExploreType.Trade || exploreType == ExploreType.Production)
            {
                if (countInfo != null)
                {
                    --countInfo.remainTradeCount;                   
                }
            }

            // 퀘스트 처리
            Quest.QuestProgress(QuestType.AGENT_EXPLORE_COUNT); // 동료 파견 보내기 횟수
            OnUpdateRemainTradeCount?.Invoke();
            OnExploreStart?.Invoke(exploreType);
        }

        /// <summary>
        /// 탐험 보상 요청
        /// </summary>
        public async Task RequestAgentExploreReward(AgentExploreState explore, bool fastClear)
        {
            if (fastClear)
            {
                if (Entity.Goods.CatCoin < GetCatcoinFastClear(explore.RemainTime))
                    return;
            }

            List<ExploreAgent> agentList = new List<ExploreAgent>();
            foreach (ExploreAgent each in explore.Participants)
            {
                agentList.Add(each);
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", explore.StageID);
            sfs.PutInt("2", (int)explore.Type);
            sfs.PutBool("3", fastClear);

            var response = await Protocol.REQUEST_AGENT_EXPLORE_REWARD.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            var update = response.GetPacket<CharUpdateData>("cud");
            Notify(update);
            UI.Show<UIAgentExploreReward>(new UIAgentExploreReward.Input(explore.StageID, explore.Type, agentList.ToArray(), update.rewards));

            OnAgentExploreReward?.Invoke(explore);
        }

        /// <summary>
        /// 파견 포기 요청
        /// </summary>
        public async Task RequestAgentExploreCancel(AgentExploreState explore)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", explore.StageID);
            sfs.PutInt("2", (int)explore.Type);

            var response = await Protocol.REQUEST_AGENT_EXPLORE_CANCEL.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            var update = response.GetPacket<CharUpdateData>("cud");
            Notify(update);

            OnAgentExploreCancel?.Invoke();
        }

        /// <summary>
        /// 일일 초기화
        /// </summary>
        private void ResetTradeCounts()
        {
            int maxTradeCount = GetTradeProductionMaxCount();
            foreach (AgentExploreCountInfo each in exploreCountInfoList)
            {
                each.remainTradeCount = maxTradeCount;
                each.SetIsViewAd(false);
            }

            OnUpdateRemainTradeCount?.Invoke();
        }

        public async Task RequestAgentResetTradeCount(int stageID)
        {
            if (!IsTradeProuctionAd(stageID)) // 광고 초기화가 아닐경우
            {
                if (Entity.Goods.CatCoin < BasisType.AGENT_TRADE_RESET_PRICE.GetInt())
                {
                    UI.ShowToastPopup(LocalizeKey._413.ToText()); // 냥다래 열매가 부족합니다.
                    return;
                }
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", stageID);

            var response = await Protocol.REQUEST_AGENT_INIT_TRADE_COUNT.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                var update = response.GetPacket<CharUpdateData>("cud");
                Notify(update);
            }

            AgentExploreCountInfo countInfo = exploreCountInfoList.Find(v => v.StageID == stageID);
            countInfo.remainTradeCount = GetTradeProductionMaxCount();
            countInfo.SetIsViewAd(true);
            OnUpdateRemainTradeCount?.Invoke();
            OnResetTradeCount?.Invoke();
        }

        public void AddNewTradeCountInfo(int stageID)
        {
            AdventureData[] datas = AdventureDataManager.Instance.GetArrData();

            if (stageID == -1)
                return;

            if (exploreCountInfoList.Find(v => v.StageID == stageID) != null)
            {
                Debug.LogError($"[AgentModel] 이미 교역 Count 정보가 있는 Stage 입니다.");
                return;
            }

            exploreCountInfoList.Add(new AgentExploreCountInfo(stageID, GetTradeProductionMaxCount()));
        }

        /// <summary>
        /// 교역 & 생산 파견 남은 횟수
        /// </summary>
        public int GetTradeProductionRemainCount(int stageID)
        {
            AgentExploreCountInfo info = exploreCountInfoList.Find(v => v.StageID == stageID);
            if (info == null)
            {
                return GetTradeProductionMaxCount();
            }
            else
            {
                return info.remainTradeCount;
            }
        }

        /// <summary>
        /// 파견 인연 도감 보상 요청
        /// </summary>
        public async Task RequestAgentBookEnable(int agentBookID)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", agentBookID);

            var response = await Protocol.REQUEST_AGENT_BOOK_ENABLE.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            var update = response.GetPacket<CharUpdateData>("cud");
            Notify(update);

            var bookState = bookStateList.Find(v => v.BookData.id == agentBookID);
            bookState.SetEnabled();
            UI.RewardInfo(update.rewards);

            OnStatusReloadRequired?.Invoke();
            SendMessageNoticeUpdate();

            OnAgentBookEnable?.Invoke(agentBookID);
        }

        public void SendMessageNoticeUpdate()
        {
            OnMessageNoticeUpdate?.Invoke();
        }

        /// <summary>
        /// 전투동료 비어 있는 슬롯 존재 여부
        /// </summary>
        private bool HasEmptyEquipSlot()
        {
            for (int i = 0; i < slotStateList.Count; i++)
            {
                if (!slotStateList[i].IsUsingSlot)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 전투동료 장착하지 않은 전투동료 존재 여부
        /// </summary>
        private bool HasUnusingAgent()
        {
            for (int i = 0; i < combatAgentList.Count; ++i)
            {
                if (!combatAgentList[i].IsUsingAgent)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 파견 즉시완료 캣코인
        /// </summary>
        public int GetCatcoinFastClear(RemainTime remainTime)
        {
            var rt = remainTime.ToRemainTime();
            if (rt > 0f)
            {
                var timeSpan = rt.ToTimeSpan();
                var initMin = BasisType.EXPLORE_COMPLETE_INIT_TIME.GetInt() * 0.001f / 60f; // 초기화 시간 단위(분)
                return BasisType.EXPLORE_IMMEDIATLY_COMPLETE_CATCOIN.GetInt() * Mathf.CeilToInt((float)timeSpan.TotalMinutes / initMin); // 5분당 5냥다래
            }
            else
            {
                return 0; // 남은시간 없음..
            }
        }

        /// <summary>
        /// 교역 & 생산 일일 무료 최대 횟수
        /// </summary>
        public int GetTradeProductionMaxCount()
        {
            return BasisType.AGENT_TRADE_MAX_COUNT.GetInt();
        }

        /// <summary>
        /// 교역 & 생산 광고 여부
        /// </summary>
        public bool IsTradeProuctionAd(int stageID)
        {
            // 광고 기능 OFF
            if (BasisType.TRADE_PRODUCTION_AD_FLAG.GetInt() == 0)
                return false;

            AgentExploreCountInfo info = exploreCountInfoList.Find(v => v.StageID == stageID);

            if (info == null)
                return false;

            // 오늘 광고 봄
            if (info.IsViewAd)
                return false;             

            return true;
        }        
    }
}