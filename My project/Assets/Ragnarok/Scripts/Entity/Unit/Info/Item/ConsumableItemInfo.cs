using System.Collections.Generic;

namespace Ragnarok
{
    public class ConsumableItemInfo : ItemInfo
    {
        RemainTime remainCooldown;

        public override bool IsStackable => true;
        public override long Cooldown => data.cooldown;
        public override long Duration => data.duration;
        public override float RemainCooldown => remainCooldown.ToRemainTime();
        public override TargetType TargetType => data.class_type.ToEnum<TargetType>();
        public override ConsumableItemType ConsumableItemType => data.skill_rate.ToEnum<ConsumableItemType>();

        public override void SetItemInfo(int tier, int itemLevel, byte itemPos, long equippedCardNo1, long equippedCardNo2, long equippedCardNo3, long equippedCardNo4, bool isLock, int itemTranscend = 0, int itemChangedElement = 0, int itemElementLevel = 0)
        {
        }

        public override void Reload(bool isEquipCard)
        {
        }

        public override IEnumerable<BattleOption> GetBattleOptionCollection(int smelt)
        {
            BattleOption option1 = new BattleOption(data.battle_option_type_1, data.value1_b1, data.value2_b1);
            BattleOption option2 = new BattleOption(data.battle_option_type_2, data.value1_b2, data.value2_b2);
            BattleOption option3 = new BattleOption(data.battle_option_type_3, data.value1_b3, data.value2_b3);
            BattleOption option4 = new BattleOption(data.battle_option_type_4, data.value1_b4, data.value2_b4);

            if (option1.battleOptionType != BattleOptionType.None)
                yield return option1;

            if (option2.battleOptionType != BattleOptionType.None)
                yield return option2;

            if (option3.battleOptionType != BattleOptionType.None)
                yield return option3;

            if (option4.battleOptionType != BattleOptionType.None)
                yield return option4;
        }

        public override void SetRemainCoolDown(float remainCooldown)
        {
            this.remainCooldown = remainCooldown;
            InvokeEvent();
        }

        public override bool IsCooldown()
        {
            return RemainCooldown > 0f; // 남아있는 쿨타임이 존재
        }
    }
}