using UnityEngine;

namespace Ragnarok
{
    public abstract class EmblemSubView : UISubCanvas<GuildEmblemPresenter>, IAutoInspectorFinder, IInspectorFinder
    {
        [SerializeField] protected UILabelHelper labelTitle;
        [SerializeField] protected UIButtonHelper btnPrevious;
        [SerializeField] protected UIButtonHelper btnNext;
        [SerializeField] protected UIScrollView scrollView;
        [SerializeField] protected UIToggle[] toggles;

        protected override void OnInit()
        {
            EventDelegate.Add(btnPrevious.OnClick, OnClickedBtnPrevious);
            EventDelegate.Add(btnNext.OnClick, OnClickedBtnNext);
            foreach (var item in toggles)
            {
                EventDelegate.Add(item.onChange, OnChangeToggle);
            }
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnPrevious.OnClick, OnClickedBtnPrevious);
            EventDelegate.Remove(btnNext.OnClick, OnClickedBtnNext);
            foreach (var item in toggles)
            {
                EventDelegate.Remove(item.onChange, OnChangeToggle);
            }
        }

        protected override void OnShow()
        {

        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {

        }

        protected virtual void OnChangeToggle()
        {

        }

        void OnClickedBtnPrevious()
        {
            Vector3 offset = new Vector3();
            offset.y = scrollView.panel.cachedTransform.localPosition.y;
            offset.x = scrollView.panel.cachedTransform.localPosition.x + scrollView.panel.finalClipRegion.z;
            SpringPanel.Begin(scrollView.panel.cachedGameObject, offset, 8f);
        }

        void OnClickedBtnNext()
        {
            Vector3 offset = new Vector3();
            offset.y = scrollView.panel.cachedTransform.localPosition.y;
            offset.x = scrollView.panel.cachedTransform.localPosition.x - scrollView.panel.finalClipRegion.z;
            SpringPanel.Begin(scrollView.panel.cachedGameObject, offset, 8f);
        }

        bool IInspectorFinder.Find()
        {
            toggles = GetComponentsInChildren<UIToggle>();
            return true;
        }
    }
}
