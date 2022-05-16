using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class UISwipe : UIView
    {
        public enum SwipeType
        {
            Up,
            Down,
            Right,
            Left,
        }

        [SerializeField] float minSwipeDistance = 50f; // 스와이프 최소 길이

        public event Action<SwipeType> OnSwipe;

        protected override void OnLocalize()
        {            
        }             

        void OnPress(bool isPressed)
        {          
            if(!isPressed)
            {
                Vector2 totalDelta = UICamera.currentTouch.totalDelta;
                float distance = Vector2.Distance(Vector2.zero, totalDelta);

                Debug.Log($"distance={distance}");

                if (distance < minSwipeDistance)
                    return;

                float angle = Mathf.Rad2Deg * Mathf.Atan2(totalDelta.x, totalDelta.y);

                angle = (360 + angle - 45) % 360;

                if (angle < 90)
                {
                    Debug.Log("Right");
                    OnSwipe?.Invoke(SwipeType.Right);
                }
                else if (angle < 180)
                {
                    // down
                    Debug.Log("Down");
                    OnSwipe?.Invoke(SwipeType.Down);
                }
                else if (angle < 270)
                {
                    // left
                    Debug.Log("left");
                    OnSwipe?.Invoke(SwipeType.Left);
                }
                else
                {
                    // up
                    Debug.Log("Up");
                    OnSwipe?.Invoke(SwipeType.Up);
                }                
            }
        }
    }
}