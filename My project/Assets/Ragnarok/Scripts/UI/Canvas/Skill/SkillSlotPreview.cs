using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 스킬아이콘, +, 잠금 등의 상태를 보여주기만 하는 스킬슬롯뷰
    /// </summary>
    public class SkillSlotPreview : MonoBehaviour
    {
        [SerializeField] UITextureHelper iconSkillIcon;
        [SerializeField] GameObject iconAdd;
        [SerializeField] GameObject iconLock;
        [SerializeField] UISprite iconCooldown;
        [SerializeField] GameObject goAutoAnimation;

        private SkillInfo skillInfo;
        private bool isLocked;

        /// <summary>
        /// 스킬 및 슬롯 잠금 여부 세팅
        /// </summary>
        /// <param name="skillInfo">null일 수 있음.</param>
        public void Initialize(SkillInfo skillInfo, bool isLock)
        {
            this.skillInfo = skillInfo;
            this.isLocked = isLock;

            Refresh();
        }

        public void Refresh()
        {
            bool isValidate = (skillInfo != null);

            iconSkillIcon.SetActive(isValidate);
            iconCooldown.cachedGameObject.SetActive(isValidate);
            goAutoAnimation.SetActive(isValidate && skillInfo.GetIsAutoSkill());

            if (!isValidate)
            {
                iconLock.SetActive(isLocked);
                iconAdd.SetActive(!isLocked);
                return;
            }

            iconLock.SetActive(!isValidate);
            iconAdd.SetActive(!isValidate);

            iconSkillIcon.SetSkill(skillInfo.IconName);
        }

        private void Update()
        {
            bool isValidate = (skillInfo != null);

            if (!isValidate)
                return;

            // 쿨타임
            iconCooldown.fillAmount = skillInfo.GetCooldownProgress();
        }
    }
}