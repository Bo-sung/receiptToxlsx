using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class AdvancedUnitAnimator : UnitAnimator
    {
        Animator aniBody;

        public override void SetBody(GameObject go)
        {
            aniBody = GetAnimation(go);

            string saved = curPlayingAnim;
            
            //if (saved != null && CanPlay(saved))
            //    Play(saved);
        }

        private Animator GetAnimation(GameObject go)
        {
            Animator ani = go.GetComponent<Animator>();

            if (ani == null)
                ani = go.GetComponentInChildren<Animator>();

            return ani ?? go.AddMissingComponent<Animator>();
        }

        protected override bool IsReady()
        {
            if (aniBody == null)
                return false;

            return true;
        }
    }
}