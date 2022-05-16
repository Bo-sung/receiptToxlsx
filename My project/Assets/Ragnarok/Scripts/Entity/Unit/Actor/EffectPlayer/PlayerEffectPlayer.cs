using UnityEngine;

namespace Ragnarok
{
    public class PlayerEffectPlayer : CharacterEffectPlayer
    {
        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            hasAttackImpulse = true; // 플레이어는 기본적으로 공격 시 카메라흔들림을 사용.
        }

        protected override bool IsShowCritical(bool isCritical)
        {
            return false;
        }

        protected override void OnUpdatePrivateStoreState()
        {
            // Do Nothing
            // 플레이어의 경우에는 Balloon 을 띄우지 않는다.
        }
    }
}