using UnityEngine;

namespace Ragnarok
{
    public class UISkillListToggle : UICanvas, IInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIButtonHelper toggleButton;
        [SerializeField] UILabelHelper labelAuto, labelAntiAuto, labelEndControl;

        [SerializeField] UISprite iconEndControl;
        [SerializeField] TweenAlpha tweenEndControl;
        [SerializeField] GameObject popupDevi;
        [SerializeField] UILabelHelper labelNotManipulate;

        Color shareColor = new Color32(71, 163, 255, 255);

        public event System.Action OnSelectToggle;

        protected override void OnInit()
        {
            ActiveDeviPopup(false);

            EventDelegate.Add(toggleButton.OnClick, OnClickToggle);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(toggleButton.OnClick, OnClickToggle);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
            ActiveDeviPopup(false);
        }

        protected override void OnLocalize()
        {
            labelAuto.LocalKey = LocalizeKey._2600; // AUTO
            labelAntiAuto.LocalKey = LocalizeKey._2601; // AUTO
            labelEndControl.LocalKey = LocalizeKey._2602; // 조종 종료
            labelNotManipulate.LocalKey = LocalizeKey._2603; // 아직 조작은 할 수 없어
        }

        public void Show(bool isAntiSkillAuto, bool isControl = false)
        {
            labelAuto.SetActive(!isAntiSkillAuto && !isControl);
            labelAntiAuto.SetActive(isAntiSkillAuto && !isControl);

            labelEndControl.SetActive(isControl);
            if (isControl)
            {
                if (Entity.player.Quest.IsOpenContent(ContentType.ShareControl, false))
                {
                    tweenEndControl.enabled = false;
                    iconEndControl.color = Color.white; // 알파값 초기화
                }
                else
                {
                    iconEndControl.color = shareColor;
                    tweenEndControl.enabled = true;
                }
            }
        }

        public void ActiveDeviPopup(bool isActive, bool isTween = false)
        {
            if (isTween)
            {
                if (isActive) TweenAlpha.Begin(popupDevi, 0.25f, 1);
                else TweenAlpha.Begin(popupDevi, 0.25f, 0);
            }
            else
            {
                if (isActive) TweenAlpha.Begin(popupDevi, 0, 1);
                else TweenAlpha.Begin(popupDevi, 0, 0);
            }
        }

        private void OnClickToggle()
        {
            OnSelectToggle?.Invoke();
        }

        bool IInspectorFinder.Find()
        {
            if (iconEndControl)
                tweenEndControl = iconEndControl.GetComponent<TweenAlpha>();

            return true;
        }
    }
}