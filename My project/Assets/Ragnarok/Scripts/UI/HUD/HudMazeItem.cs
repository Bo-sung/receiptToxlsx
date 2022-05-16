using UnityEngine;

namespace Ragnarok
{
    public sealed class HudMazeItem : HUDObject, IAutoInspectorFinder
    {
        [SerializeField] UIPlayTween offset;
        [SerializeField] UISpriteAnimation loop;
        [SerializeField] UIPlayTween finish;
        [SerializeField] UISprite item;
        
        GameObject goLoop;

        protected override void Awake()
        {
            base.Awake();

            goLoop = loop.gameObject;
            EventDelegate.Add(finish.onFinished, Release);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(finish.onFinished, Release);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            offset.Play();
            NGUITools.SetActive(goLoop, true);
            NGUITools.SetActive(finish.tweenTarget, false);
        }

        public void Finish(int itemType)
        {
            NGUITools.SetActive(goLoop, false);

            item.spriteName = string.Concat(loop.namePrefix, itemType.ToString());
            finish.Play();
        }
    }
}