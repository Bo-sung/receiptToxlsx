using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattlePlayerStatus"/>
    /// </summary>
    public class BattlePlayerStatusPresenter : ViewPresenter
    {
        private CharacterEntity entity;

        public event UnitEntity.ChangeHPEvent OnChangeHP;
        public event UnitEntity.ChangeHPEvent OnChangeMP;
        public event System.Action OnUpdateSkill;
        public event System.Action OnChangeAutoSkill;
        public event System.Action<bool> OnChangedJob;
        public event System.Action OnChangedGender;
        public event System.Action OnUpdateStatPoint;
        public event System.Action<int> OnChangeAP;
        public event System.Action OnUpdateProfile;
        public event System.Action OnUpdateBattleMode;

        private readonly BattleManager battleManager;

        public BattlePlayerStatusPresenter()
        {
            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            BattleManager.OnChangeBattleMode += OnChangeBattleMode;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnChangeBattleMode -= OnChangeBattleMode;

            RemovePlayerEvent();
        }

        void OnChangeBattleMode(BattleMode mode)
        {
            OnUpdateBattleMode?.Invoke();
        }

        public void SetEntity(CharacterEntity entity)
        {
            // 기존 플레이어에 연결되어있던 이벤트 제거
            RemovePlayerEvent();

            this.entity = entity;

            // 이벤트 추가
            if (this.entity != null)
            {
                this.entity.OnChangeHP += InvokeOnChangeHP;
                this.entity.OnChangeMP += InvokeOnChangeMP;
                this.entity.Skill.OnUpdateSkillSlot += InvokeOnUpdateSkillSlot;
                this.entity.Skill.OnChangeAntiSkillAuto += InvokeChangeAutoSkill;
                this.entity.Character.OnChangedJob += InvokeOnChangedJob;
                this.entity.Character.OnChangedGender += InvokeOnChangedGender;
                this.entity.Status.OnUpdateStatPoint += InvokeOnUpdateStatPoint;
                this.entity.OnReloadStatus += InvokeOnReloadStatus;
                this.entity.Character.OnUpdateProfile += InvokeUpdateProfile;
                if (this.entity is PlayerEntity playerEntity)
                {
                    playerEntity.OnChangeAP += InvokeOnChangeAP;
                }
            }
        }

        /// <summary>
        /// 플레이어에 연결된 이벤트 제거
        /// </summary>
        void RemovePlayerEvent()
        {
            if (entity != null)
            {
                entity.OnChangeHP -= InvokeOnChangeHP;
                entity.OnChangeMP -= InvokeOnChangeMP;
                entity.Skill.OnUpdateSkillSlot -= InvokeOnUpdateSkillSlot;
                entity.Skill.OnChangeAntiSkillAuto -= InvokeChangeAutoSkill;
                entity.Character.OnChangedJob -= InvokeOnChangedJob;
                entity.Character.OnChangedGender -= InvokeOnChangedGender;
                entity.Status.OnUpdateStatPoint -= InvokeOnUpdateStatPoint;
                entity.OnReloadStatus -= InvokeOnReloadStatus;
                entity.Character.OnUpdateProfile -= InvokeUpdateProfile;
                if (entity is PlayerEntity playerEntity)
                {
                    playerEntity.OnChangeAP -= InvokeOnChangeAP;
                }
            }
        }

        /// <summary>
        /// Entity 설정 여부
        /// </summary>
        public bool HasEntity()
        {
            return (entity != null);
        }

        public int GetCurHP()
        {
            return entity.CurHP;
        }

        public int GetMaxHP()
        {
            return entity.MaxHP;
        }

        public int GetCurMP()
        {
            return entity.CurMp;
        }

        public int GetMaxMP()
        {
            return entity.MaxMp;
        }

        public int GetAP()
        {
            if (entity is PlayerEntity playerEntity)
            {
                return playerEntity.GetTotalAttackPower();
            }

            return 0;
        }

        public (SkillInfo skillInfo, bool isLockedSlot)[] GetSkillSlotInfos()
        {
            int maxSkillSlotCount = BasisType.MAX_CHAR_SKILL_SLOT.GetInt(); // 총 스킬 슬롯 개수 (4)
            int openedSkillSlotCount = entity.Skill.SkillSlotCount; // 잠금해제한 스킬 슬롯 개수 

            List<(SkillInfo skillInfo, bool isLockedSlot)> ret = new List<(SkillInfo skillInfo, bool isLockedSlot)>();

            for (int i = 0; i < maxSkillSlotCount; ++i)
            {
                bool isLockedSlot = (i >= openedSkillSlotCount); // 슬롯 잠금 여부

                SkillInfo skillInfo = null;
                var curSlotInfo = entity.Skill.GetSlotInfo(i); // 해당 슬롯의 스킬정보 얻어오기
                if (curSlotInfo != null)
                {
                    long skillNo = curSlotInfo.SkillNo;
                    skillInfo = entity.Skill.GetSkill(skillNo, isBattleSkill: true);
                }
                ret.Add((skillInfo, isLockedSlot));
            }
            return ret.ToArray();
        }

        public bool IsStatusNotice()
        {
            return false;
        }

        public string GetThumbnailIconName()
        {
            return entity.Character.GetProfileName();
        }

        private void InvokeOnChangeAP(int AP)
        {
            OnChangeAP?.Invoke(AP);
        }

        private void InvokeOnUpdateStatPoint()
        {
            OnUpdateStatPoint?.Invoke();
        }

        private void InvokeOnChangedJob(bool isInit)
        {
            OnChangedJob?.Invoke(isInit);
        }

        private void InvokeOnChangedGender()
        {
            OnChangedGender?.Invoke();
        }

        private void InvokeOnUpdateSkillSlot()
        {
            OnUpdateSkill?.Invoke();
        }

        private void InvokeOnReloadStatus()
        {
            OnUpdateSkill?.Invoke();
        }

        private void InvokeChangeAutoSkill()
        {
            OnChangeAutoSkill?.Invoke();
        }

        private void InvokeOnChangeHP(int current, int max)
        {
            OnChangeHP?.Invoke(current, max);
        }

        private void InvokeOnChangeMP(int current, int max)
        {
            OnChangeMP?.Invoke(current, max);
        }

        private void InvokeUpdateProfile()
        {
            OnUpdateProfile?.Invoke();
        }

        public bool IsDungeon()
        {
            return battleManager.Mode.IsDungeon();
        }

        public bool IsCostumeOpen()
        {
            return entity.Character.JobGrade() == 2;
        }
    }
}