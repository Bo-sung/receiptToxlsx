using UnityEngine;

namespace Ragnarok
{
    public class UIBattleNormalSkillSlot : MonoBehaviour, IAutoInspectorFinder
    {
        public enum SlotType
        {
            Empty,
            Skill,
            Lock,
        }

        [SerializeField] GameObject lockBase;
        [SerializeField] GameObject unLockBase;
        [SerializeField] UISkillButtonHelper button;
        [SerializeField] protected GameObject auto;
        [SerializeField] ParticleSystem progress;
        [SerializeField] GameObject skillBlock;
        [SerializeField] GameObject upsideArrow;
        [SerializeField] GameObject downArrow;
        [SerializeField] GameObject goPlus;

        public event System.Action<SkillInfo, SlotType> OnSelect;

        protected SkillInfo info;
        private bool isLock;
        private EquipmentClassType weaponType;
        private bool isCooldownJustNow;
        private bool isDie;

        void Awake()
        {
            EventDelegate.Add(button.OnClick, OnClickedBtnSkill);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(button.OnClick, OnClickedBtnSkill);
        }

        void Update()
        {
            if (info == null)
                return;

            float remainCooldownTime = info.RemainCooldownTime; // 남은 쿨타임 시간
            if (remainCooldownTime > 0f)
            {
                button.Progress = info.GetCooldownProgress();
                button.Text = remainCooldownTime.ToString("0.0");
                isCooldownJustNow = true;
            }
            else
            {
                button.Progress = 0;
                button.Text = string.Empty;
                isCooldownJustNow = false;

                if (isCooldownJustNow)
                    PlayProgressEffect();
            }
        }

        /// <summary>
        /// 스킬 클릭 이벤트
        /// </summary>
        void OnClickedBtnSkill()
        {
            OnSelect?.Invoke(info, GetSlotType());
        }

        public void SetDie(bool isDie)
        {
            this.isDie = isDie;
        }

        public void SetLock(bool isLock)
        {
            this.isLock = isLock;
        }

        public void SetWeaponType(EquipmentClassType weaponType)
        {
            this.weaponType = weaponType;
        }

        public void SetData(SkillInfo info)
        {
            this.info = info;

            isCooldownJustNow = false;

            lockBase.SetActive(isLock);
            unLockBase.SetActive(!isLock);
            NGUITools.SetActive(upsideArrow, false);
            NGUITools.SetActive(downArrow, false);

            Refresh();
        }

        /// <summary>
        /// 플러스 버튼 액티브 (스테이지에서만 표시해줘야함)
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActivePlus(bool isActive)
        {
            if (goPlus)
                goPlus.SetActive(isActive);
        }

        protected virtual void Refresh()
        {
            if (info == null)
            {
                button.Progress = 0;
                button.Text = string.Empty;
                button.Icon = string.Empty;
                NGUITools.SetActive(auto, false);
                NGUITools.SetActive(skillBlock, false);
            }
            else
            {
                button.Icon = info.IconName;
                bool isAuto = info.GetIsAutoSkill();
                NGUITools.SetActive(auto, isAuto);
                // 무기와 스킬이 어울리는지 체크.
                bool isValid = info.IsAvailableWeapon(weaponType);
                NGUITools.SetActive(skillBlock, !isValid);

                button.SetMode(isDie ? UIGraySprite.SpriteMode.Grayscale : UIGraySprite.SpriteMode.None);
                button.SetColor(isDie ? Color.gray : Color.white);
            }
        }

        private void PlayProgressEffect()
        {
            progress.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            progress.Play(true);
        }

        public SlotType GetSlotType()
        {
            if (isLock)
                return SlotType.Lock;

            if (info == null)
                return SlotType.Empty;

            return SlotType.Skill;
        }
    }
}