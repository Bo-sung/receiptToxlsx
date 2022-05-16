using UnityEngine;

namespace Ragnarok
{
    public class UITutoFirstPopup : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        [SerializeField] UIEventTrigger unselect;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnExit;

        protected override void OnInit()
        {
            EventDelegate.Add(unselect.onClick, Back);
            EventDelegate.Add(btnConfirm.OnClick, Back);
            EventDelegate.Add(btnExit.OnClick, Back);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(unselect.onClick, Back);
            EventDelegate.Remove(btnConfirm.OnClick, Back);
            EventDelegate.Remove(btnExit.OnClick, Back);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            /**
             * [라그나로크:메이즈]에 오신 걸 환영합니다.
             * 모험심을 자극하는 [62AEE4][C]미로[/c][-]들과
             * 거대한 MVP 몬스터가 등장하는 [62AEE4][C]사냥 필드[/c][-]까지,
             * 흥미진진한 모험들이 여러분을 기다리고 있습니다.
             * 
             * 또한, 신들의 가호를 받은 나무들이 계속 자랍니다.
             * 모험가님은 언제든 돌아오셔서
             * 나무에 열린 [62AEE4][C]제니[/c][-]와 [62AEE4][C]아이템[/c][-]을 얻을 수 있습니다.
             * 이제 모험을 떠나볼까요?
             */
            labelDescription.LocalKey = LocalizeKey._26032;

            labelTitle.LocalKey = LocalizeKey._26028; // 환영합니다
            btnConfirm.LocalKey = LocalizeKey._18003; // 확인
        }
    }
}