using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UITurretInfo"/>
    /// </summary>
    public class CupetElement : UIView
    {
        [SerializeField] UICupetProfile cupetProfile;
        [SerializeField] UIButtonHelper btnCupet;

        public event System.Action<ICupetModel> OnSelectBtnCupet;

        private ICupetModel cupet;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnCupet.OnClick, OnClickedBtnCupet);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnCupet.OnClick, OnClickedBtnCupet);
        }

        protected override void OnLocalize()
        {
        }

        public void Set(CupetModel cupetModel)
        {
            cupet = cupetModel;
            cupetProfile.SetData(cupetModel);
        }

        /// <summary>
        /// 큐펫 클릭시 이벤트 호출
        /// </summary>
        void OnClickedBtnCupet()
        {
            OnSelectBtnCupet?.Invoke(cupet);
        }
    }
}