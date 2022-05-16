using System.Text;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UILoudSpeakerPopup : UICanvas<LoudSpeakerPopupPresenter>
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;
        public override int layer => Layer.UI_Popup;

        private const int MAX_BYTES = 140;

        // 최상단
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;

        // 중단
        [SerializeField] UIInput input;
        [SerializeField] UILabelHelper labelCharCount;
        [SerializeField] UILabelHelper labelDesc;

        // 최하단
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnUse;

        private ItemInfo itemInfo; // 확성기 아이템 인포

        protected override void OnInit()
        {
            presenter = new LoudSpeakerPopupPresenter();
            presenter.AddEvent();

            input.submitOnUnselect = true;

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(input.onChange, OnChangeInput);
            EventDelegate.Add(input.onSubmit, OnSubmitInput);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnUse.OnClick, OnClickedBtnUse);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(input.onChange, OnChangeInput);
            EventDelegate.Remove(input.onSubmit, OnSubmitInput);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnUse.OnClick, OnClickedBtnUse);
        }

        /// <summary>
        /// IUIData = ItemInfo 확성기 아이템인포
        /// </summary>
        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._21100; // 확성기
            labelDesc.LocalKey = LocalizeKey._21101; // 다른 모험가들에게 알릴 내용을 써주세요!
            btnCancel.LocalKey = LocalizeKey._21102; // 취소
            btnUse.LocalKey = LocalizeKey._21103; // 사용
        }

        public void Set(long itemNo)
        {
            itemInfo = presenter.GetItemInfo(itemNo);

            if(itemInfo == null)
            {
                CloseUI();
                return;
            }

            input.value = string.Empty;
            OnChangeInput();
        }

        private void OnSubmitInput()
        {
            string trimedStr = TrimStringWithBytes(input.value, MAX_BYTES);

            input.value = trimedStr;
            OnChangeInput();
        }

        private void OnChangeInput()
        {
            int bytes = GetStringBytes(input.value);

            if (bytes > MAX_BYTES)
            {
                OnSubmitInput();
                return;
            }

            labelCharCount.Text = StringBuilderPool.Get()
                                    .Append("(").Append(MAX_BYTES - bytes).Append("/").Append(MAX_BYTES).Append(")").Release(); // (138/140)
        }

        /// <summary>
        /// 닫기 버튼
        /// </summary>
        private void OnClickedBtnExit()
        {
            CloseUI();
        }

        /// <summary>
        /// 취소 버튼
        /// </summary>
        private void OnClickedBtnCancel()
        {
            CloseUI();
        }

        /// <summary>
        /// 사용 버튼
        /// </summary>
        private void OnClickedBtnUse()
        {
            if (string.IsNullOrEmpty(input.value) || string.IsNullOrWhiteSpace(input.value))
            {
                UI.ConfirmPopup(LocalizeKey._21104.ToText()); // 내용을 입력해주세요.
                return;
            }

            CloseUI();
            presenter.RequestUseLoudSpeaker(itemInfo, input.value);
        }



        private void CloseUI()
        {
            UI.Close<UILoudSpeakerPopup>();
        }

        private int GetStringBytes(string str)
        {
            int bytes = 0;
            for (int i = 0; i < str.Length; i++)
            {
                string curChar = str.Substring(i, 1);
                bytes += Mathf.Min(Encoding.UTF8.GetByteCount(curChar), 2);
            }
            return bytes;
        }

        private string TrimStringWithBytes(string str, int limitBytes)
        {
            var sb = StringBuilderPool.Get();
            int bytes = 0;
            for (int i = 0; i < str.Length; i++)
            {
                string curChar = str.Substring(i, 1);
                int curByte = Mathf.Min(Encoding.UTF8.GetByteCount(curChar), 2);
                if (bytes + curByte > limitBytes)
                    break;
                sb.Append(curChar);
                bytes += curByte;
            }

            return sb.Release();
        }
    }
}