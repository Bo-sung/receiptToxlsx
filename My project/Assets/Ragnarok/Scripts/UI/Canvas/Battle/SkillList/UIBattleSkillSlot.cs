using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleSkillSlot : UIBattleNormalSkillSlot, IAutoInspectorFinder
    {
        const string TAG = nameof(UIBattleSkillSlot);

        [SerializeField] GameObject goCostBase;
        [SerializeField] UILabelHelper labelCost;
        [SerializeField] GameObject goOutlineFX; // 스킬예약효과

        void OnEnable()
        {
            goOutlineFX.SetActive(false);
        }

        protected override void Refresh()
        {
            base.Refresh();

            if (info == null)
            {
                goCostBase.SetActive(false);
            }
            else
            {
                goCostBase.SetActive(true);
                labelCost.Text = GetSkillMpCostText();

                if (auto)
                    auto.SetActive(false);
            }
        }

        public void RefreshSkillInput(SkillInfo inputSkill)
        {
            if (goOutlineFX == null)
                return;

            goOutlineFX.SetActive(false);

            if (inputSkill == null || info == null || inputSkill.SkillNo != info.SkillNo)
                return;

            goOutlineFX.SetActive(true);
        }

        private string GetSkillMpCostText()
        {
            return string.Concat(info.MpCost, "%");
        }
    }
}