using UnityEngine;

namespace Ragnarok
{
    public class UIBookOnBuff : UIBook
    {
        [SerializeField] UIBookOnBuffView onBuffView;

        protected override void AddEventButtons()
        {
            base.AddEventButtons();
            EventDelegate.Add(mainTab[5].OnChange, OnTabChangeOnBuff);
        }

        protected override void RemoveEventButtons()
        {
            base.RemoveEventButtons();
            EventDelegate.Remove(mainTab[5].OnChange, OnTabChangeOnBuff);
        }

        protected override void InitView()
        {
            base.InitView();
            onBuffView.OnInit();
        }

        protected override void CloseView()
        {
            base.CloseView();
            onBuffView.OnClose();
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();
            //mainTab[5].Text = LocalizeKey._40233.ToText(); // 스페셜
        }

        protected override void ShowTab()
        {
            base.ShowTab();
            mainTab[5].Value = false;
        }

        private void OnTabChangeOnBuff()
        {
            if (!mainTab[5].Value)
                return;

            SetCurView(onBuffView);
        }
    }
}