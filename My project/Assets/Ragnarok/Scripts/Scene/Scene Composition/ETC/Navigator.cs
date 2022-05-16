using UnityEngine;

namespace Ragnarok
{
    public class Navigator : MonoBehaviour
    {
        private Transform myTransform;

        public Transform Target { get; private set; }

        void Awake()
        {
            myTransform = transform;

            GameObject goTarget = new GameObject("Target");
            Target = goTarget.transform;
            Target.SetParent(myTransform, worldPositionStays: true);
        }

        public void Move(Vector3 motion)
        {
            //CachedTransform.Translate(motion * speed * Time.deltaTime);
            //Vector3 direction = motion * speed * Time.deltaTime;
            //Vector3 translation = GetTranslation(direction);
            //Target.position = translation;
            //CachedTransform.position = Vector3.ClampMagnitude(translation, limit);
        }

        public void Warp(Vector3 targetPos)
        {
            Target.localPosition = targetPos;
        }

        public void SetScale(Vector3 scale)
        {
            myTransform.localScale = scale;
        }

        public void SetHome(Vector3 home)
        {
            myTransform.position = home;
        }

        //private Vector3 GetTranslation(Vector3 direction)
        //{
        //    Vector3 pos = Target.position;

        //    switch (space)
        //    {
        //        case Space.World:
        //            return pos + direction; // Space Type - World 일 경우에

        //        case Space.Self:
        //            return pos + Target.TransformDirection(direction); // Space Type - Self 일 경우에
        //    }

        //    return pos;
        //}
    }
}