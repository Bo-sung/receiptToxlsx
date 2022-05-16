using UnityEngine;

namespace Ragnarok.View
{
    public class BattleFogView : UIView
    {
        [SerializeField] UIWidget fog;
        [SerializeField] int min = 560, max = 1260;
        [SerializeField] int plusSize = 100;

        protected override void OnLocalize()
        {
        }

        public override void Show()
        {
            base.Show();

            SetSize(min);
        }

        public void Plus()
        {
            SetSize(fog.width + plusSize);
        }

        public void Minus()
        {
            SetSize(fog.width - plusSize);
        }

        private void SetSize(int size)
        {
            fog.width = Mathf.Clamp(size, min, max);
        }
    }
}