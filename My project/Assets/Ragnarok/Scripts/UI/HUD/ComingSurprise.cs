using AnimationOrTween;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ComingSurprise : HUDObject
    {
        [SerializeField] Animator animator;

        public override void OnSpawn()
        {
            base.OnSpawn();

            var anim = ActiveAnimation.Play(animator, "UI_BossComing_Surprise", Direction.Forward, EnableCondition.DoNothing, DisableCondition.DoNotDisable);
            EventDelegate.Add(anim.onFinished, Release, true);
        }
    }
}