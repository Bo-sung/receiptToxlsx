namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattlePotion"/>
    /// </summary>
    public sealed class BattlePotionPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;
        private readonly GuildModel guildModel;
        private readonly CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly SkillDataManager skillDataRepo;

        // <!-- Event --!>
        public event System.Action OnUpdateZeny;
        public event System.Action OnUpdateCatCoin;

        public event System.Action OnUseMpPotion
        {
            add { guildModel.OnUseGuildAttackPotion += value; }
            remove { guildModel.OnUseGuildAttackPotion -= value; }
        }

        private readonly float mpPotionCooldownTime;
        private readonly float autoGuardCooldownTime;

        public BattlePotionPresenter()
        {
            goodsModel = Entity.player.Goods;
            guildModel = Entity.player.Guild;
            characterModel = Entity.player.Character;
            skillDataRepo = SkillDataManager.Instance;

            System.TimeSpan timeSpan = System.TimeSpan.FromMilliseconds(BasisType.MP_POTION_COOL_TIME.GetInt());
            mpPotionCooldownTime = (float)timeSpan.TotalSeconds;

            int skillId = BasisForestMazeInfo.UseSkillId.GetInt();
            autoGuardCooldownTime = GetSkillCooldown(skillId, level: 1);
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += InvokeUpdateZeny;
            goodsModel.OnUpdateCatCoin += InvokeUpdateCatCoin;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= InvokeUpdateZeny;
            goodsModel.OnUpdateCatCoin -= InvokeUpdateCatCoin;
        }

        private void InvokeUpdateZeny(long zeny)
        {
            OnUpdateZeny?.Invoke();
        }

        private void InvokeUpdateCatCoin(long catCoin)
        {
            OnUpdateCatCoin?.Invoke();
        }

        /// <summary>
        /// 필요 재화
        /// </summary>
        public int GetNeedCost(UIBattlePotion.MenuContent content)
        {
            switch (content)
            {
                case UIBattlePotion.MenuContent.MpPotion:
                    return BasisType.MP_POTION_ZENY_BY_JOB_LEVEL.GetInt(characterModel.JobLevel);

                case UIBattlePotion.MenuContent.AutoGuard:
                    return 0;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattlePotion.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 사용 가능 여부
        /// </summary>
        public bool CanUse(UIBattlePotion.MenuContent content)
        {
            switch (content)
            {
                case UIBattlePotion.MenuContent.MpPotion:
                    return goodsModel.Zeny >= GetNeedCost(content);

                case UIBattlePotion.MenuContent.AutoGuard:
                    return true;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattlePotion.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 쿨타임
        /// </summary>
        public float GetCooldownTime(UIBattlePotion.MenuContent content)
        {
            switch (content)
            {
                case UIBattlePotion.MenuContent.MpPotion:
                    return mpPotionCooldownTime;

                case UIBattlePotion.MenuContent.AutoGuard:
                    return autoGuardCooldownTime;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattlePotion.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 버튼 언어
        /// </summary>
        public string GetLocalKey(UIBattlePotion.MenuContent content)
        {
            switch (content)
            {
                case UIBattlePotion.MenuContent.MpPotion:
                    return string.Empty;

                case UIBattlePotion.MenuContent.AutoGuard:
                    return ForestMazeSkill.SKILL_3.NameId.ToText();

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattlePotion.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 스킬 쿨타임 반환 (초)
        /// </summary>
        private float GetSkillCooldown(int id, int level)
        {
            SkillData data = skillDataRepo.Get(id, level);
            if (data == null)
                return 1f;

            System.TimeSpan timeSpan = System.TimeSpan.FromMilliseconds(data.cooldown);
            return (float)timeSpan.TotalSeconds;
        }
    }
}