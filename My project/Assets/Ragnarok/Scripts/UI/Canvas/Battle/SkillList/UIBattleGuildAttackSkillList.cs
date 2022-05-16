using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleGuildAttackSkillList : UIBattleSkillList
    {
        [SerializeField] UISprite hpBar;
        [SerializeField] UILabel hpLabel;

        protected override void LastAddEvent()
        {
            base.LastAddEvent();

            entity.OnChangeHP += UpdateHP;
        }

        protected override void LastRemoveEvent()
        {
            base.LastRemoveEvent();

            entity.OnChangeHP -= UpdateHP;
        }

        public override void SetCharacter(CharacterEntity entity)
        {
            base.SetCharacter(entity);

            UpdateHP(entity.CurHP, entity.MaxHP);
        }

        private void UpdateHP(int current, int max)
        {
            float progress = MathUtils.GetProgress(current, max);

            hpBar.fillAmount = progress;
            hpLabel.text = MathUtils.GetPercentText(progress);
        }
    }
}