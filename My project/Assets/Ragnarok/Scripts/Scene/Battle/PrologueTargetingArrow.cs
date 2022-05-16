using UnityEngine;

namespace Ragnarok
{
    public class PrologueTargetingArrow : MonoBehaviour
    {
        private const float SHOW_DISTANCE = 3.6f;
        private const float SHOW_SQR_MAGNITUDE = SHOW_DISTANCE * SHOW_DISTANCE;

        Transform cachedTransform;
        GameObject cachedGameObject;

        private void Awake()
        {
            if (cachedGameObject == null)
            {
                cachedTransform = transform;
                cachedGameObject = gameObject;
            }
        }

        private void Hide()
        {
            SetActive(false);
        }

        private void Show()
        {
            SetActive(true);
        }

        public void SetActive(bool isActive)
        {
            if (cachedGameObject == null) Awake();

            cachedGameObject.SetActive(isActive);
        }

        public void SetPosition(Vector3 start, Vector3 end)
        {
            // 타겟과의 거리가 작을 경우에는 Hiide
            Vector3 direction = end - start;
            direction.y = 0f;

            float dist = direction.sqrMagnitude;
            if (dist < SHOW_SQR_MAGNITUDE)
            {
                Hide();
            }
            else
            {
                Show();

                Quaternion rotation = Quaternion.LookRotation(direction);
                cachedTransform.rotation = rotation.normalized;
            }
        }
    }
}