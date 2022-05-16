using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class RandomAnimation : MonoBehaviour, IInspectorFinder
    {
        public enum Type
        {
            Idle,
            Random,
        }

        [SerializeField] Animation ani;

        [SerializeField, Rename(displayName = "Idle 시간")]
        float idleTime;
        [SerializeField, Rename(displayName = "랜덤 애니메이션 시간")]
        float randomAnimationTime;

        float time;
        List<string> clips;
        bool isIdle = true;

        private void Awake()
        {
            clips = new List<string>();
            foreach (AnimationState state in ani)
            {
                if (clips.Contains(state.clip.name))
                    continue;

                clips.Add(state.clip.name);
            }
        }

        private void Update()
        {
            if (clips.Count == 0)
                return;

            time += Time.deltaTime;

            if (isIdle)
            {
                if (time > idleTime)
                {
                    isIdle = false;
                    Play(Random.Range(0, clips.Count));
                }
            }
            else
            {
                if (time > randomAnimationTime)
                {
                    isIdle = true;
                    Play(0);
                }
            }
        }

        void Play(int index)
        {
            time = 0;
            ani.CrossFade(clips[index]);
        }

        bool IInspectorFinder.Find()
        {
            ani = GetComponent<Animation>();
            return true;
        }
    }
}
