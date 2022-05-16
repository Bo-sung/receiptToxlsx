using UnityEngine;

namespace Ragnarok.View
{
    public class UIExchangeElement : UIElement<UIExchangeElement.IInput>
    {
        public interface IInput
        {
            event System.Action OnUpdateSelectedCount;

            int Id { get; }
            RewardData Result { get; }
            RewardData Material { get; }

            int Count { get; set; }
        }

        [SerializeField] UIMaterialRewardHelper material;
        [SerializeField] UILabelHelper labelMaterialName;
        [SerializeField] UILabelHelper labelAmount;
        [SerializeField] UIPressButton btnMinus, btnPlus;
        [SerializeField] UIRewardHelper result;
        [SerializeField] UIButtonHelper btnExchange;

        public event System.Action<int, int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            material.OnUpdateLackAmount += UpdateBtnExchange;

            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Add(btnExchange.OnClick, OnClickedBtnExchange);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            material.OnUpdateLackAmount -= UpdateBtnExchange;

            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Add(btnExchange.OnClick, OnClickedBtnExchange);
        }

        void OnClickedBtnMinus()
        {
            if (info == null)
                return;

            if (info.Count > 1)
            {
                --info.Count;
            }
            else
            {
                // 누르고 있을 때에는 처리하지 않음
                if (btnMinus.IsPressing)
                    return;

                info.Count = material.GetMaxAmount(); // 최대치로 변경
            }
        }

        void OnClickedBtnPlus()
        {
            if (info == null)
                return;

            if (info.Count < material.GetMaxAmount())
            {
                ++info.Count;
            }
            else
            {
                // 누르고 있을 때에는 처리하지 않음
                if (btnPlus.IsPressing)
                    return;

                info.Count = 1; // 최소치로 변경
            }
        }

        void OnClickedBtnExchange()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.Id, info.Count);
        }

        protected override void OnLocalize()
        {
            btnExchange.LocalKey = LocalizeKey._8067; // 교환하기
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            info.OnUpdateSelectedCount += UpdateSelectedCount;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            info.OnUpdateSelectedCount -= UpdateSelectedCount;
        }

        protected override void Refresh()
        {
            material.SetData(info.Material);
            labelMaterialName.Text = info.Material.ItemName;
            result.SetData(info.Result);
            UpdateSelectedCount();
            UpdateBtnExchange();
        }

        private void UpdateSelectedCount()
        {
            material.SetAmount(info.Count);
            labelAmount.Text = info.Count.ToString("N0");
        }

        private void UpdateBtnExchange()
        {
            btnExchange.IsEnabled = !material.IsLackAmount;
        }
    }
}