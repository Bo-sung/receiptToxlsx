using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class UIEmotionSlot : UIView, IInspectorFinder
    {
        [SerializeField] Animation[] emotionAry;
        [SerializeField] GameObject[] emotionGOAry;

        [SerializeField] UIButtonHelper emotionSlot;

        Action<EmotionType, float> OnEmotion;
        bool isLoop, isInit = false;
        int idx;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(emotionSlot.OnClick, OnClickSlot);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(emotionSlot.OnClick, OnClickSlot);
        }

        protected override void OnLocalize()
        {
        }

        public void InitEmotion(int idx, bool isLoop, Action<EmotionType, float> onEmotion)
        {
            this.idx = idx;
            this.isLoop = isLoop;
            OnEmotion = onEmotion;

            // 이모션 뎁스 조절
            if (!isLoop && !isInit)
            {
                isInit = true;

                foreach (var psr in GetComponentsInChildren<ParticleSystemRenderer>())
                {
                    psr.sortingOrder = 0;
                }
            }

            foreach (var go in emotionGOAry)
            {
                go.SetActive(false);
            }

            emotionGOAry[idx].SetActive(true);

        }

        public void ActiveButton(bool isActive)
        {
            emotionSlot.IsEnabled = isActive;
        }

        private IEnumerator<float> YieldLoopEmotion(Animation ani, int idx)
        {
            var aniName = Enum.GetNames(typeof(EmotionType))[idx];
            yield return Timing.WaitForOneFrame;

            while (true)
            {
                if (!ani.isPlaying)
                {
                    ani.Play();
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        public bool IsPlayingAnimation(int idx)
        {
            return emotionAry[idx].isPlaying;
        }

        private void OnEnable()
        {
            if (isLoop)
            {
                Timing.RunCoroutineSingleton(YieldLoopEmotion(emotionAry[idx], idx).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
        }

        void OnClickSlot()
        {
            if (isLoop) // 이모션 반복(팝업UI)일 때만 처리
            {
                OnEmotion?.Invoke((EmotionType)idx, emotionAry[idx].clip.length);
            }
        }

        bool IInspectorFinder.Find()
        {
            EmotionType tempType;
            var names = Enum.GetNames(typeof(EmotionType));
            var animations = GetComponentsInChildren<Animation>();

            emotionAry = new Animation[names.Length];
            emotionGOAry = new GameObject[names.Length];

            for (int i = 0; i < animations.Length; i++)
            {
                if (Enum.TryParse(animations[i].name, out tempType))
                {
                    emotionAry[(int)tempType] = animations[i];
                    emotionGOAry[(int)tempType] = animations[i].gameObject;
                }
            }
            return true;
        }
    }
}