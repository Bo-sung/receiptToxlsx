using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 큐펫 목록 정보
    /// </summary>
    public class CupetListModel : CharacterEntityModel, CupetListModel.ICupetListModelImpl
    {
        public interface ICupetListModelImpl
        {
            CupetEntity Get(int cupetId);
        }

        public interface IInputValue
        {
            int Cid { get; }
            int CupetId { get; }
            int CupetRank { get; }
            int CupetLevel { get; }
            int CupetExp { get; }
            int Count { get; }
        }

        private readonly CupetDataManager cupetDataRepo;
        private readonly ItemDataManager itemDataRepo;

        private int cachedRankSum = default;

        /// <summary>
        /// 큐펫 보유 목록 (미보유 포함) (key: cupetID)
        /// 큐펫은 중복 보유가 불가능하므로, id 를 key값으로 사용하여도 문제 없음
        /// </summary>
        private readonly Dictionary<ObscuredInt, CupetEntity> cupetDic;

        /// <summary>
        /// 큐펫 소환
        /// </summary>
        public event System.Action OnSummon;

        /// <summary>
        /// 큐펫 진화
        /// </summary>
        public event System.Action OnEvolution;

        /// <summary>
        /// 큐펫 스탯 리로드 이벤트
        /// </summary>
        public event System.Action OnReloadCupetStatus;

        /// <summary>
        /// 큐펫 목록 정보 업데이트
        /// </summary>
        public event System.Action OnUpdateCupetList;

        public CupetListModel()
        {
            cupetDataRepo = CupetDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            cupetDic = new Dictionary<ObscuredInt, CupetEntity>(ObscuredIntEqualityComparer.Default);
        }

        public override void AddEvent(UnitEntityType type)
        {
            OnSummon += UpdateRankSum;
            OnEvolution += UpdateRankSum;
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            OnSummon -= UpdateRankSum;
            OnEvolution -= UpdateRankSum;
        }

        public override void ResetData()
        {
            base.ResetData();
            ResetCupet();
        }

        internal void ResetCupet()
        {
            cupetDic.Clear();
            cachedRankSum = 0;
        }

        internal void Initialize(IInputValue[] cupets)
        {
            Initialize(); // 데이터 초기화

            if (cupets != null)
            {
                foreach (var item in cupets)
                {
                    UpdateCupet(item);
                }
            }

            Reload(); // 다시 로드
            UpdateRankSum(); // cachedRankSum 초기화
        }

        public void UpdateData(GuildCupetPacket[] inputCupets)
        {
            if (inputCupets == null)
                return;

            foreach (var item in inputCupets)
            {
                UpdateCupet(item);
            }

            Reload();
        }

        public void SetState(UnitEntity.UnitState state)
        {
            foreach (var item in cupetDic.Values)
            {
                item.SetState(state);
            }
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        private void Initialize()
        {
            cupetDic.Clear();

            int cid = Entity.Character.Cid;
            bool isPlayer = (Entity.type == UnitEntityType.Player);
            foreach (var cupetID in cupetDataRepo.GetCupetIDs())
            {
                CupetEntity cupetEntity;

                if (isPlayer)
                {
                    cupetEntity = CupetEntity.Factory.CreatePlayerCupet(cupetID);
                }
                else
                {
                    cupetEntity = CupetEntity.Factory.CreateMultiBattleCupet(cupetID);
                }

                cupetDic.Add(cupetID, cupetEntity);
            }
        }

        /// <summary>
        /// 큐펫 정보 반환 (key: cupetID)
        /// </summary>
        public CupetEntity Get(int cupetId)
        {
            if (!cupetDic.ContainsKey(cupetId))
            {
                Debug.LogError($"큐펫 데이터가 존재하지 않습니다: {nameof(cupetId)} = {cupetId.ToString()}");
                return null;
            }

            return cupetDic[cupetId];
        }

        /// <summary>
        /// 큐펫 정보 목록 반환 (보유하지 않은 큐펫 정보 포함)
        /// </summary>
        public CupetEntity[] GetArray()
        {
            return cupetDic.Values.ToArray();
        }

        /// <summary>
        /// 보유 큐펫들의 랭크 총합을 반환
        /// </summary>
        public int GetHaveCupetRankSum()
        {
            return cachedRankSum;
        }

        /// <summary>
        /// 큐펫 랭크 총합 업데이트 (매번 계산하지 않기 위해)
        /// </summary>
        private void UpdateRankSum()
        {
            if (Entity.type != UnitEntityType.Player)
                return;

            CupetEntity[] cupetList = GetArray();
            int rankSum = 0;
            foreach (CupetEntity cupet in cupetList)
            {
                CupetModel cupetModel = cupet.Cupet;
                if (cupetModel.IsInPossession)
                {
                    rankSum += cupetModel.Rank;
                }
            }
            this.cachedRankSum = rankSum;
        }

        /// <summary>
        /// 큐펫 업데이트
        /// </summary>
        private void UpdateCupet(IInputValue input)
        {
            CupetEntity cupetEntity = Get(input.CupetId);

            if (cupetEntity == null)
            {
                Debug.LogError($"[큐펫 업데이트] 데이터가 존재하지 않습니다: {nameof(input.CupetId)} = {input.CupetId}, {nameof(input.CupetRank)} = {input.CupetRank}");
                return;
            }

            cupetEntity.Cupet.Initialize(input.CupetId, input.CupetRank, input.CupetExp, input.Count);
        }

        /// <summary>
        /// 다시 로드
        /// </summary>
        private void Reload()
        {
            OnReloadCupetStatus?.Invoke();
        }

        #region 프로토콜

        /// <summary>
        /// 큐펫 소환
        /// </summary>
        public async Task RequestSummonCupet(int cupetId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", cupetId);

            var response = await Protocol.GUILD_CUPET_RANK_UP.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            OnSummon?.Invoke();
            UI.Show<UIRewardCupet>().Set(Get(cupetId).Cupet);
        }

        /// <summary>
        /// 큐펫 진화
        /// </summary>
        public async Task RequestCupetEvolution(int cupetId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", cupetId);

            var response = await Protocol.GUILD_CUPET_RANK_UP.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                OnEvolution?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드큐펫 정보 요청
        /// </summary>
        public async Task RequestGuildCupetInfo()
        {
            var response = await Protocol.GUILD_CUPET_LIST.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
            {
                UpdateData(response.GetPacketArray<GuildCupetPacket>("1"));
            }

            OnUpdateCupetList?.Invoke();
        }

        /// <summary>
        /// 큐펫 경험치 업
        /// </summary>
        public async Task RequestCupetExpUp(int cupetId, (int id, int count)[] items)
        {
            var sfs = Protocol.NewInstance();
            var sfsArray = Protocol.NewArrayInstance();
            sfs.PutInt("1", cupetId);
            foreach (var item in items)
            {
                var element = Protocol.NewInstance();
                element.PutInt("1", item.id);
                element.PutInt("2", item.count);
                sfsArray.AddSFSObject(element);

                ItemData itemData = itemDataRepo.Get(item.id);
                if (itemData == null)
                    continue;
            }
            sfs.PutSFSArray("2", sfsArray);

            var response = await Protocol.REQUEST_GUILD_CUPET_EXP_UP.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            if (response.ContainsKey("1"))
            {
                UpdateData(response.GetPacketArray<GuildCupetPacket>("1"));
            }

            OnUpdateCupetList?.Invoke();
        }

        #endregion
    }
}