using UnityEngine;

namespace Ragnarok
{
    public class UIIntegerInput : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIInput input;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButton btnMinus, btnPlus;

        int increaseValue = 1;
        int minValue = 1;
        int maxValue = int.MaxValue;
        int value;

        public event System.Action<int> OnUpdate;

        void Awake()
        {
            EventDelegate.Add(input.onChange, RefreshInputCount);
            EventDelegate.Add(input.onSubmit, RefreshInputCount);
            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(input.onChange, RefreshInputCount);
            EventDelegate.Remove(input.onSubmit, RefreshInputCount);
            EventDelegate.Remove(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Remove(btnPlus.onClick, OnClickedBtnPlus);
        }

        public void Initialize(int increaseValue, int min, int max)
        {
            this.increaseValue = increaseValue;
            minValue = min;
            maxValue = max;
        }

        public void SetTitle(string title)
        {
            labelTitle.Text = title;
        }

        public void SetValue(int value)
        {
            this.value = value;
            Refresh();
        }

        void RefreshInputCount()
        {
            value = int.Parse(input.value);
            Refresh();
        }

        void OnClickedBtnMinus()
        {
            value -= increaseValue;
            Refresh();
        }

        void OnClickedBtnPlus()
        {
            value += increaseValue;
            Refresh();
        }

        private void Refresh()
        {
            value = Mathf.Clamp(value, minValue, maxValue);
            input.value = value.ToString();
            btnMinus.isEnabled = value > minValue;
            btnPlus.isEnabled = value < maxValue;
            OnUpdate?.Invoke(value);
        }
    }
}