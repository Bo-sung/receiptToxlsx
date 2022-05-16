using UnityEngine;

namespace Ragnarok
{
    public class UITouchEffect : HUDObject
    {
        private float time;

        public override void OnSpawn()
        {
            base.OnSpawn();

            time = 0f;
        }

        void FixedUpdate()
        {
            time += Time.fixedDeltaTime;
            if (time >= 2f)
                Release();
        }
    }
}