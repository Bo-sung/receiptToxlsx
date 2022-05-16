using UnityEngine;

namespace Ragnarok
{
    public class ProjectileEffect : PoolObject
    {
        UnitActor caster;
        UnitActor target;
        SkillSetting.Projectile projectile;

        private float currentTime;

        public void Initialize(UnitActor caster, UnitActor target, SkillSetting.Projectile projectile)
        {
            this.caster = caster;
            this.target = target;
            this.projectile = projectile;

            Link();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            currentTime = 0f;
        }

        /// <summary>
        /// 위치 정보 세팅
        /// </summary>
        private void Link()
        {
            bool toTarget = false;
            UnitActor unit = toTarget ? target : caster; // ToTarget

            // 죽었을 경우
            if (unit.Entity.IsDie)
            {
                transform.position = unit.Entity.LastPosition; // 마지막 위치한 곳으로
            }
            else
            {
                Transform source = unit.CachedTransform;
                Transform node = GetNode(source, projectile.node); // Node
                transform.SetParent(node ?? source, worldPositionStays: false); // Attach
                transform.localPosition = projectile.offset; // Offset
                transform.localRotation = Quaternion.Euler(projectile.rotate); // Rotate

                // 다시 빼주는 작업
                bool isAttach = false;
                if (!isAttach)
                {
                    transform.SetParent(null, worldPositionStays: true);
                    transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Find Recursive
        /// </summary>
        private Transform GetNode(Transform tf, string name)
        {
            if (tf == null)
                return null;

            if (string.IsNullOrEmpty(name))
                return null;

            if (tf.name.Equals(name))
                return tf;

            // 재귀함수를 통하여 모든 Transform 의 name 을 찾음
            for (int i = 0; i < tf.childCount; ++i)
            {
                Transform child = GetNode(tf.GetChild(i), name);

                if (child)
                    return child;
            }

            return null;
        }
    }
}