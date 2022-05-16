using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class UICharacterShareBar : UISimpleCharacterShareBar
    {
        public enum BarType
        {
            /// <summary>
            /// 아무것도 없는 상태
            /// </summary>
            None,
            /// <summary>
            /// 추가 가능 상태
            /// </summary>
            Slot,
            /// <summary>
            /// 정보 존재
            /// </summary>
            Info,
        }

        [SerializeField] GameObject goEmpty;
        [SerializeField] GameObject addIcon;
        [SerializeField] UILabelHelper labelEmpty;
        [SerializeField] GameObject goInfo;
        [SerializeField] UIButton btnDelete;
        [SerializeField] UIWidget btnAdd;

        public UIWidget AddButton => btnAdd;

        private BarType barType;
        private GameObject cachedGameObject;

        public event System.Action OnSelectAddSlot;
        public event UICharacterShare.SelectShareCharacterEvent OnSelectDelete;
        public event UICharacterShare.SelectCloneCharacterEvent OnSelectCloneDelete;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnDelete.onClick, ShowDeletePopup);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnDelete.onClick, ShowDeletePopup);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelEmpty.Text = LocalizeKey._10205.ToText(); // 고용된 용병이 없습니다.
        }

        public void SetBarType(BarType barType)
        {
            this.barType = barType;

            NGUITools.SetActive(goEmpty, barType == BarType.Slot || barType == BarType.None);
            NGUITools.SetActive(goInfo, barType == BarType.Info);
            NGUITools.SetActive(addIcon, barType == BarType.Slot);
        }

        public void SetActiveGO(bool isActive)
        {
            if (cachedGameObject == null)
                cachedGameObject = gameObject;

            gameObject.SetActive(isActive);
        }

        void OnClick()
        {
            if (barType == BarType.Slot || barType == BarType.None)
            {
                OnSelectAddSlot?.Invoke();
            }
        }

        private void ShowDeletePopup()
        {
            OnSelectDelete?.Invoke(data.Cid, data.Uid, data.SharingCharacterType);
            OnSelectCloneDelete?.Invoke(data.Cid, data.Uid, data.CloneCharacterType);
            // AsyncShowDeletePopup().WrapNetworkErrors();
        }

        //private async Task AsyncShowDeletePopup()
        //{
        //    if (data == null)
        //        return;
        //
        //    string title = LocalizeKey._10215.ToText(); // 계약 해지
        //    string message = LocalizeKey._10216.ToText() // {NAME}님의 셰어 계약을 해지하시겠습니까?
        //        .Replace(ReplaceKey.NAME, data.Name);
        //    
        //    if (!await UI.SelectPopup(title, message))
        //        return;
        //
        //    OnSelectDelete?.Invoke(data.Cid, data.Uid, data.SharingCharacterType);
        //}
    }
}