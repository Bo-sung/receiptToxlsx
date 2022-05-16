using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 재화 정보
    /// </summary>
    public class GoodsModel : CharacterEntityModel
    {
        private ObscuredInt catCoin; // 냥다래
        private ObscuredLong zeny; // 제니
        private ObscuredInt roPoint; // RoPoint
        private ObscuredInt guildCoin; // 길드코인
        private ObscuredInt normalQuestCoin; // 의뢰 퀘스트 코인
        private ObscuredInt onBuffPoint; // 온버프 포인트

        Action<long> onUpdateCatCoin;
        Action<long> onUpdateZeny;
        Action<long> onUpdateRoPoint;
        Action<long> onUpdateGuildCoin;

        public long CatCoin => catCoin;
        public long Zeny => zeny;
        public long RoPoint => roPoint;
        public long GuildCoin => guildCoin;
        public long NormalQuestCoin => normalQuestCoin;

        /// <summary>
        /// 온버프 포인트
        /// </summary>
        public int OnBuffPoint => onBuffPoint;

        /// <summary>
        /// 캣코인 변경 시 호출 (등록과 동시에 호출)
        /// </summary>
        public event Action<long> OnUpdateCatCoin
        {
            add
            {
                onUpdateCatCoin += value;
                value(CatCoin); // 등록과 동시에 호출
            }
            remove { onUpdateCatCoin -= value; }
        }

        /// <summary>
        /// 제니 변경 시 호출 (등록과 동시에 호출)
        /// </summary>
        public event Action<long> OnUpdateZeny
        {
            add
            {
                onUpdateZeny += value;
                value(Zeny); // 등록과 동시에 호출
            }
            remove { onUpdateZeny -= value; }
        }

        /// <summary>
        /// RopPoint 변경 시 호출 (등록과 동시에 호출)
        /// </summary>
        public event Action<long> OnUpdateRoPoint /// TODO: 필요한 곳에 이벤트 등록하기 (거래소)
        {
            add
            {
                onUpdateRoPoint += value;
                value(RoPoint); // 등록과 동시에 호출
            }
            remove { onUpdateRoPoint -= value; }
        }

        /// <summary>
        /// GuildCoin 변경 시 호출 (등록과 동시에 호출)
        /// </summary>
        public event Action<long> OnUpdateGuildCoin
        {
            add
            {
                onUpdateGuildCoin += value;
                value(GuildCoin); // 등록과 동시에 호출
            }
            remove { onUpdateGuildCoin -= value; }
        }

        public event Action<int> OnUpdateNormalQuestCoin;

        /// <summary>
        /// 온버프 포인트 수량 변경시 호출
        /// </summary>
        public event Action OnUpdateOnBuffPoint;

        /// <summary>
        /// 온버프 포인트 수량 동기화 완료시 호출
        /// </summary>
        public event Action OnUpdateOnBuffMyPoint;

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        public override void ResetData()
        {
            base.ResetData();

            onBuffPoint = 0;
        }

        internal void SetCatCoin(int catCoin)
        {
            this.catCoin = catCoin;
        }

        internal void SetZeny(long zeny)
        {
            this.zeny = zeny;
        }

        internal void SetRoPoint(int roPoint)
        {
            this.roPoint = roPoint;
        }

        internal void SetGuildCoin(int guildCoin)
        {
            this.guildCoin = guildCoin;
        }

        internal void SetNormalQuestCoin(int normalQuestCoin)
        {
            this.normalQuestCoin = normalQuestCoin;
        }

        internal void Update(long? zeny, int? catCoin, int? changedZeny, int? guildCoin, int? normalQuestCoin, int? roPoint, int? onBuffPoint)
        {
            if (changedZeny.HasValue)
                ChangedZeny(changedZeny.Value);

            if (zeny.HasValue)
                UpdateZeny(zeny.Value);

            if (catCoin.HasValue)
                UpdateCatCoin(catCoin.Value);

            if (guildCoin.HasValue)
                UpdateGuildCoin(guildCoin.Value);

            if (normalQuestCoin.HasValue)
                UpdateNormalQuestCoin(normalQuestCoin.Value);

            if (roPoint.HasValue)
                UpdateRoPoint(roPoint.Value);

            if (onBuffPoint.HasValue)
                UpdateOnBuffPoint(onBuffPoint.Value);
        }

        /// <summary>
        /// 온버프 포인트 업데이트
        /// </summary>
        internal void UpdateOnBuffPoint(int value)
        {
            int oldValue = OnBuffPoint;
            if (oldValue == value)
                return;

#if UNITY_EDITOR
            Debug.Log($"OnBuffPoint : {oldValue} => {value} ({value - oldValue})");
#endif

            onBuffPoint = value; // Set

            OnUpdateOnBuffPoint?.Invoke();
        }

        /// <summary>
        /// 제니 변화량
        /// </summary>
        /// <param name="value"></param>
        internal void ChangedZeny(int value)
        {
            int changedValue = Math.Abs(value);
            if (value > 0)
            {
#if UNITY_EDITOR
                if (DebugUtils.IsLogZeny)
                    Debug.Log($"획득한 제니 = {changedValue}");
#endif
                Quest.QuestProgress(QuestType.ZENY_GAIN, questValue: changedValue); // 제니 획득
            }
            else
            {
#if UNITY_EDITOR
                if (DebugUtils.IsLogZeny)
                    Debug.Log($"사용한 제니 = {changedValue}");
#endif
                Quest.QuestProgress(QuestType.ZENY_USE, questValue: changedValue); // 제니 소모
            }
        }

        /// <summary>
        /// [멀티미로] 멀티미로 제니 획득
        /// </summary>
        public async Task RequestMultiMazeGetZeny()
        {
            var response = await Protocol.REQUEST_MULMAZE_GET_ZENY.SendAsync();
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        private void UpdateZeny(long value)
        {
            long oldValue = zeny;

            if (oldValue == value)
                return;

#if UNITY_EDITOR
            if (DebugUtils.IsLogZeny)
                Debug.Log($"총제니 = {value}");
#endif

            zeny = value; // Set

            onUpdateZeny?.Invoke(value);
        }

        private void UpdateCatCoin(int value)
        {
            int oldValue = catCoin;
            if (oldValue == value)
                return;

            int changedValue = Math.Abs(value - oldValue);
            if (oldValue < value)
            {
                Debug.Log($"획득한 냥다래 = {changedValue}, 총냥다래 = {value}");
            }
            else
            {
                Debug.Log($"사용한 냥다래 = {changedValue}, 총냥다래 = {value}");
            }

            catCoin = value; // Set

            onUpdateCatCoin?.Invoke(value);
        }

        private void UpdateRoPoint(int value)
        {
            int oldValue = roPoint;
            if (oldValue == value)
                return;

            int changedValue = Math.Abs(value - oldValue);
            if (oldValue < value)
            {
                Debug.Log($"획득한 RoPoint = {changedValue}, 총RoPoint = {value}");
            }
            else
            {
                Debug.Log($"사용한 RoPoint = {changedValue}, 총RoPoint = {value}");
            }

            roPoint = value; // Set

            onUpdateRoPoint?.Invoke(value);
        }

        private void UpdateGuildCoin(int value)
        {
            int oldValue = guildCoin;
            if (oldValue == value)
                return;

            int changedValue = Math.Abs(value - oldValue);
            if (oldValue < value)
            {
                Debug.Log($"획득한 GuildCoin = {changedValue}, 총GuildCoin = {value}");
            }
            else
            {
                Debug.Log($"사용한 GuildCoin = {changedValue}, 총GuildCoin = {value}");
            }

            guildCoin = value; // Set

            onUpdateGuildCoin?.Invoke(value);
        }

        private void UpdateNormalQuestCoin(int value)
        {
            int oldValue = normalQuestCoin;
            if (oldValue == value)
                return;

            int changedValue = Math.Abs(value - oldValue);
            if (oldValue < value)
            {
                Debug.Log($"획득한 NormalQuestCoin = {changedValue}, 총NormalQuestCoin = {value}");
            }
            else
            {
                Debug.Log($"사용한 NormalQuestCoin = {changedValue}, 총NormalQuestCoin = {value}");
            }

            normalQuestCoin = value; // Set

            OnUpdateNormalQuestCoin?.Invoke(value);
        }

        /// <summary>
        /// 온버프 내 포인트 조회(동기화)
        /// </summary>
        public async Task RequestOnBuffMyPoint()
        {
            var response = await Protocol.REQUEST_ONBUFF_MY_POINT_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            UpdateOnBuffPoint(response.GetInt("1"));

            OnUpdateOnBuffMyPoint?.Invoke();
        }
    }
}