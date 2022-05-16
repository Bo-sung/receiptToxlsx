using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIForestBossIcon : UIView
    {
        [SerializeField] UISprite icon;
        [SerializeField] UILabelHelper labelLevel;

        Transform myTransform;

        protected override void Awake()
        {
            base.Awake();

            myTransform = transform;
        }

        protected override void OnLocalize()
        {
        }

        /// <summary>
        /// 레벨 세팅
        /// </summary>
        public void SetLevel(int level)
        {
            labelLevel.Text = LocalizeKey._4102.ToText() // Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, level);
        }

        /// <summary>
        /// 아이콘 세팅
        /// </summary>
        public void SetIconName(string spriteName)
        {
            icon.spriteName = spriteName;
        }

        /// <summary>
        /// 아이콘 이름 반환
        /// </summary>
        public string GetIconName()
        {
            return icon.spriteName;
        }

        /// <summary>
        /// x축만 움직임
        /// </summary>
        public void SetPosX(float worldPosX)
        {
            Vector3 pos = myTransform.position;
            pos.x = worldPosX;

            myTransform.position = pos;
        }
    }
}