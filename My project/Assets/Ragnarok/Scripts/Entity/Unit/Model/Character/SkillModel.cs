using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 스킬 정보
    /// </summary>
    public class SkillModel : CharacterEntityModel, SkillModel.ISkillModelImpl
    {
        /// <summary>
        /// 스킬사용과 동시에 슬롯변경에 대한 대응 (스킬 no)
        /// </summary>
        public const string SKILL_COOLDOWN_CHECK_SKILL_NO_KEY = "n";

        /// <summary>
        /// 스킬사용과 동시에 슬롯변경에 대한 대응 (참조 스킬 id)
        /// </summary>
        public const string SKILL_COOLDOWN_CHECK_REF_SKILL_ID_KEY = "i";

        /// <summary>
        /// 스킬사용과 동시에 슬롯변경에 대한 대응 (참조 스킬 쿨타임 시간)
        /// </summary>
        public const string SKILL_COOLDOWN_CHECK_REF_SKILL_COOLDOWN_TIME_KEY = "t";

        public interface ISkillValue
        {
            bool IsInPossession { get; }
            long SkillNo { get; }
            int SkillId { get; }
            int SkillLevel { get; }
            int OrderId { get; }
            int ChangeSkillId { get; }
        }

        public interface ISlotValue
        {
            long SlotNo { get; }
            long SkillNo { get; }
            int SlotIndex { get; }
            bool IsAutoSkill { get; }
        }

        public interface ISkillModelImpl
        {
            /// <summary>
            /// 스킬 아이디에 해당하는 스킬 레벨 반환
            /// </summary>
            int GetSkillLevel(int skillId);

            /// <summary>
            /// 스킬 슬롯 개수
            /// </summary>
            int SkillSlotCount { get; }

            /// <summary>
            /// 스킬 슬롯에 해당하는 스킬 아이디
            /// </summary>
            int GetSkillId(int slotIndex);

            /// <summary>
            /// 장착 가능한 스킬슬롯 여부
            /// </summary>
            /// <returns></returns>
            bool HasEquipSkillSlot();

            /// <summary>
            /// 전투 스킬Id (초월로 인하여 변경될 수 있음)
            /// </summary>
            int GetBattleSkillId(int skillId);
        }

        public interface ISkillSimpleValue : IInfo
        {
            int SkillId { get; }
            int SkillLevel { get; }
            SkillType SkillType { get; }

            string GetIconName();
        }

        private readonly SkillDataManager skillDataRepo;

        /// <summary>
        /// 스킬 정보
        /// </summary>
        public readonly List<SkillInfo> skillList;

        /// <summary>
        /// 스킬 정보
        /// Key: no, Value: SkillInfo
        /// </summary>
        private readonly Dictionary<ObscuredLong, SkillInfo> skillDic;

        /// <summary>
        /// 스킬 슬롯 정보
        /// </summary>
        public readonly List<ISlotValue> skillSlotList;

        /// <summary>
        /// 변경된 스킬 정보
        /// Key: originalSkillId, Value: changedSkillId
        /// </summary>
        private readonly Dictionary<ObscuredInt, ObscuredInt> changedSkillDic;

        /// <summary>
        /// 스킬 버퍼
        /// </summary>
        private readonly Buffer<SkillInfo> skillBuffer;

        /// <summary>
        /// 장착한 스킬 저장 정보 (이전과 다를 경우에 쿨타임 돌려주기 위함)
        /// </summary>
        private readonly HashSet<int> savedEquipedSkillHashSet;

        private ObscuredInt skillPoint; // 스킬 포인트

        /// <summary>
        /// 스킬 포인트
        /// </summary>
        public int SkillPoint => skillPoint;

        /// <summary>
        /// 스킬 슬롯 개수
        /// </summary>
        public int SkillSlotCount => skillSlotList.Count;

        /// <summary>
        /// 스킬 오토 여부
        /// </summary>
        public bool IsAutoSkill { get; private set; }

        /// <summary>
        /// 스킬 수동 여부
        /// </summary>
        public bool IsAntiSkillAuto { get; private set; }

        /// <summary>
        /// 새로운 스킬 포인트가 있을 경우 호출
        /// </summary>
        public bool HasNewSkillPoint { get; private set; }

        /// <summary>
        /// 스킬 초기화 비용 무료 여부
        /// </summary>
        public bool IsFreeSkillReset { get; private set; }

        /// <summary>
        /// 스킬 포인트 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateSkillPoint;

        /// <summary>
        /// 스킬 업데이트 시 변경
        /// </summary>
        public event System.Action OnUpdateSkill;

        /// <summary>
        /// 스킬 슬롯 업데이트 시 변경
        /// </summary>
        public event System.Action OnUpdateSkillSlot;

        /// <summary>
        /// 스킬 초기화 시 호출
        /// </summary>
        public event System.Action OnSkillInit;

        /// <summary>
        /// 스킬 변경 시 호출
        /// </summary>
        public event System.Action OnSkillChange;

        /// <summary>
        /// 장착된 스킬의 스펙이 변경될 때 호출. (스킬 장착 여부, 스킬포인트 추가, 초기화)
        /// </summary>
        public event System.Action OnChangeSkillStatus;

        /// <summary>
        /// 자동 사냥 여부
        /// </summary>
        public event System.Action<bool> OnChangeAutoSkill;

        /// <summary>
        /// 수동 사냥 여부
        /// </summary>
        public event System.Action OnChangeAntiSkillAuto;

        /// <summary>
        /// 새로운 스킬 포인트 존재 여부 이벤트
        /// </summary>
        public event System.Action OnUpdateHasNewSkillPoint;

        public SkillModel()
        {
            skillDataRepo = SkillDataManager.Instance;
            skillList = new List<SkillInfo>();
            skillDic = new Dictionary<ObscuredLong, SkillInfo>(ObscuredLongEqualityComparer.Default);
            skillSlotList = new List<ISlotValue>();
            changedSkillDic = new Dictionary<ObscuredInt, ObscuredInt>(ObscuredIntEqualityComparer.Default);
            IsAutoSkill = true;
            skillBuffer = new Buffer<SkillInfo>();
            savedEquipedSkillHashSet = new HashSet<int>(IntEqualityComparer.Default);
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.SKILL_COOLTIME_CHECK.AddEvent(OnSkillCooltimeCheck);
                Entity.OnReloadStatus += OnReloadStatus;
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.SKILL_COOLTIME_CHECK.RemoveEvent(OnSkillCooltimeCheck);
                Entity.OnReloadStatus -= OnReloadStatus;
            }
        }

        public override void ResetData()
        {
            base.ResetData();

            skillList.Clear();
            skillDic.Clear();
            changedSkillDic.Clear();
            IsAutoSkill = true;
            skillBuffer.Release();
            skillSlotList.Clear();
            savedEquipedSkillHashSet.Clear();
            IsExceptPassvieSkills = false;
        }

        void OnReloadStatus()
        {
            int skillSlotCount = SkillSlotCount;
            for (int i = 0; i < skillSlotCount; i++)
            {
                ISlotValue slotData = GetSlotInfo(i);
                if (slotData == null)
                    continue;

                SkillInfo info = GetSkill(slotData.SkillNo, isBattleSkill: true);
                if (info == null)
                    continue;

                // 기존에 보유한 스킬의 경우
                if (savedEquipedSkillHashSet.Contains(info.SkillId))
                    continue;

                Entity.ForceStartCooldown(info); // 강제 쿨타임 적용
            }

            // 장착한 스킬 세팅
            savedEquipedSkillHashSet.Clear();

            for (int i = 0; i < skillSlotCount; i++)
            {
                ISlotValue slotData = GetSlotInfo(i);
                if (slotData == null)
                    continue;

                SkillInfo info = GetSkill(slotData.SkillNo, isBattleSkill: true);
                if (info == null)
                    continue;

                savedEquipedSkillHashSet.Add(info.SkillId);
            }
        }

        /// <summary>
        /// 무료 스킬 초기화 여부 세팅
        /// </summary>
        internal void SetIsFreeSkillReset(byte isFreeSkillReset)
        {
            IsFreeSkillReset = isFreeSkillReset == 0;
        }

        internal void SetSkillPoint(int skillPoint)
        {
            // 기존의 스킬 포인트보다 더 커질 경우
            bool isNewSkillPoint = this.skillPoint < skillPoint;

            this.skillPoint = skillPoint;

            if (isNewSkillPoint)
            {
                SetHasNewSkillPoint(true);
            }
            else if (skillPoint == 0)
            {
                SetHasNewSkillPoint(false);
            }
        }

        /// <summary>
        /// 유닛 스탯 계산 시, 패시브 스킬 옵션 포함 예외 여부
        /// </summary>
        public bool IsExceptPassvieSkills { get; private set; }

        internal void Initialize(bool isExceptPassvieSkills, ISkillValue[] skills)
        {
            IsExceptPassvieSkills = isExceptPassvieSkills;
            Initialize(skills);
        }

        internal void Initialize(ISkillValue[] skills)
        {
            if (skills != null)
            {
                System.Array.Sort(skills, SortByOrderId); // Sort

                foreach (var item in skills)
                {
                    Initialize(item);
                }
            }

            // 스킬 업데이트 이벤트 호출
            OnUpdateSkill?.Invoke();
        }

        internal void Initialize(ISlotValue[] slots)
        {
            skillSlotList.Clear();

            if (slots != null)
            {
                System.Array.Sort(slots, SortByOrderId); // Sort
                skillSlotList.AddRange(slots);
            }

            SortSkills(); // 스킬 다시 정렬

            // 스킬 슬롯 업데이트 이벤트 호출
            OnUpdateSkillSlot?.Invoke();
        }

        internal void UpdateData(short? inputSkillPoint)
        {
            if (inputSkillPoint == null)
                return;

            int skillPoint = inputSkillPoint.Value;
            if (this.skillPoint == skillPoint)
                return;

            // 기존의 스킬 포인트보다 더 커질 경우
            bool isNewSkillPoint = this.skillPoint < skillPoint;

            this.skillPoint = skillPoint;
            OnUpdateSkillPoint?.Invoke();

            if (isNewSkillPoint)
            {
                SetHasNewSkillPoint(true);
            }
            else if (skillPoint == 0)
            {
                SetHasNewSkillPoint(false);
            }
        }

        /// <summary>
        /// 전투 스킬Id (초월로 인하여 변경될 수 있음)
        /// </summary>
        public int GetBattleSkillId(int skillId)
        {
            SkillInfo info = Entity.battleSkillInfo.GetBattleSkill(skillId);
            return info == null ? skillId : info.SkillId;
        }

        /// <summary>
        /// 스킬 정보 반환
        /// </summary>
        public SkillInfo GetSkill(long skillNo, bool isBattleSkill)
        {
            if (skillNo == 0L)
                return null;

            if (!skillDic.ContainsKey(skillNo))
                return null;

            SkillInfo info = skillDic[skillNo];

            // 실제 전투에 사용하고 있는 스킬 (참조 가져오기)
            if (isBattleSkill)
            {
                SkillInfo battleSkill = Entity.battleSkillInfo.GetBattleSkill(info.SkillId);
                return battleSkill ?? info;
            }

            return info;
        }

        /// <summary>
        /// 보유한 모든 스킬 정보 반환 (스킬레벨 0은 무시)
        /// </summary>
        public SkillInfo[] GetAllPossessedSkills()
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                if (skillList[i].SkillLevel == 0)
                    continue;

                // 공격형 스킬의 경우
                if (skillList[i] is ActiveSkill)
                {
                    SkillInfo battleSkill = Entity.battleSkillInfo.GetBattleSkill(skillList[i].SkillId);
                    if (battleSkill == null)
                    {
                        skillBuffer.Add(skillList[i]);
                    }
                    else
                    {
                        skillBuffer.Add(battleSkill);
                    }
                }
                else if (skillList[i] is PassiveSkill)
                {
                    SkillInfo battleSkill = Entity.battleSkillInfo.GetBattleSkill(skillList[i].SkillId);
                    if (battleSkill == null)
                    {
                        skillBuffer.Add(skillList[i]);
                    }
                    else
                    {
                        skillBuffer.Add(battleSkill);
                    }
                }
                else
                {
                    skillBuffer.Add(skillList[i]);
                }
            }

            return skillBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 스킬 고유 번호 반환
        /// </summary>
        public long GetSkillNo(int skillId)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                if (skillList[i].SkillId == skillId)
                    return skillList[i].SkillNo;
            }

            return 0L;
        }

        /// <summary>
        /// 스킬 슬롯 정보 반환
        /// </summary>
        public ISlotValue GetSlotInfo(int index)
        {
            return index >= 0 && index < skillSlotList.Count ? skillSlotList[index] : null;
        }

        /// <summary>
        /// 장착한 모든 스킬 반환
        /// </summary>
        public SkillInfo[] GetValidSkills(EquipmentClassType weaponType)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                // 패시브 스킬 옵션 제외
                //Debug.LogError($"IsExceptPassvieSkills={IsExceptPassvieSkills}, skillList[i].SkillType={skillList[i].SkillType}");
                if (IsExceptPassvieSkills && skillList[i].SkillType == SkillType.Passive)
                    continue;

                // 보유스킬 && 무기유효성검사 (Active일 경우에는 자동 장착했는지 여부 체크)
                if (skillList[i].IsInPossession && skillList[i].IsValid(weaponType))
                    skillBuffer.Add(skillList[i]);
            }

            return skillBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 스킬 아이디에 해당하는 스킬 레벨 반환
        /// </summary>
        public int GetSkillLevel(int skillId)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                if (skillList[i].SkillId == skillId)
                    return skillList[i].IsInPossession ? skillList[i].SkillLevel : 0; // 보유중인 스킬의 경우에만 Level 반환
            }

            return 0;
        }

        /// <summary>
        /// 새로운 스킬 포인트 존재 여부 세팅
        /// </summary>
        public void SetHasNewSkillPoint(bool hasNewSkillPoint)
        {
            HasNewSkillPoint = hasNewSkillPoint;
            OnUpdateHasNewSkillPoint?.Invoke();
        }

        /// <summary>
        /// 변경된 스킬 아이디 반환
        /// </summary>
        public int GetChangedSkillId(int skillId)
        {
            // 존재할 경우에는 변경된 스킬 아이디 반환
            if (changedSkillDic.ContainsKey(skillId))
                return changedSkillDic[skillId];

            return 0;
        }

        /// <summary>
        /// 스킬 슬롯에 해당하는 스킬 아이디
        /// </summary>
        public int GetSkillId(int slotIndex)
        {
            ISlotValue slotData = GetSlotInfo(slotIndex);

            if (slotData == null)
                return 0;

            SkillInfo skillInfo = GetSkill(slotData.SkillNo, isBattleSkill: false);
            return skillInfo == null ? 0 : skillInfo.SkillId;
        }

        /// <summary>
        /// 레벨업 한 스킬 존재 여부
        /// </summary>
        public bool HasInPossessionSkill()
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                if (skillList[i].IsInPossession)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 장착 가능한 스킬 존재 여부
        /// </summary>
        public bool HasEquipSkill()
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                // 보유한 스킬이면서 Active 인 스킬이 존재
                if (skillList[i].IsInPossession && skillList[i].SkillType == SkillType.Active)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 빈슬롯에 장착 가능한 스킬 존재 여부
        /// </summary>
        public bool HasEquipSkillSlot()
        {
            bool isEmptySlot = false;
            for (int i = 0; i < skillSlotList.Count; i++)
            {
                if (skillSlotList[i].SkillNo == 0)
                {
                    isEmptySlot = true;
                    break;
                }
            }

            if (!isEmptySlot)
                return false;

            for (int i = 0; i < skillList.Count; i++)
            {
                // 보유한 스킬이면서 미장착인 Active 인 스킬
                if (skillList[i].IsInPossession && skillList[i].SlotNo == 0 && skillList[i].SkillType == SkillType.Active)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 스킬 슬롯이 비어있음 여부
        /// </summary>
        public bool IsEmptySlot()
        {
            for (int i = 0; i < skillSlotList.Count; i++)
            {
                if (skillSlotList[i].SkillNo != 0L)
                    return false;
            }

            return true;
        }

        public void SetAntiSkillAuto(bool isAntiSkillAuto)
        {
            IsAntiSkillAuto = isAntiSkillAuto;
            for (int i = 0; i < skillList.Count; i++)
            {
                if (!skillList[i].IsInPossession)
                    continue;

                skillList[i].SetSkillRate(IsAntiSkillAuto ? 0 : 10000);

                // 초월로 인한 스킬도 같이 처리
                SkillInfo battleSkill = Entity.battleSkillInfo.GetBattleSkill(skillList[i].SkillId);
                if (battleSkill == null)
                    continue;

                battleSkill.SetSkillRate(IsAntiSkillAuto ? 0 : 10000);
            }

            OnChangeAntiSkillAuto?.Invoke();
        }

        /// <summary>
        /// 쿨타임 초기화
        /// </summary>
        public void ResetCooldown()
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                skillList[i].StartCooldown(0L); // 쿨타임 초기화

                // 초월로 인한 스킬도 같이 처리
                SkillInfo battleSkill = Entity.battleSkillInfo.GetBattleSkill(skillList[i].SkillId);
                if (battleSkill == null)
                    continue;

                battleSkill.StartCooldown(0L); // 쿨타임 초기화
            }
        }

        /// <summary>
        /// 스킬 슬롯 구매
        /// </summary>
        public async Task RequestBuySkillSlot()
        {
            // 스킬 슬롯 최대 체크
            int skillSlotCount = SkillSlotCount;
            if (skillSlotCount == BasisType.MAX_CHAR_SKILL_SLOT.GetInt())
                return;

            // 스킬 슬롯 가격
            int needCatCoin = (skillSlotCount == 2 ? BasisType.PRICE_SKILL_SLOT_1ST : BasisType.PRICE_SKILL_SLOT_2ST).GetInt();

            string title = LocalizeKey._90005.ToText(); // 스킬 슬롯 구매
            string description = LocalizeKey._90006.ToText(); // 스킬 슬롯을 구매하시겠습니까?
            if (!await UI.CostPopup(CoinType.CatCoin, needCatCoin, title, description))
                return;

            var response = await Protocol.BUY_SKILL_SLOT.SendAsync();
            if (response.isSuccess)
            {
                // 1. 캐릭터 스킬 슬롯 정보
                CharacterSkillSlotData[] skillSlots = response.GetPacketArray<CharacterSkillSlotData>("1");
                Notify(skillSlots);

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

        /// <summary>
        /// 캐릭터 스킬 초기화
        /// </summary>
        public async Task RequestSkillInit()
        {
            var response = await Protocol.SKILL_INIT.SendAsync();
            if (response.isSuccess)
            {
                IsFreeSkillReset = false;

                // 1. 캐릭터 스킬 정보
                CharacterSkillData[] skills = response.GetPacketArray<CharacterSkillData>("1");
                // 2. 캐릭터 스킬 슬롯 정보
                CharacterSkillSlotData[] skillSlots = response.GetPacketArray<CharacterSkillSlotData>("2");
                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;

                ResetSkill(skills, skillSlots, charUpdateData);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 스킬 초기화
        /// </summary>
        public void ResetSkill(CharacterSkillData[] skills, CharacterSkillSlotData[] skillSlots, CharUpdateData charUpdateData)
        {
            skillList.Clear();
            skillDic.Clear();
            changedSkillDic.Clear();

            // 스킬 초기화 이벤트 호출
            OnSkillInit?.Invoke();

            Notify(skills); // 캐릭터 스킬 정보
            Notify(skillSlots); // 캐릭터 스킬 슬롯 정보

            if (charUpdateData != null)
                Notify(charUpdateData);

            OnChangeSkillStatus?.Invoke();
        }

        /// <summary>
        /// 캐릭터 스킬 레벨업
        /// </summary>
        public async Task RequestSkillLevelUp(long no, int plusLevel)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", no);
            sfs.PutInt("2", plusLevel);

            var response = await Protocol.SKILL_LEVEL_UP.SendAsync(sfs);
            if (response.isSuccess)
            {
                // 1. 캐릭터 스킬 정보
                CharacterSkillData[] skills = response.GetPacketArray<CharacterSkillData>("1");
                Notify(skills);

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                // 퀘스트 체크
                Quest.QuestProgress(QuestType.SKILL_LEVEL_UP_COUNT); // 스킬 레벨업 횟수

                OnChangeSkillStatus?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 스킬 등록
        /// </summary>
        public async Task RequestUpdateSkillSlot(long charSkillSlotId, long charSkillId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", charSkillSlotId);
            sfs.PutLong("2", charSkillId); // 0일 경우 등록 해제

            var response = await Protocol.UPDATE_SKILL_SLOT.SendAsync(sfs);
            if (response.isSuccess)
            {
                // TODO 스킬 등록시 오토중이 아니면 스킬오토 끄기
                // TODO 나중에 전체 온/오프로 변경
                if (!IsAutoSkill && charSkillId != 0)
                {
                    await RequestRegistUseSkill(charSkillSlotId, false);
                }
                else
                {
                    // 1. 캐릭터 스킬 슬롯 정보
                    CharacterSkillSlotData[] skillSlots = response.GetPacketArray<CharacterSkillSlotData>("1");
                    Notify(skillSlots);
                }

                OnChangeSkillStatus?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 스킬 훔쳐 배우기
        /// </summary>
        public async Task RequestChangeSkill(long skillNo, int skillID)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", skillNo);
            sfs.PutInt("2", skillID);

            var response = await Protocol.SKILL_ID_CHANGE.SendAsync(sfs);
            if (response.isSuccess)
            {
                // 1. 캐릭터 스킬 정보
                CharacterSkillData skill = response.GetPacket<CharacterSkillData>("1");
                Initialize(skill);

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                // 스킬 변경 이벤트 호출
                OnSkillChange?.Invoke();

                OnChangeSkillStatus?.Invoke();

                // 스킬 업데이트 이벤트 호출
                OnUpdateSkill?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 슬롯에 등록된 스킬중 사용 스킬 설정/해제
        /// </summary>
        public async Task RequestRegistUseSkill(long charSkillSlotId, bool isAutoUse)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", charSkillSlotId);
            sfs.PutBool("2", isAutoUse);

            var response = await Protocol.REGIST_USE_SKILL.SendAsync(sfs);
            if (response.isSuccess)
            {
                // 1. 캐릭터 스킬 슬롯 정보
                CharacterSkillSlotData[] skillSlots = response.GetPacketArray<CharacterSkillSlotData>("1");
                Notify(skillSlots);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        public async Task RequestAllRegistUseSkill()
        {
            CharacterSkillSlotData[] skillSlots = null;
            foreach (var item in skillSlotList)
            {
                if (item.SkillNo == 0) continue;
                if (item.IsAutoSkill == !IsAutoSkill) continue;

                var sfs = Protocol.NewInstance();
                sfs.PutLong("1", item.SlotNo);
                sfs.PutBool("2", !IsAutoSkill);
                var response = await Protocol.REGIST_USE_SKILL.SendAsync(sfs);
                if (response.isSuccess)
                {
                    // 1. 캐릭터 스킬 슬롯 정보
                    skillSlots = response.GetPacketArray<CharacterSkillSlotData>("1");
                }
                else
                {
                    response.ShowResultCode();
                }
            }

            if (skillSlots != null)
            {
                Notify(skillSlots);
            }
            else
            {
                SetIsAutoSkill(!IsAutoSkill);
            }
        }

        private ISlotValue GetSkillSlotInfo(long skillNo)
        {
            for (int i = 0; i < skillSlotList.Count; i++)
            {
                if (skillSlotList[i].SkillNo == skillNo)
                    return skillSlotList[i];
            }

            return null;
        }

        /// <summary>
        /// 장착중인 모든 스킬의 오토여부를 설정
        /// </summary>
        public void SetAutoSkillAll(bool isAuto)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                if (!skillList[i].IsInPossession)
                    continue;

                skillList[i].SetSkillRate(isAuto ? 10000 : 0);

                // 초월로 인한 스킬도 같이 처리
                SkillInfo battleSkill = Entity.battleSkillInfo.GetBattleSkill(skillList[i].SkillId);
                if (battleSkill == null)
                    continue;

                battleSkill.SetSkillRate(isAuto ? 10000 : 0);
            }
        }

        private void Initialize(ISkillValue inputValue)
        {
            // 스킬 바꾸기
            if (inputValue.ChangeSkillId > 0)
            {
                if (changedSkillDic.ContainsKey(inputValue.SkillId))
                {
                    changedSkillDic[inputValue.SkillId] = inputValue.ChangeSkillId;
                }
                else
                {
                    changedSkillDic.Add(inputValue.SkillId, inputValue.ChangeSkillId);
                }
            }

            int skillId = inputValue.ChangeSkillId > 0 ? inputValue.ChangeSkillId : inputValue.SkillId;
            SkillData data = skillDataRepo.Get(skillId, Mathf.Max(1, inputValue.SkillLevel));
            if (data == null)
            {
                Debug.LogError($"스킬 세팅 실패: {nameof(skillId)} = {skillId}, {nameof(inputValue.SkillLevel)} = {inputValue.SkillLevel}");
                return;
            }

            SkillInfo info = GetSkill(inputValue.SkillNo, isBattleSkill: false);
            SkillType skillType = data.skill_type.ToEnum<SkillType>();

            // 데이터는 있지만 스킬 바꾸기를 통해서 SkillType 이 변경된 경우 => 기존 데이터를 제거
            if (info != null && info.SkillType != skillType)
            {
                skillDic.Remove(inputValue.SkillNo);
                skillList.Remove(info);
                info = null;
            }

            if (info == null)
            {
                // 추가
                info = Create(skillType);
                skillDic.Add(inputValue.SkillNo, info);
                skillList.Add(info);
            }

            info.SetSkillInfo(inputValue.SkillNo);

            if (inputValue.IsInPossession)
                info.SetIsInPossession();

            info.SetData(data);
        }

        /// <summary>
        /// 타입에 맞는 아이템 정보 생성
        /// </summary>
        private SkillInfo Create(SkillType skillType)
        {
            switch (skillType)
            {
                case SkillType.Plagiarism:
                case SkillType.Reproduce:
                case SkillType.SummonBall:
                case SkillType.Passive:
                case SkillType.RuneMastery:
                    return new PassiveSkill();

                case SkillType.Active:
                case SkillType.BasicActiveSkill:
                    return new ActiveSkill();

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(skillType)}] {nameof(skillType)} = {skillType.ToString()}");
                    return null;
            }
        }

        /// <summary>
        /// 오토스킬 세팅
        /// </summary>
        private void SetIsAutoSkill(bool isAutoSkill)
        {
            if (IsAutoSkill == isAutoSkill)
                return;

            IsAutoSkill = isAutoSkill;
            OnChangeAutoSkill?.Invoke(IsAutoSkill);
        }

        void OnSkillCooltimeCheck(Response response)
        {
            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.PASS_SKILL_CHECK)
                {
                    // 보유한 스킬 클라이언트 쿨타임 적용
                    if (response.sended.ContainsKey(SKILL_COOLDOWN_CHECK_SKILL_NO_KEY))
                    {
                        long sendedSkillNo = response.sended.GetLong(SKILL_COOLDOWN_CHECK_SKILL_NO_KEY);
                        SkillInfo info = GetSkill(sendedSkillNo, isBattleSkill: false);
                        if (info == null)
                        {
                            // 스킬사용과 동시에 스킬초기화
                            return;
                        }

                        // 스킬사용과 동시에 슬롯해제
                        Entity.ForceStartCooldown(info); // 강제 쿨타임 적용
                        return;
                    }

                    // 보유한 참조스킬 클라이언트 쿨타임 적용
                    if (response.sended.ContainsKey(SKILL_COOLDOWN_CHECK_REF_SKILL_ID_KEY))
                    {
                        int sendedRefSkillId = response.sended.GetInt(SKILL_COOLDOWN_CHECK_REF_SKILL_ID_KEY);
                        long sendedRefSkillCooldownTime = response.sended.GetInt(SKILL_COOLDOWN_CHECK_REF_SKILL_COOLDOWN_TIME_KEY);
                        Entity.battleSkillInfo.SetRefCooldown(sendedRefSkillId, sendedRefSkillCooldownTime);
                        return;
                    }
                }

                if (Issue.SHOW_SKILL_COOLTIME_CHECK_ERROR)
                {
                    response.ShowResultCode();
                    return;
                }
            }

            long remainCooldown = response.GetLong("1");
            if (response.ContainsKey("3"))
            {
                int? remainMp = null;
                long skillNo = response.GetLong("3");
                SkillInfo info = GetSkill(skillNo, isBattleSkill: false);

#if UNITY_EDITOR
                if (DebugUtils.IsLogSkillCoolTime)
                {
                    Debug.LogError($"서버가 생각하는 쿨타임: {remainCooldown}");
                }
#endif

                if (info == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("존재하지 않은 스킬");
#endif
                    return;
                }

                if (response.ContainsKey("2"))
                {
                    remainMp = response.GetInt("2");
                }
                else if (BattleManager.isUseSkillPoint) // 스킬 포인트 사용 중일 경우
                {
                    remainMp = Entity.CurMp - info.MpCost; // remainMp 계산
                }

                // 남은 mp 세팅
                if (remainMp.HasValue)
                {
                    Entity.SetCurrentMp(remainMp.Value);
                }

                // 쿨타임 세팅
                info.StartCooldown(remainCooldown);
            }

            // 참조 스킬의 경우
            if (response.ContainsKey("4"))
            {
                int refSkillId = response.GetInt("4");
                Entity.battleSkillInfo.SetRefCooldown(refSkillId, remainCooldown);
            }
        }

        private void SortSkills()
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                ISlotValue slotData = GetSkillSlotInfo(skillList[i].SkillNo);

                // 슬롯 정보가 없을 경우
                if (slotData == null)
                {
                    skillList[i].ResetSlotInfo();
                    skillList[i].SetSkillRate(0);
                }
                else
                {
                    skillList[i].SetSlotInfo(slotData.SlotNo, slotData.SlotIndex); // 슬롯 정보 세팅
                    if (IsAntiSkillAuto)
                    {
                        skillList[i].SetSkillRate(0);
                    }
                    else
                    {
                        skillList[i].SetSkillRate(slotData.IsAutoSkill ? 10000 : 0); // 사용중의 경우 스킬 발동 확률은 10000
                                                                                     // TODO 스킬 개별로 온/오프 기능 전체로 변경
                    }
                    SetIsAutoSkill(slotData.IsAutoSkill);
                }
            }

            skillList.Sort(SortBySlotPos); // 슬롯 변경되면 스킬 리스트 다시 변경
        }

        private int SortByOrderId(ISkillValue a, ISkillValue b)
        {
            return a.OrderId.CompareTo(b.OrderId);
        }

        private int SortByOrderId(ISlotValue a, ISlotValue b)
        {
            return a.SlotIndex.CompareTo(b.SlotIndex);
        }

        private int SortBySlotPos(SkillInfo a, SkillInfo b)
        {
            return a.SlotPos.CompareTo(b.SlotPos);
        }
    }
}