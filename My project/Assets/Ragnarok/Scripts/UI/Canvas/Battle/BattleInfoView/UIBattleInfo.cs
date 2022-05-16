using UnityEngine;

namespace Ragnarok
{
    public class UIBattleInfo : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIWidget anchors;
        [SerializeField] GameObject mode1, mode2, mode3;
        [SerializeField] UILabelHelper labelMain;
        [SerializeField] UIButtonHelper btnInfo;

        public enum Mode
        {
            TYPE_1,
            TYPE_2,
            TYPE_3,
        }

        private string mainText;

        public event System.Action OnClickedBattleInfo;

        protected override void OnInit()
        {
            SetMode(Mode.TYPE_1);

            EventDelegate.Add(btnInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        private void Refresh()
        {
            if (IsInvalid())
                return;

            Show();

            labelMain.Text = mainText;
        }

        void OnClickedBtnInfo()
        {
            OnClickedBattleInfo?.Invoke();
        }

        /// <summary>
        /// 표시될 텍스트와 아이콘 세팅
        /// </summary>
        public void Set(string mainText)
        {
            // 변동사항이 없다면 Refresh 안 함.
            if (string.Equals(this.mainText, mainText))
                return;

            this.mainText = mainText;

            Refresh();
        }

        private bool IsInvalid()
        {
            return (string.IsNullOrEmpty(mainText));
        }

        public void SetMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.TYPE_1:
                    anchors.SetAnchor(mode1, 0, 0, 0, 0);
                    break;

                case Mode.TYPE_2:
                    anchors.SetAnchor(mode2, 0, 0, 0, 0);
                    break;

                case Mode.TYPE_3:
                    anchors.SetAnchor(mode3, 0, 0, 0, 0);
                    break;
            }
        }
    }
}