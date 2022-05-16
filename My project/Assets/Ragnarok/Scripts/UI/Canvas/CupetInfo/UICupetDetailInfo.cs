using UnityEngine;

namespace Ragnarok
{
    public class UICupetDetailInfo : UIInfo<CupetEntity>, IAutoInspectorFinder
    {
        [SerializeField] UICupetProfile cupetProfile;
        [SerializeField] UICupetDetailProfile cupetDetailProfile;

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            cupetProfile.SetData(info.Cupet);
            cupetDetailProfile.SetData(info.Cupet);
        }

        public void RefreshInfo()
        {
            Refresh();
        }

        /// <summary>
        /// Refresh (public)
        /// </summary>
        public void SetData(CupetInfoPresenter presenter)
        {
            SetData(presenter.Cupet);
        }
    }
}