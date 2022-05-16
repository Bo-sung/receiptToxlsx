using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(LineRenderer), typeof(ScrollUV))]
    public class TargetingLine : PoolObject
    {
        private const float SHOW_DISTANCE = 3.6f;
        private const float SHOW_SQR_MAGNITUDE = SHOW_DISTANCE * SHOW_DISTANCE;

        LineRenderer line;
        ScrollUV scrollUV;

        protected override void Awake()
        {
            base.Awake();

            line = GetComponent<LineRenderer>();
            scrollUV = GetComponent<ScrollUV>();
        }

        void Start()
        {
            scrollUV.speed = 4f;
            scrollUV.direction = ScrollUV.Direction.Horizontal;
        }

        public void SetPosition(Vector3 start, Vector3 end)
        {
            // 타겟과의 거리가 작을 경우에는 Hiide
            float dist = (start - end).sqrMagnitude;
            if (dist < SHOW_SQR_MAGNITUDE)
            {
                Hide();
            }
            else
            {
                Show();

                line.SetPosition(0, start);
                line.SetPosition(1, end);
            }
        }
    }
}