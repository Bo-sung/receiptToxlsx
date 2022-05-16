namespace Ragnarok
{
    public class PlayerAI : CharacterAI
    {
        public override void StartAI()
        {
            base.StartAI();

            actor.EffectPlayer.ShowTargetingLine();
            actor.EffectPlayer.ShowTargetingArrow();
            SetInputMove(isControl: false);
        }

        protected override void RefreshAutoSkill(bool isAutoSkill)
        {
            isAutoChangeState = isAutoSkill;
        }

        public override bool CheckMpCost(SkillInfo skillInfo)
        {
            if (actor.Entity.CurMp < skillInfo.MpCost)
            {
                //Debug.LogError($"MP 부족 = {skillInfo.SkillName}, {actor.Entity.CurMp}, {skillInfo.MpCost}");
                return true;
            }
            return false;
        }
    }
}