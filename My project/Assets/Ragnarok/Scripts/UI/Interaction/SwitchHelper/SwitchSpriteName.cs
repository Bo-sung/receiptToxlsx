using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UISprite))]
    public sealed class SwitchSpriteName : SwitchHelper<string>
    {
        UISprite sprite;

        protected override void Awake()
        {
            base.Awake();

            sprite = GetComponent<UISprite>();
        }

        protected override void Execute(string value)
        {
            sprite.spriteName = value;
        }
    }
}