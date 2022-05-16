using UnityEngine;

namespace Ragnarok
{
    public class BounceDroppedItem : DroppedItem
    {
        [SerializeField, Rename(displayName = "최대 랜덤 튀어오르는 추가 높이")]
        float maxRandHeight = 0.05f;

        [SerializeField, Rename(displayName = "최대 랜덤 퍼지는 추가 값")]
        Vector2 maxRandValue = Vector2.one * 0.01f;

        [SerializeField, Rename(displayName = "튕기는 시간")]
        float jumpDuration = 2f;

        [SerializeField, Range(0f, 1f), Tooltip("저항값")]
        float drag = 0.5f;

        public override void OnSpawn()
        {
            base.OnSpawn();

            float randX = Random.Range(-maxRandValue.x, maxRandValue.x);
            float randZ = Random.Range(-maxRandValue.y, maxRandValue.y);
            velocity += new Vector3(randX, 0f, randZ);
        }

        protected override void Jump()
        {
            base.Jump();

            // Bounce
            if (CachedTransform.position.y < homeY)
            {
                Vector3 pos = CachedTransform.position;
                pos.y = homeY;
                CachedTransform.position = pos;

                velocity.y *= -(1f - drag);
            }
        }

        protected override bool CheckFinishJump()
        {
            return time > jumpDuration;
        }

        protected override float GetPlusHeight()
        {
            return Random.Range(0, maxRandHeight);
        }
    }
}