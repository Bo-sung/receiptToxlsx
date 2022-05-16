using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public class CupetModel : UnitModel<CupetEntity>, ICupetModel
    {
        private readonly CupetDataManager cupetDataRepo;
        private readonly SkillDataManager skillDataRepo;
        private readonly ExpDataManager expDataRepo;
        private readonly CupetPositionDataManager cupetPositionDataRepo;
        private readonly BetterList<SkillInfo> skillList;

        ObscuredInt cupetId; // 큐펫 id
        ObscuredInt rank; // 큐펫 성급
        ObscuredInt level; // 레벨
        ObscuredInt exp; // 경험치
        ObscuredInt count; // 조각 수량

        ObscuredInt basicStr; // 기본 힘
        ObscuredInt basicAgi; // 기본 민첩 (어질)
        ObscuredInt basicVit; // 기본 체력 (바이탈)
        ObscuredInt basicInt; // 기본 지능 (인트)
        ObscuredInt basicDex; // 기본 재주 (덱스)
        ObscuredInt basicLuk; // 기본 운 (럭)

        public int BasicStr => basicStr;
        public int BasicAgi => basicAgi;
        public int BasicVit => basicVit;
        public int BasicInt => basicInt;
        public int BasicDex => basicDex;
        public int BasicLuk => basicLuk;

        /// <summary>
        /// 보유 여부
        /// </summary>
        public bool IsInPossession => rank > 0;

        /// <summary>
        /// 큐펫 아이디
        /// </summary>
        public int CupetID => cupetId;

        /// <summary>
        /// 장착 여부
        /// </summary>
        [System.Obsolete]
        public bool IsEquip => false;

        /// <summary>
        /// 경험치
        /// </summary>
        public int Exp => exp;

        /// <summary>
        /// 보유한 조각 수량
        /// </summary>
        public int Count => count;

        /// <summary>
        /// 큐펫 등급
        /// </summary>
        public int Rank => rank;

        /// <summary>
        /// 큐펫 레벨
        /// </summary>
        public int Level => level;

        /// <summary>
        /// 큐펫 타입
        /// </summary>
        public CupetType CupetType => data.cupet_type.ToEnum<CupetType>();

        /// <summary>
        /// 큐펫 속성
        /// </summary>
        public ElementType ElementType => data.element_type.ToEnum<ElementType>();

        /// <summary>
        /// 이름 id
        /// </summary>
        public int NameId => GetNameId();

        /// <summary>
        /// 이름
        /// </summary>
        public string Name => GetName();

        /// <summary>
        /// 프리팹 이름
        /// </summary>
        public string PrefabName => GetPrefabName();

        /// <summary>
        /// 썸네일 아이콘 이름
        /// </summary>
        public string ThumbnailName => GetThumbnailName();

        /// <summary>
        /// 진화 필요여부
        /// </summary>
        public bool IsNeedEvolution
        {
            get
            {
                if (!IsInPossession || IsMaxRank())
                    return false;

                // 등급별 최대레벨 도달시 진화필요
                return level == BasisType.MAX_CUPET_LEVEL.GetInt(Rank);
            }
        }

        private CupetData data;
        private System.Action onUpdateEvent;

        bool IInfo.IsInvalidData => data == null;

        event System.Action IInfo.OnUpdateEvent
        {
            add { onUpdateEvent += value; }
            remove { onUpdateEvent -= value; }
        }

        public CupetModel()
        {
            cupetDataRepo = CupetDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;
            expDataRepo = ExpDataManager.Instance;
            cupetPositionDataRepo = CupetPositionDataManager.Instance;

            skillList = new BetterList<SkillInfo>();
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        public override void ResetData()
        {
            base.ResetData();

            data = null;
            cupetId = 0;
            rank = 0;
            level = 1;
            exp = 0;
            count = 0;

            basicStr = 0;
            basicAgi = 0;
            basicVit = 0;
            basicInt = 0;
            basicDex = 0;
            basicLuk = 0;

            skillList.Release();
        }

        /// <summary>
        /// 큐펫 세팅
        /// </summary>
        public void Initialize(int cupetID, int rank, int exp, int count)
        {
            bool isDirtyCupetData = SetCupetData(cupetID, rank);
            bool isDirtyLevel = SetLevel(exp);
            bool isDirtyExp = this.exp.Replace(exp);
            bool isDirtyCount = this.count.Replace(count);

            if (isDirtyCupetData || isDirtyLevel)
            {
                SetStat();
                Entity.ReloadStatus(); // 스탯 다시로드
            }

            if (isDirtyCupetData || isDirtyLevel || isDirtyExp || isDirtyCount)
            {
                onUpdateEvent?.Invoke(); // Model 업데이트
            }
        }

        /// <summary>
        /// 큐펫 세팅
        /// </summary>
        public void Initialize(int cupetID, int rank, int level)
        {
            bool isDirtyCupetData = SetCupetData(cupetID, rank);
            bool isDirtyLevel = this.level.Replace(level);

            if (isDirtyCupetData || isDirtyLevel)
            {
                SetStat();
                Entity.ReloadStatus(); // 스탯 다시로드
            }
        }

        /// <summary>
        /// 큐펫 데이터 반환
        /// </summary>
        public CupetData GetCupetData()
        {
            return data;
        }

        /// <summary>
        /// 최대 레벨 여부
        /// </summary>
        public bool IsMaxLevel()
        {
            if (!IsInPossession)
                return false;

            return Level == GetMaxLevel();
        }

        public int GetMaxLevel()
        {
            return BasisType.MAX_CUPET_LEVEL.GetInt(Rank);
        }

        /// <summary>
        /// 최대 등급 여부
        /// </summary>
        public bool IsMaxRank()
        {
            if (!IsInPossession)
                return false;

            return Rank == BasisType.CUPET_MAX_RANK.GetInt(); // 최대 등급
        }

        /// <summary>
        /// 소환에 필요한 조각 개수
        /// </summary>
        public int GetNeedSummonPieceCount()
        {
            return expDataRepo.GetNeedSummonPieceCount();
        }

        /// <summary>
        /// 진화에 필요한 조각 개수
        /// </summary>
        public int GetNeedEvolutionPieceCount()
        {
            if (IsMaxRank())
                return 0;

            int rank = Mathf.Max(1, Rank); // 최소 1 등급
            return BasisType.CUPET_NEED_MON_PIECE.GetInt(rank + 1); // 필요 몬스터조각 수
        }

        /// <summary>
        /// 진화에 필요한 제니 반환
        /// </summary>
        public int GetNeedEvolutionPrice()
        {
            int rank = Mathf.Max(1, Rank); // 최소 1 등급
            return BasisType.CONST_ZENY.GetInt(4) + (rank + 1) * BasisType.CONST_ZENY.GetInt(5);
        }

        /// <summary>
        /// 유효한 스킬 목록
        /// </summary>
        public SkillInfo[] GetValidSkillList()
        {
            return skillList.ToArray();
        }

        private bool SetCupetData(int cupetId, int rank)
        {
            if (cupetId == 0)
            {
                ResetData();
                return false;
            }

            bool isDirtyCupetId = this.cupetId.Replace(cupetId);
            bool isDirtyCupetRank = this.rank.Replace(rank);
            bool isDirty = isDirtyCupetId || isDirtyCupetRank;

            if (isDirty)
            {
                ResetData();
                ReloadCupetData(cupetId, rank);
            }

            return isDirty;
        }

        private void ReloadCupetData(int cupetId, int rank)
        {
            this.cupetId = cupetId;
            this.rank = rank;

            data = cupetDataRepo.Get(cupetId, Mathf.Max(1, rank));
            if (data == null)
            {
                Debug.LogError($"큐펫 데이터가 존재하지 않습니다: {nameof(cupetId)} = {cupetId}, {nameof(rank)} = {rank}");
                return;
            }

            for (int i = 0; i < CupetData.CUPET_SKILL_SIZE; i++)
            {
                int skillId = data.GetSkillID(i);

                if (skillId == 0)
                    continue;

                int skillLevel = 1;
                int skillRate = data.GetSkillRate(i);

                SkillData skillData = skillDataRepo.Get(skillId, skillLevel);

                if (skillData == null)
                {
                    Debug.LogError($"스킬 세팅 실패: {nameof(data.prefab_name)} = {data.prefab_name}, {nameof(data.id)} = {data.id}, {nameof(skillId)} = {skillId}, {nameof(skillLevel)} = {skillLevel}");
                    continue;
                }

                SkillInfo info;

                SkillType skillType = skillData.skill_type.ToEnum<SkillType>();
                switch (skillType)
                {
                    case SkillType.Plagiarism:
                    case SkillType.Reproduce:
                    case SkillType.SummonBall:
                    case SkillType.Passive:
                    case SkillType.RuneMastery:
                        info = new PassiveSkill();
                        break;

                    case SkillType.Active:
                    case SkillType.BasicActiveSkill:
                        info = new ActiveSkill();
                        break;

                    default:
                        Debug.LogError($"설정되지 않은 타입: skillType = {skillType}");
                        continue;
                }

                info.SetIsInPossession();
                info.SetData(skillData);
                info.SetSkillRate(skillRate);

                skillList.Add(info);
            }
        }

        private int GetNameId()
        {
            if (data == null)
                return 0;

            return data.name_id;
        }

        private string GetName()
        {
            if (data == null)
                return string.Empty;

            return data.name_id.ToText();
        }

        private string GetPrefabName()
        {
            string prefabName = data.prefab_name;
            if (string.IsNullOrEmpty(prefabName))
            {
                Debug.LogError($"프리팹 이름 음슴: id = {data.id}");
                return string.Empty;
            }

            return prefabName;
        }

        private string GetThumbnailName()
        {
            string thumbnailName = data.icon_name;
            if (string.IsNullOrEmpty(thumbnailName))
            {
                Debug.LogError($"썸네일 이름 음슴: id = {data.id}");
                return string.Empty;
            }

            return thumbnailName;
        }

        private bool SetLevel(int exp)
        {
            int maxLevel = IsInPossession ? GetMaxLevel() : 1;
            int level = Mathf.Min(expDataRepo.Get(exp, ExpDataManager.ExpType.Cupet).level, maxLevel);
            return this.level.Replace(level);
        }

        private void SetStat()
        {
            // 큐펫 재사용시 데이터 초기화
            if (data == null)
                return;

            CupetAutoStatusData guideData = cupetPositionDataRepo.Get(CupetType, level);
            basicStr = data.add_str + guideData.str;
            basicAgi = data.add_agi + guideData.agi;
            basicVit = data.add_vit + guideData.vit;
            basicInt = data.add_int + guideData.@int;
            basicDex = data.add_dex + guideData.dex;
            basicLuk = data.add_luk + guideData.luk;
        }
    }
}