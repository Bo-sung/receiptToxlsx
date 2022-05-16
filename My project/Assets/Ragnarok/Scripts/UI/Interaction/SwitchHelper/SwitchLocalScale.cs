using UnityEngine;

namespace Ragnarok
{
    public sealed class SwitchLocalScale : SwitchHelper<Vector3>
    {
        Transform myTransform;

        protected override void Awake()
        {
            base.Awake();

            myTransform = transform;
        }

        protected override void Reset()
        {
            base.Reset();

            on = Vector3.one;
            off = Vector3.one;
        }

        protected override void Execute(Vector3 value)
        {
            myTransform.localScale = value;
        }
    }
}