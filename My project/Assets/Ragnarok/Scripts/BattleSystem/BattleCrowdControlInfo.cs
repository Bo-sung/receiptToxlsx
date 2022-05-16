using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleCrowdControlInfo : IEqualityComparer<CrowdControlType>, BattleCrowdControlInfo.ITargetValue, BattleCrowdControlInfo.IAttackerValue
    {
        public interface IValue
        {
            int[] GetCrowdControlIds();
        }

        public interface ITargetValue : IValue
        {
            bool CannotFlee { get; }

            int DefDecreaseRate { get; }
            int MdefDecreaseRate { get; }
        }

        public interface IAttackerValue : IValue
        {
            int TotalDmgDecreaseRate { get; }
            int CriRateDecreaseRate { get; }
        }

        public delegate void CrowdControlEvent(CrowdControlType type, bool isGroggy, int overapCount);
        public delegate void DotDamageEvent(CrowdControlType type, float duration, int dotDamageRate);

        public struct ApplySettings
        {
            public bool isStun, isSilence, isSleep, isHallucination, isBleeding, isBurning, isPoison, isCurse, isFreezing, isFrozen; // 상태이상 여부
        }

        public struct TargetValue
        {
            public int stunCount, silenceCount, sleepCount, hallucinationCount, bleedingCount, burningCount, poisonCount, curseCount, freezingCount, frozenCount; // 상태이상 중첩 수
        }

        private readonly CrowdControlDataManager crowdControlDataRepo;
        private readonly List<CrowdControlInfo> crowdControlList;
        private readonly Dictionary<CrowdControlType, ObscuredInt> crowdControlCountDic;

        public bool CannotFlee => GetCannotFlee();
        public int DefDecreaseRate => GetDefDecreaseRate();
        public int MdefDecreaseRate => GetMdefDecreaseRate();
        public int TotalDmgDecreaseRate => GetTotalDmgDecreaseRate();
        public int CriRateDecreaseRate => GetCriRateDecreaseRate();
        public int MoveSpdDecreaseRate => GetMoveSpdDecreaseRate();
        public int AtkSpdDecreaseRate => GetAtkSpdDecreaseRate();

        public event CrowdControlEvent OnCrowdControl; // 특정 상태이상 오버랩 이벤트
        public event DotDamageEvent OnAddDotDamage; // 도트 대미지 이벤트

        private bool isForceCannotFlee;

        public BattleCrowdControlInfo()
        {
            crowdControlDataRepo = CrowdControlDataManager.Instance;
            crowdControlList = new List<CrowdControlInfo>();
            crowdControlCountDic = new Dictionary<CrowdControlType, ObscuredInt>(this);
            foreach (CrowdControlType type in System.Enum.GetValues(typeof(CrowdControlType)))
            {
                crowdControlCountDic.Add(type, 0);
            }
        }

        public void Clear()
        {
            for (int i = crowdControlList.Count - 1; i >= 0; i--)
            {
                Remove(crowdControlList[i]);
            }

            isForceCannotFlee = false;
        }

        /// <summary>
        /// 유효성 체크
        /// </summary>
        public bool CheckDurationEffect()
        {
            if (crowdControlList.Count == 0)
                return false;

            bool isDirty = false;

            for (int i = crowdControlList.Count - 1; i >= 0; i--)
            {
                if (crowdControlList[i].IsValid())
                    continue;

                isDirty = true;
                Remove(crowdControlList[i]);
            }

            return isDirty;
        }

        /// <summary>
        /// 이동 불가능 여부
        /// </summary>
        public bool GetCannotMove()
        {
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                if (crowdControlList[i].CannotMove)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 평타 스킬 사용 불가능 여부
        /// </summary>
        public bool GetCannotUseBasicAttack()
        {
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                if (crowdControlList[i].CannotUseBasicAttack)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 스킬 사용 불가능 여부
        /// </summary>
        public bool GetCannotUseSkill()
        {
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                if (crowdControlList[i].CannotUseSkill)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 현재 상태이상 Id 배열
        /// </summary>
        public int[] GetCrowdControlIds()
        {
            int[] output = new int[crowdControlList.Count];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = crowdControlList[i].CrowdControlId;
            }

            return output;
        }

        /// <summary>
        /// 회피 불가능 여부
        /// </summary>
        private bool GetCannotFlee()
        {
            if (isForceCannotFlee)
                return true;

            for (int i = 0; i < crowdControlList.Count; i++)
            {
                if (crowdControlList[i].CannotFlee)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 물리방어력 감소율
        /// </summary>
        private int GetDefDecreaseRate()
        {
            int rate = 0;
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                rate += crowdControlList[i].DefDecreaseRate;
            }

            return rate;
        }

        /// <summary>
        /// 마법방어력 감소율
        /// </summary>
        private int GetMdefDecreaseRate()
        {
            int rate = 0;
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                rate += crowdControlList[i].MdefDecreaseRate;
            }

            return rate;
        }

        /// <summary>
        /// 전체대미지 감소율
        /// </summary>
        private int GetTotalDmgDecreaseRate()
        {
            int rate = 0;
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                rate += crowdControlList[i].TotalDmgDecreaseRate;
            }

            return rate;
        }

        /// <summary>
        /// 크리티컬 확률 감소율
        /// </summary>
        private int GetCriRateDecreaseRate()
        {
            int rate = 0;
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                rate += crowdControlList[i].CriRateDecreaseRate;
            }

            return rate;
        }

        /// <summary>
        /// 이동속도 감소율
        /// </summary>
        private int GetMoveSpdDecreaseRate()
        {
            int rate = 0;
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                rate += crowdControlList[i].MoveSpdDecreaseRate;
            }

            return rate;
        }

        /// <summary>
        /// 공격속도 감소율
        /// </summary>
        private int GetAtkSpdDecreaseRate()
        {
            int rate = 0;
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                rate += crowdControlList[i].AtkSpdDecreaseRate;
            }

            return rate;
        }

        /// <summary>
        /// 상태이상 처리
        /// </summary>
        public void Apply(ApplySettings settings)
        {
            if (settings.isStun)
                Apply(CrowdControlType.Stun);

            if (settings.isSilence)
                Apply(CrowdControlType.Silence);

            if (settings.isSleep)
                Apply(CrowdControlType.Sleep);

            if (settings.isHallucination)
                Apply(CrowdControlType.Hallucination);

            if (settings.isBleeding)
                Apply(CrowdControlType.Bleeding);

            if (settings.isBurning)
                Apply(CrowdControlType.Burning);

            if (settings.isPoison)
                Apply(CrowdControlType.Poison);

            if (settings.isCurse)
                Apply(CrowdControlType.Curse);

            if (settings.isFreezing)
                Apply(CrowdControlType.Freezing);

            if (settings.isFrozen)
                Apply(CrowdControlType.Frozen);
        }

        /// <summary>
        /// 상태이상 적용
        /// </summary>
        public void Apply(CrowdControlType type)
        {
            if (type == default)
                return;

            int curOverlapCount = GetCurrentOverlapCount(type); // 현재 중첩 수
            int maxOverlapCount = GetMaxOverlapCount(type); // 최대 중첩 가능 수

            if (maxOverlapCount == 0 || curOverlapCount < maxOverlapCount)
            {
                Add(type);
            }
            else
            {
                Overap(type);
            }
        }

        /// <summary>
        /// 상태이상 추가
        /// </summary>
        private void Add(CrowdControlType type)
        {
            CrowdControlInfo info = Create(type);

            Plus(info);
            OnCrowdControl?.Invoke(info.Type, info.IsGroggy, crowdControlCountDic[info.Type]);

            if (info.DotDamageRate > 0 && !BattleManager.isIndependenceDamage) // 화상 등의 대미지가 서버처리될 경우 클라이언트 자체의 도트대미지 무시
                OnAddDotDamage?.Invoke(type, info.Duration, info.DotDamageRate);
        }

        /// <summary>
        /// 상태이상 중첩
        /// </summary>
        private void Overap(CrowdControlType type)
        {
            CrowdControlInfo info = Find(type);

            Minus(info);
            Plus(info);
        }

        /// <summary>
        /// 상태이상 제거
        /// </summary>
        private void Remove(CrowdControlInfo info)
        {
            Minus(info);
            OnCrowdControl?.Invoke(info.Type, info.IsGroggy, crowdControlCountDic[info.Type]);
        }

        private void Plus(CrowdControlInfo info)
        {
            if (info == null)
                return;

            info.Initialize(); // 초기화

            ++crowdControlCountDic[info.Type];
            crowdControlList.Add(info);
        }

        private void Minus(CrowdControlInfo info)
        {
            if (info == null)
                return;

            --crowdControlCountDic[info.Type];
            crowdControlList.Remove(info);
        }

        /// <summary>
        /// 현재 진행중인 상태이상 중 앞단의 정보 반환
        /// </summary>
        private CrowdControlInfo Find(CrowdControlType type)
        {
            for (int i = 0; i < crowdControlList.Count; i++)
            {
                if (crowdControlList[i].Type == type)
                    return crowdControlList[i];
            }

            return default;
        }

        /// <summary>
        /// 현재 중첩 수
        /// </summary>
        private int GetCurrentOverlapCount(CrowdControlType type)
        {
            if (crowdControlCountDic.ContainsKey(type))
                return crowdControlCountDic[type];

            return 0;
        }

        /// <summary>
        /// 최대 중첩 가능 수
        /// </summary>
        private int GetMaxOverlapCount(CrowdControlType type)
        {
            return crowdControlDataRepo.GetOverlapCount(type);
        }

        private CrowdControlInfo Create(CrowdControlType type)
        {
            CrowdControlData data = crowdControlDataRepo.Get(type);

            if (data == null)
                return null;

            CrowdControlInfo info = new CrowdControlInfo();
            info.SetData(data);

            return info;
        }

        public void MakeWeak()
        {
            isForceCannotFlee = true;
        }

        bool IEqualityComparer<CrowdControlType>.Equals(CrowdControlType x, CrowdControlType y)
        {
            return x == y;
        }

        int IEqualityComparer<CrowdControlType>.GetHashCode(CrowdControlType obj)
        {
            return obj.GetHashCode();
        }
    }
}