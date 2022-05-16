using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class TransitionComponent : MonoBehaviour
    {
        public List<EventDelegate> onFinished = new List<EventDelegate>();
        public abstract void Animate(bool isSkip);

        public abstract bool IsSkip();

        public abstract void Finish();

        public abstract bool IsPlaying();
    }
}