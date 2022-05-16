using UnityEngine;

namespace Ragnarok.View.BattleDuel
{
    public class BattleDuelCountView : UIView, IAutoInspectorFinder
    {
        [SerializeField] UIToggleHelper toggle;
        [SerializeField] UILabel labelCount;

        public event System.Action<bool> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(toggle.OnChange, OnChangedToggle);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Add(toggle.OnChange, OnChangedToggle);
        }

        public override void Show()
        {
            base.Show();

            toggle.Set(true); // 토글 세팅
        }

        public override void Hide()
        {
            base.Hide();

            toggle.Set(false); // 토글 세팅
        }

        protected override void OnLocalize()
        {
            toggle.LocalKey = LocalizeKey._47012; // 연속 대전
        }

        public void SetCount(int cur, int max)
        {
            labelCount.text = StringBuilderPool.Get()
                .Append(cur.ToString()).Append("/").Append(max.ToString())
                .Release();
        }

        void OnChangedToggle()
        {
            bool isOn = UIToggle.current.value;
            OnSelect?.Invoke(isOn);
        }
    }
}