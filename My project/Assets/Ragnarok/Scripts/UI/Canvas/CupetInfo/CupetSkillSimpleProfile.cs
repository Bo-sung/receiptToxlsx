using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 스킬아이콘 + Lock커버
    /// </summary>
    public class CupetSkillSimpleProfile : UIInfo<SkillInfo>
    {
        [SerializeField] UIButtonHelper btnSelf;
        [SerializeField] UITextureHelper skillIcon;
        [SerializeField] GameObject goLockBase;
        [SerializeField] GameObject goSelectBase;
        bool isLocked;
        bool isSelected;

        public List<EventDelegate> OnClick => btnSelf.OnClick;

        protected override void Awake()
        {
            base.Awake();

            isLocked = false;
            isSelected = false;
        }

        public new bool IsInvalid()
        {
            return base.IsInvalid();
        }

        public void SetLock(bool isLock)
        {
            this.isLocked = isLock;
            Refresh();
        }

        public void SetSelect(bool isSelect)
        {
            this.isSelected = isSelect;
            Refresh();
        }

        protected override void Refresh()
        {
            if (IsInvalid())
            {
                skillIcon.SetActive(false);
                goLockBase.SetActive(false);
                if (goSelectBase)
                    goSelectBase.SetActive(false);
                return;
            }

            // 아이콘
            skillIcon.SetActive(true);
            skillIcon.SetSkill(info.IconName);

            // 잠금
            goLockBase.SetActive(isLocked);

            // 체크표시
            if (goSelectBase)
                goSelectBase.SetActive(isSelected);
        }
    }
}