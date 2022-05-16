using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRewardCupet : UICanvas
    {
        public class Input : IUIData
        {
            public CupetEntity cupetEntity;

            public Input(CupetEntity cupetEntity)
            {
                this.cupetEntity = cupetEntity;
            }
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UICupetProfile cupetProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void OnInit()
        {
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._20500; // 큐펫 소환
            btnConfirm.LocalKey = LocalizeKey._20501; // 확 인
        }

        public void Set(CupetModel cupetModel)
        {
            cupetProfile.SetData(cupetModel);
            labelName.Text = cupetModel.Name;
        }

        void CloseUI()
        {
            UI.Close<UIRewardCupet>();
        }
    }
}