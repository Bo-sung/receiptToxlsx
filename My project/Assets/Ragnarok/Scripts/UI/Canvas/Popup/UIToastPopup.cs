using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ToastPopupData : IUIData
    {
        public string description { get; private set; }

        public ToastPopupData(string description)
        {
            this.description = description;
        }
    }

    public sealed class UIToastPopup : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_Chatting;

        [SerializeField] UIPanel panel;
        [SerializeField] UILabelHelper labDescription;
        [Range(1f, 10f)]
        [SerializeField] float visibleTime = 2f;
        [Range(0f, 5f)]
        [SerializeField] float alphaTime = 0.5f;

        private Queue<string> queue;
        private bool isPlaying = false;
        private float time = 0;

        protected override void OnInit()
        {
            queue = new Queue<string>();
        }

        protected override void OnClose()
        {
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnShow(IUIData data = null)
        {
            string description = ((ToastPopupData)data).description;

            // 플레이 중이며, 글씨가 같을 경우에는 일일히 queue 에 넣지 않는다
            if (isPlaying && labDescription.Text.Equals(description))
                return;

            // Queue 에 포함되어 있는 글씨의 경우 중복 포함하지 않는다.
            foreach (var item in queue)
            {
                if (string.Equals(item, description))
                    return;
            }

            queue.Enqueue(description);

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

            panel.alpha = 1f;
            labDescription.Text = queue.Dequeue();

            yield return Timing.WaitForSeconds(visibleTime);

            time = 0f;
            while (time < 1f)
            {
                yield return Timing.WaitForOneFrame;
                time += Time.deltaTime / alphaTime;
                panel.alpha = Mathf.Lerp(1f, 0f, time);
            }

            isPlaying = false;

            Execute();
        }
    }
}
