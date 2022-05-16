using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRewardZeny : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnConfirm;

        RewardZenyPresenter presenter;

        protected override void OnInit()
        {
            presenter = new RewardZenyPresenter();

            EventDelegate.Add(btnConfirm.OnClick, OnBack);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnConfirm.OnClick, OnBack);

            rewardHelper.Launch(UIRewardLauncher.GoodsDestination.Mail);
        }

        protected override void OnShow(IUIData data = null)
        {
            rewardHelper.SetData(presenter.GetZenyBox());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
            labelDesc.LocalKey = LocalizeKey._7900; // 반가워요 모험가님~\n미드가르드 대륙의 첫 방문 기념으로\n제니 선물을 드려요~
        }
    }
}