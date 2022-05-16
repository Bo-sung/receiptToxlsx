using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPrologueFX : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        [SerializeField] ParticleSystem flash;
        [SerializeField] Animation anim;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public bool IsFallingDirection()
        {
            if (anim == null)
                return false;

            return anim.isPlaying;
        }

        public ParticleSystem GetFinalParticle()
        {
            return flash;
        }
    }
}