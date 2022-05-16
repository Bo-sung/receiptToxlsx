using UnityEngine;

namespace Ragnarok
{
    public abstract class UnitAppearance : MonoBehaviour, IEntityActorElement<UnitEntity>
    {
        UnitActor unitActor;

        GameObject myGameObject;
        Transform myTransform;

        float radius;

        protected virtual void Awake()
        {
            unitActor = GetComponent<UnitActor>();

            myGameObject = gameObject;
            myTransform = transform;
        }

        /// <summary>
        /// 몸통 연결
        /// </summary>
        protected void LinkBody(GameObject body)
        {
            unitActor.LinkBody(body); // 연결 : body => Actor
        }

        public abstract void OnReady(UnitEntity entity);

        public virtual void OnRelease()
        {
            radius = 0f;
        }

        public abstract void AddEvent();

        public abstract void RemoveEvent();

        public abstract float GetRadius();

        /// <summary>
        /// 레이어 변경
        /// </summary>
        public void SetLayer(int layer)
        {
            if (myGameObject.layer == layer)
                return;

            myGameObject.layer = layer;
            myTransform.SetChildLayer(layer);
        }

        protected float GetRadius(Transform tf)
        {
            if (radius == 0)
                radius = CalculateRadius(tf);

            return radius;
        }

        public virtual void PlayEmotion(bool isPlay, float remainTime = default)
        {
        }

        private float CalculateRadius(Transform tf)
        {
            Renderer[] renderers = tf.GetComponentsInChildren<SkinnedMeshRenderer>();
            float maxSizeX = 0f;
            foreach (var item in renderers)
            {
                maxSizeX = Mathf.Max(maxSizeX, item.bounds.size.x);
            }

            //float scaleSizeX = maxSizeX / tf.lossyScale.x; // 스케일 고려
            //return scaleSizeX * 0.5f; // 반지름으로 변경
            return maxSizeX * 0.5f; // 반지름으로 변경
        }

        public virtual void OnSetParent() { }
    }
}