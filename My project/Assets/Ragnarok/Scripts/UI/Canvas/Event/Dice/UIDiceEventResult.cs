using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIDiceEventResult : UIView
    {
        public interface IInput
        {
            int EventId { get; }
            DiceEventType DiceEventType { get; }
            int DiceEventValue { get; }
            string ImageName { get; }
            int NameId { get; }
            int DescrptionId { get; }
        }

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UITextureHelper texture;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelResult;
        [SerializeField] UIButtonHelper btnConfirm;

        public event System.Action OnConfirm;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        void OnClickedBtnConfirm()
        {
            OnConfirm?.Invoke();
        }

        public void Show(IInput input)
        {
            if (input == null)
                return;

            Show();
            labelTitle.LocalKey = input.NameId;
            texture.SetEvent(input.ImageName);
            labelDescription.LocalKey = input.DescrptionId;
            labelResult.Text = input.DiceEventType.ToText(input.DiceEventValue);
        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }
    }
}