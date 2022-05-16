using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleExit : UICanvas
    {
        protected override UIType uiType => UIType.Hide | UIType.Fixed;

        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIWidget container;
        [SerializeField] GameObject mode1, mode2, mode3;

        public enum Mode
        {
            TYPE_1,
            TYPE_2,
            TYPE_3,
        }

        public event System.Action OnExit;

        protected override void OnInit()
        {
            SetMode(Mode.TYPE_1);

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnExit.Text = LocalizeKey._2100.ToText(); // 나가기
        }

        void OnClickedBtnExit()
        {
            OnExit?.Invoke();
        }

        public void SetMode(Mode uiMode)
        {
            switch (uiMode)
            {
                case Mode.TYPE_1:
                    container.SetAnchor(mode1, 0, 0, 0, 0);
                    break;

                case Mode.TYPE_2:
                    container.SetAnchor(mode2, 0, 0, 0, 0);
                    break;

                case Mode.TYPE_3:
                    container.SetAnchor(mode3, 0, 0, 0, 0);
                    break;
            }
        }
    }
}