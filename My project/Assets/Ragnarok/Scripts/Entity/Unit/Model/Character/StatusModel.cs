using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class StatusModel : CharacterEntityModel, StatusModel.IInputValue
    {
        public interface IInputValue
        {
            int Str { get; }
            int Agi { get; }
            int Vit { get; }
            int Int { get; }
            int Dex { get; }
            int Luk { get; }
            int StatPoint { get; }
        }

        private ObscuredInt basicStr; // 기본 힘
        private ObscuredInt basicAgi; // 기본 민첩 (어질)
        private ObscuredInt basicVit; // 기본 체력 (바이탈)
        private ObscuredInt basicInt; // 기본 지능 (인트)
        private ObscuredInt basicDex; // 기본 재주 (덱스)
        private ObscuredInt basicLuk; // 기본 운 (럭)
        private ObscuredInt statPoint; // 스탯 포인트
        /// <summary>
        /// 유닛 스탯 계산 시, 장착아이템 옵션 포함 예외 여부
        /// </summary>
        public bool IsExceptEquippedItems { get; private set; }
        private BattleOption[] serverBattleOptions; // 합산된 전옵타 - 길드 제외 (멀티 플레이어 전용)
        private BattleOption[] serverGuildBattleOptions; // 합산된 전옵타 - 길드 (멀티 플레이어 전용)

        private ObscuredByte autoStat; // 능력치 자동분배 여부

        public int BasicStr => basicStr + previewStr;
        public int BasicAgi => basicAgi + previewAgi;
        public int BasicVit => basicVit + previewVit;
        public int BasicInt => basicInt + previewInt;
        public int BasicDex => basicDex + previewDex;
        public int BasicLuk => basicLuk + previewLuk;
        public int StatPoint => statPoint;

        int IInputValue.Str => BasicStr;
        int IInputValue.Agi => BasicAgi;
        int IInputValue.Vit => BasicVit;
        int IInputValue.Int => BasicInt;
        int IInputValue.Dex => BasicDex;
        int IInputValue.Luk => BasicLuk;
        int IInputValue.StatPoint => statPoint;

        public int MaxStr { get; private set; } // 0~499
        public int MaxAgi { get; private set; }
        public int MaxVit { get; private set; }
        public int MaxInt { get; private set; }
        public int MaxDex { get; private set; }
        public int MaxLuk { get; private set; }
        private int maxStatus;

        private int previewStr;
        private int previewAgi;
        private int previewVit;
        private int previewInt;
        private int previewDex;
        private int previewLuk;

        /// <summary>
        /// 사용한 스탯과 남아있는 스탯 총합
        /// </summary>
        public int TotalStatusPoint => StatPoint + BasicStr + BasicAgi + BasicVit + BasicInt + BasicDex + BasicLuk;

        public bool IsAutoStat => autoStat != 0;

        /// <summary>
        /// 기본 스탯 변경 시 호출
        /// </summary>
        public event Action OnUpdateBasicStatus;

        /// <summary>
        /// 스탯 포인트 변경 시 호출
        /// </summary>
        public event Action OnUpdateStatPoint;

        /// <summary>
        /// 스탯 자동분배 옵션 변경
        /// </summary>
        public event Action OnAutoStat;

        /// <summary>
        /// 오버 스탯 결과
        /// </summary>
        public event Action<bool> OnOverStatusResult;

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        public override void ResetData()
        {
            base.ResetData();

            basicStr = 0;
            basicAgi = 0;
            basicVit = 0;
            basicInt = 0;
            basicDex = 0;
            basicLuk = 0;
            statPoint = 0;

            previewStr = 0;
            previewAgi = 0;
            previewVit = 0;
            previewInt = 0;
            previewDex = 0;
            previewLuk = 0;

            IsExceptEquippedItems = false;
            serverBattleOptions = null;
            serverGuildBattleOptions = null;
        }

        internal void Initialize(IInputValue inputValue)
        {
            basicStr = inputValue.Str;
            basicAgi = inputValue.Agi;
            basicVit = inputValue.Vit;
            basicInt = inputValue.Int;
            basicDex = inputValue.Dex;
            basicLuk = inputValue.Luk;
            statPoint = inputValue.StatPoint;
        }

        internal void Initialize(OverStatusPacket packet)
        {
            maxStatus = BasisType.MAX_STAT.GetInt(); // 500
            MaxStr = packet.str;
            MaxAgi = packet.agi;
            MaxVit = packet.vit;
            MaxInt = packet.@int;
            MaxDex = packet.dex;
            MaxLuk = packet.luk;

            Debug.Log($"{nameof(MaxStr)}={MaxStr}, {nameof(MaxAgi)}={MaxAgi}, {nameof(MaxVit)}={MaxVit}, {nameof(MaxInt)}={MaxInt}, {nameof(MaxDex)}={MaxDex}, {nameof(MaxLuk)}={MaxLuk}");
        }

        /// <summary>
        /// 멀티 플레이어 전용
        /// </summary>
        internal void Initialize(bool isExceptEquippedItems, IBattleOption[] serverBattleOptions, IBattleOption[] serverGuildBattleOptions)
        {
            Initialize(isExceptEquippedItems, ConvertToBattleOption(serverBattleOptions), ConvertToBattleOption(serverGuildBattleOptions));
        }

        /// <summary>
        /// 클론 플레이어 전용
        /// </summary>
        internal void Initialize(bool isExceptEquippedItems, BattleOption[] serverBattleOptions, BattleOption[] serverGuildBattleOptions)
        {
            IsExceptEquippedItems = isExceptEquippedItems;
            this.serverBattleOptions = serverBattleOptions;
            this.serverGuildBattleOptions = serverGuildBattleOptions;
        }

        internal void Initialize(byte autoStat)
        {
            this.autoStat = autoStat;
        }

        internal void Update(CharacterStatData statusData, short? statPoint)
        {
            // 스탯 정보
            if (statusData != null) // 값이 유효할 경우
                UpdateStat(statusData.str, statusData.agi, statusData.vit, statusData.inte, statusData.dex, statusData.lux);

            // 스탯 포인트
            if (this.statPoint.Replace(statPoint))
                OnUpdateStatPoint?.Invoke();
        }

        public void UpdateStat(short? str, short? agi, short? vit, short? @int, short? dex, short? lux)
        {
            bool isDirty = false;

            if (basicStr.Replace(str))
                isDirty = true;

            if (basicAgi.Replace(agi))
                isDirty = true;

            if (basicVit.Replace(vit))
                isDirty = true;

            if (basicInt.Replace(@int))
                isDirty = true;

            if (basicDex.Replace(dex))
                isDirty = true;

            if (basicLuk.Replace(lux))
                isDirty = true;

            if (isDirty)
                OnUpdateBasicStatus?.Invoke();
        }

        /// <summary>
        /// 추가 서버 전옵타 - 길드 제외 (멀티 플레이어 전용)
        /// </summary>
        public BattleOption[] GetServerBattleOptions()
        {
            return serverBattleOptions;
        }

        /// <summary>
        /// 추가 서버 전옵타 - 길드 (멀티 플레이어 전용)
        /// </summary>
        public BattleOption[] GetServerGuildBattleOptions()
        {
            return serverGuildBattleOptions;
        }

        /// <summary>
        /// 캐릭터 스탯포인트 업데이트
        /// </summary>
        public async Task<bool> RequestCharStatPointUpdate(short str, short agi, short vit, short inte, short dex, short lux, bool isAsk = true, bool isAuto = false)
        {
            if (isAsk)
            {
                string title = LocalizeKey._5.ToText(); // 알람
                string description = LocalizeKey._90042.ToText(); // 지금 능력치로 적용하시겠습니까?
                if (!await UI.SelectPopup(title, description))
                    return false;
            }

            var obj = Protocol.NewInstance();
            if (str != 0)
                obj.PutShort("1", str);

            if (agi != 0)
                obj.PutShort("2", agi);

            if (vit != 0)
                obj.PutShort("3", vit);

            if (inte != 0)
                obj.PutShort("4", inte);

            if (dex != 0)
                obj.PutShort("5", dex);

            if (lux != 0)
                obj.PutShort("6", lux);

            var sfs = Protocol.NewInstance();
            sfs.PutSFSObject("1", obj);
            sfs.PutBool("2", isAuto);

            Response response = await Protocol.CHAR_STAT_POINT_UPDATE.SendAsync(sfs);

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

            return response.isSuccess;
        }

        /// <summary>
        /// 캐릭터 스탯 포인트 초기화
        /// </summary>
        public async Task<bool> RequestCharStatPointInit()
        {
            string title = LocalizeKey._90003.ToText(); // 능력치 초기화
            string description = LocalizeKey._90046.ToText(); // 능력치를 초기화 하시겠습니까?
            if (!await UI.CostPopup(CoinType.CatCoin, BasisType.PRICE_STAT_INIT.GetInt(), title, description))
                return false;

            Response response = await Protocol.CHAR_STAT_POINT_INIT.SendAsync();
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }
                autoStat = 0; // 자동분배 OFF
                OnAutoStat?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }

            return true;
        }

        /// <summary>
        /// 현재 스탯 포인트로 자동 분배했을 때의 스탯 배분량 구하기
        /// </summary>
        /// <returns> short[6] { Str Agi Vit Int Dex Luk }</returns>
        public short[] GetAutoStatGuidePoints(int remainPoint)
        {
            JobData jobData = Entity.Character.GetJobData();
            JobData.StatValue basicStat = new JobData.StatValue(BasicStr, BasicAgi, BasicVit, BasicInt, BasicDex, BasicLuk);
            JobData.StatValue maxStat = new JobData.StatValue(MaxStatus(BasicStatusType.Str), MaxStatus(BasicStatusType.Agi), MaxStatus(BasicStatusType.Vit), MaxStatus(BasicStatusType.Int), MaxStatus(BasicStatusType.Dex), MaxStatus(BasicStatusType.Luk));
            return jobData.GetAutoStatGuidePoints(remainPoint, basicStat, maxStat);
        }

        public void AddStatusStr(short point)
        {
            previewStr += point;
        }

        public void AddStatusAgi(short point)
        {
            previewAgi += point;
        }

        public void AddStatusVit(short point)
        {
            previewVit += point;
        }

        public void AddStatusInt(short point)
        {
            previewInt += point;
        }

        public void AddStatusDex(short point)
        {
            previewDex += point;
        }

        public void AddStatusLuk(short point)
        {
            previewLuk += point;
        }

        /// <summary>
        /// 자동분배 옵션
        /// </summary>
        /// <param name="isAutoStat"></param>
        /// <returns></returns>
        public async Task RequestAutoStat(bool isAutoStat)
        {
            if (IsAutoStat == isAutoStat)
                return;

            var sfs = Protocol.NewInstance();
            byte autoStat = isAutoStat ? (byte)1 : (byte)0;
            sfs.PutByte("1", autoStat);

            Response response = await Protocol.REQUEST_AUTO_STAT.SendAsync(sfs);
            if (response.isSuccess)
            {
                this.autoStat = autoStat;
                OnAutoStat?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 오버 스탯 요청
        /// </summary>
        public async Task RequestOverStatus(BasicStatusType statusType)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", statusType.ToIntValue());

            Response response = await Protocol.REQUEST_OVER_STAT.SendAsync(sfs);
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

            bool isSuccess = response.GetInt("1") == 1; // 0: 실패, 1: 성공

            if (isSuccess)
            {
                MaxStatusAdd(statusType);
                UI.Show<UIOverStatusResult>().Set(UIOverStatusResult.ResultType.Success);
            }
            else
            {
                UI.Show<UIOverStatusResult>().Set(UIOverStatusResult.ResultType.Failed);
            }
            OnOverStatusResult?.Invoke(isSuccess);
        }

        public void MaxStatusAdd(BasicStatusType type)
        {
            int max = BasisType.OVER_STATUS_MAX.GetInt();
            switch (type)
            {
                case BasicStatusType.Str:
                    MaxStr = Math.Min(max, MaxStr + 1);
                    break;

                case BasicStatusType.Agi:
                    MaxAgi = Math.Min(max, MaxAgi + 1);
                    break;

                case BasicStatusType.Vit:
                    MaxVit = Math.Min(max, MaxVit + 1);
                    break;

                case BasicStatusType.Int:
                    MaxInt = Math.Min(max, MaxInt + 1);
                    break;

                case BasicStatusType.Dex:
                    MaxDex = Math.Min(max, MaxDex + 1);
                    break;

                case BasicStatusType.Luk:
                    MaxLuk = Math.Min(max, MaxLuk + 1);
                    break;
            }
        }

        public int MaxStatus()
        {
            return maxStatus;
        }

        public int MaxStatus(BasicStatusType type)
        {
            switch (type)
            {
                case BasicStatusType.Str:
                    return maxStatus + MaxStr;

                case BasicStatusType.Agi:
                    return maxStatus + MaxAgi;

                case BasicStatusType.Vit:
                    return maxStatus + MaxVit;

                case BasicStatusType.Int:
                    return maxStatus + MaxInt;

                case BasicStatusType.Dex:
                    return maxStatus + MaxDex;

                case BasicStatusType.Luk:
                    return maxStatus + MaxLuk;
            }
            return maxStatus;
        }

        public int MaxStatusPlusCount(BasicStatusType type)
        {
            switch (type)
            {
                case BasicStatusType.Str:
                    return MaxStr;

                case BasicStatusType.Agi:
                    return MaxAgi;

                case BasicStatusType.Vit:
                    return MaxVit;

                case BasicStatusType.Int:
                    return MaxInt;

                case BasicStatusType.Dex:
                    return MaxDex;

                case BasicStatusType.Luk:
                    return MaxLuk;
            }
            return default;
        }

        private BattleOption[] ConvertToBattleOption(IBattleOption[] inputs)
        {
            if (inputs == null)
                return null;

            return Array.ConvertAll(inputs, ConvertToBattleOption);
        }

        private BattleOption ConvertToBattleOption(IBattleOption input)
        {
            return new BattleOption(input.BattleOptionType, input.Value1, input.Value2);
        }
    }
}