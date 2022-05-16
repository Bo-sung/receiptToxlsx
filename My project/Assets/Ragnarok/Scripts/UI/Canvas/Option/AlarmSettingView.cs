using UnityEngine;

namespace Ragnarok.View
{
    public class AlarmSettingView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelPush, labelNightPush, labelSharePush;
        [SerializeField] UIButton btnPush, btnNightPush, btnSharePush;
        [SerializeField] UIToggleHelper togglePush, toggleNightPush, toggleSharePush;

        public event System.Action OnSelectPush;
        public event System.Action OnSelectNightPush;
        public event System.Action OnSelectSharePush;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnPush.onClick, OnClickedBtnPush);
            EventDelegate.Add(btnNightPush.onClick, OnClickedBtnNightPush);
            EventDelegate.Add(btnSharePush.onClick, OnClickedBtnSharePush);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnPush.onClick, OnClickedBtnPush);
            EventDelegate.Remove(btnNightPush.onClick, OnClickedBtnNightPush);
            EventDelegate.Remove(btnSharePush.onClick, OnClickedBtnSharePush);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._14049; // 알림
            labelPush.LocalKey = LocalizeKey._14050; // 전체 푸시
            labelNightPush.LocalKey = LocalizeKey._14051; // 야간 푸시
            labelSharePush.LocalKey = LocalizeKey._14053; // 셰어 푸시
        }

        public void Set(bool isPush, bool isNightPush, bool isSharePush)
        {
            UIToggle.current = null;
            Debug.Log($"isPush={isPush} {togglePush.Value} isNightPush={isNightPush} {toggleNightPush.Value}, isSharePush={isSharePush} {toggleSharePush.Value}");
            togglePush.Value = isPush;
            toggleNightPush.Value = isNightPush;
            toggleSharePush.Value = isSharePush;
        }

        void OnClickedBtnPush()
        {
            Debug.Log($"OnChangePush={togglePush.Value}");
            OnSelectPush?.Invoke();
        }

        void OnClickedBtnNightPush()
        {
            Debug.Log($"OnChangeNightPush={toggleNightPush.Value}");
            OnSelectNightPush?.Invoke();
        }

        void OnClickedBtnSharePush()
        {
            Debug.Log($"OnChangeSharePush={toggleSharePush.Value}");
            OnSelectSharePush?.Invoke();
        }
    }
}