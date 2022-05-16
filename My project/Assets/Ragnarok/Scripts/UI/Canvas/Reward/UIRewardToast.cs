using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class RewardToastData : IUIData
    {
        public string iconName { get; private set; }
        public string description { get; private set; }

        public RewardToastData(string iconName, string description)
        {
            this.iconName = iconName;
            this.description = description;
        }
    }

    public sealed class UIRewardToast : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIWidget widget;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labDescription;
        [Range(1f, 10f)]
        [SerializeField] float visibleTime = 1f;
        [Range(0f, 5f)]
        [SerializeField] float alphaTime = 0.2f;
        [Range(0f, 5f)]
        [SerializeField] float moveTime = 0.2f;
        [SerializeField] float moveY = 60f;
        [SerializeField] Vector2 destination = new Vector2(0f, 300f);

        private Queue<RewardToastData> queue;
        private bool isPlaying = false;
        private float time = 0;

        protected override void OnInit()
        {
            queue = new Queue<RewardToastData>();
        }

        protected override void OnClose()
        {
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnShow(IUIData data = null)
        {
            queue.Enqueue((RewardToastData)data);

            if (!isPlaying)
                Execute();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        private void Execute()
        {
            Timing.RunCoroutine(_Execute(), Segment.RealtimeUpdate, gameObject);
        }

        IEnumerator<float> _Execute()
        {
            if (queue.Count == 0)
                yield break;

            isPlaying = true;

            var info = queue.Dequeue();
            icon.Set(info.iconName);
            labDescription.Text = info.description;

            float startAlpha, endAlpha;
            Vector2 startPos, endPos;

            // Step 1
            startAlpha = 0f;
            endAlpha = 1f;
            startPos = destination + (Vector2.up * -moveY);
            endPos = destination;

            time = 0f;
            while (time < 1f)
            {
                widget.alpha = Mathf.Lerp(startAlpha, endAlpha, time);
                widget.cachedTransform.localPosition = Vector2.Lerp(startPos, endPos, time);

                time += Time.deltaTime / moveTime;
                yield return Timing.WaitForOneFrame;
            }

            widget.alpha = endAlpha;
            widget.cachedTransform.localPosition = endPos;

            // Step 2
            yield return Timing.WaitForSeconds(visibleTime);

            // Step 3
            startAlpha = 1f;
            endAlpha = 0f;
            startPos = destination;
            endPos = destination + (Vector2.up * moveY);

            time = 0f;
            while (time < 1f)
            {
                widget.alpha = Mathf.Lerp(startAlpha, endAlpha, time);
                widget.cachedTransform.localPosition = Vector2.Lerp(startPos, endPos, time);

                time += Time.deltaTime / moveTime;
                yield return Timing.WaitForOneFrame;
            }

            widget.alpha = endAlpha;
            widget.cachedTransform.localPosition = endPos;

            isPlaying = false;

            Execute();
        }
    }
}