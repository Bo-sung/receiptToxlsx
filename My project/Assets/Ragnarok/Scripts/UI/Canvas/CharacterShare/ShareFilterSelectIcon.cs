using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class ShareFilterSelectIcon : UIView, IInspectorFinder
    {
        [SerializeField] UIButtonHelper btnEmpty;
        [SerializeField] UIButtonHelper btnRemove;
        [SerializeField] GameObject info;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UISprite background;

        private int slotIdx;
        private Job job;
        private Color32 selectColor = new Color32(204, 251, 255, 255);

        public event System.Action<int> OnClickBtnRemoveSlot;
        public event System.Action<int> OnClickBtnAddSlot;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEmpty.OnClick, OnClickedBtnEmpty);
            EventDelegate.Add(btnRemove.OnClick, OnClickedBtnRemove);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEmpty.OnClick, OnClickedBtnEmpty);
            EventDelegate.Remove(btnRemove.OnClick, OnClickedBtnRemove);
        }

        protected override void OnLocalize()
        {
        }

        public void UpdateData(int idx, Job job)
        {
            slotIdx = idx;
            this.job = job;

            if (job == default)
            {
                info.SetActive(false);
                ActiveSelectIcon(false);
            }
            else
            {
                info.SetActive(true);
                icon.Set(job.GetJobIcon());
            }
        }

        public void ActiveSelectIcon(bool isActive)
        {
            // ON/OFF
            if (isActive) background.color = selectColor;
            else background.color = Color.white;
        }

        private void OnClickedBtnRemove()
        {
            OnClickBtnRemoveSlot?.Invoke(slotIdx);
        }

        private void OnClickedBtnEmpty()
        {
            OnClickBtnAddSlot?.Invoke(slotIdx);
        }

        bool IInspectorFinder.Find()
        {
            background = GetComponent<UISprite>();
            return true;
        }
    }
}