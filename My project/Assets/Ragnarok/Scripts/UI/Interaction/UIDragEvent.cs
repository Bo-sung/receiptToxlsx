using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIDragEvent : MonoBehaviour
    {
        const float VEL = 80;

        [System.Flags]
        public enum DragArrow
        {
            Right = 1 << 0,  // 1
            Left = 1 << 1, // 2
            Up = 1 << 2, // 4
            Down = 1 << 3, // 8
        }

        [SerializeField] DragArrow drag;

        public event System.Action<DragArrow> OnDragEvent;

        void OnDrag(Vector2 delta)
        {
            if (drag.HasFlag(DragArrow.Right))
            {
                if (delta.x > VEL && delta.x > Mathf.Abs(delta.y))
                    OnDragEvent?.Invoke(DragArrow.Right);
            }

            if (drag.HasFlag(DragArrow.Left))
            {
                if (delta.x < -VEL && delta.x < -Mathf.Abs(delta.y))
                    OnDragEvent?.Invoke(DragArrow.Left);
            }

            if (drag.HasFlag(DragArrow.Up))
            {
                if (delta.y > VEL && delta.y > Mathf.Abs(delta.x))
                    OnDragEvent?.Invoke(DragArrow.Up);
            }

            if (drag.HasFlag(DragArrow.Down))
            {
                if (delta.y < -VEL && delta.y < -Mathf.Abs(delta.x))
                    OnDragEvent?.Invoke(DragArrow.Down);
            }
        }
    }
}