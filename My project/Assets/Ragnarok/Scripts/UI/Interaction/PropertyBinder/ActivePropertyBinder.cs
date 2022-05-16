using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 바인딩 되어 있는 변수(bool)로
    /// GameObject의 Active 제어
    /// </summary>
    public class ActivePropertyBinder : PropertyBinder<bool>
    {
        GameObject myGameObject;

        [SerializeField]
        bool isActiveInHierarchy;

        void Awake()
        {
            myGameObject = gameObject;
        }

        protected override bool ToReverseValue(bool value)
        {
            return !value;
        }

        protected override bool Get()
        {
            if (isActiveInHierarchy)
                return myGameObject.activeInHierarchy;

            return myGameObject.activeSelf;
        }

        protected override void Set(bool value)
        {
            myGameObject.SetActive(value);
        }

        public override bool Equals(bool x, bool y)
        {
            return x == y;
        }

        public override int GetHashCode(bool obj)
        {
            return obj.GetHashCode();
        }
    }
}