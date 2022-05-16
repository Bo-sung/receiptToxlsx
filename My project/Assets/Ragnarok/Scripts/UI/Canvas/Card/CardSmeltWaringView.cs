using UnityEngine;

namespace Ragnarok.View
{
    public class CardSmeltWaringView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnConfirm;

        public event System.Action<bool> OnConfirm;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._18519; // 고급 재료 투입
            labelDescription.LocalKey = LocalizeKey._90130; // [62AEE4][C]더 높은 레벨[/c][-]까지 사용 가능한 재료가 선택 되었습니다.\n해당 아이템을 제련하는데 사용하시겠습니까?
            btnCancel.LocalKey = LocalizeKey._18520; // 취소
            btnConfirm.LocalKey = LocalizeKey._18521; // 확인
        }

        public void Set(string iconName, int count)
        {
            icon.Set(iconName);
            labelCount.Text = count.ToString();
        }

        void OnClickedBtnConfirm()
        {
            Hide();
            OnConfirm?.Invoke(true);
        }

        void OnClickedBtnCancel()
        {
            Hide();
            OnConfirm?.Invoke(false);
        }
    }
}