using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UICircleGrid : MonoBehaviour
    {
        [SerializeField] bool hideInactive;
        [SerializeField] float radius = 10f;
        [SerializeField] bool hasRotation;

        [ContextMenu("Execute")]
        public virtual void Reposition()
        {
            List<Transform> list = GetChildList();

            int count = list.Count;
            float radGap = Mathf.PI * 2f / count; // 각 유닛당 각도 차이
            float rotateGap = 360f / count;
            for (int i = 0; i < count; ++i)
            {
                float rad = radGap * i;
                float x = Mathf.Sin(rad) * radius;
                float y = Mathf.Cos(rad) * radius;
                list[i].localPosition = new Vector2(x, y);

                if (hasRotation)
                {
                    list[i].localRotation = Quaternion.Euler(0f, 0f, -rotateGap * i);
                }
            }
        }

        public List<Transform> GetChildList()
        {
            Transform myTrans = transform;
            List<Transform> list = new List<Transform>();

            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);

                if (!hideInactive || (t && t.gameObject.activeSelf))
                {
                    if (!UIDragDropItem.IsDragged(t.gameObject)) list.Add(t);
                }
            }

            return list;
        }

#if UNITY_EDITOR
        [SerializeField] float duration = 2f;

        [ContextMenu("임시 설정")]
        private void EditorSetDuration()
        {
            TweenAlpha[] tweens = transform.GetComponentsInChildren<TweenAlpha>();

            if (tweens.Length == 0)
                return;

            int count = tweens.Length;
            float delta = 1f / (count + 1);
            for (int i = 0; i < tweens.Length; i++)
            {
                tweens[i].style = UITweener.Style.Loop;
                tweens[i].duration = duration;
                tweens[i].delay = 0f;

                AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
                animationCurve.AddKey(new Keyframe(delta * (i), 1f));
                animationCurve.AddKey(new Keyframe(delta * (i + 1), 1f));
                animationCurve.AddKey(new Keyframe(delta * (i + 2), 0f));
                var keys = animationCurve.keys;
                for (int j = 0; j < keys.Length; j++)
                {
                    UnityEditor.AnimationUtility.SetKeyLeftTangentMode(animationCurve, j, UnityEditor.AnimationUtility.TangentMode.Linear);
                    UnityEditor.AnimationUtility.SetKeyRightTangentMode(animationCurve, j, UnityEditor.AnimationUtility.TangentMode.Linear);
                }
                tweens[i].animationCurve = animationCurve;
            }
        }
#endif
    }
}