using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleToastPopup : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_Chatting;

        [SerializeField] UIPlayTween tween;
        [SerializeField] UITextureHelper texture;
        [SerializeField] UILabelHelper labelDescription;

        private readonly Queue<Input> queue = new Queue<Input>();
        private bool isPlayTween;

        public event System.Action OnFinished;

        protected override void OnInit()
        {
            EventDelegate.Add(tween.onFinished, OnFinishedTween);

            SetActiveTween(false);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(tween.onFinished, OnFinishedTween);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Play(string textureName, string description)
        {
            queue.Enqueue(new Input(textureName, description));

            SetActiveTween(true);
            PlayTween();
        }

        void OnFinishedTween()
        {
            isPlayTween = false;

            if (queue.Count > 0)
            {
                PlayTween();
            }
            else
            {
                SetActiveTween(false);
                OnFinished?.Invoke();
            }
        }

        private void SetActiveTween(bool isActive)
        {
            NGUITools.SetActive(tween.tweenTarget, isActive);
        }

        private void PlayTween()
        {
            if (isPlayTween)
                return;

            isPlayTween = true;
            tween.Play();

            Input input = queue.Dequeue();
            texture.Set(input.textureName, isAsync: false);
            labelDescription.Text = input.description;
        }

        private class Input : IUIData
        {
            public readonly string textureName;
            public readonly string description;

            public Input(string textureName, string description)
            {
                this.textureName = textureName;
                this.description = description;
            }
        }
    }
}