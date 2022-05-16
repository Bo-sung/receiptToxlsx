using UnityEngine;

namespace Ragnarok.View
{
    public class UIKafraListElement : UIElement<UIKafraListElement.IInput>
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

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Remove(btnPlus.onClick, OnClickedBtnPlus);
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

        protected override void OnLocalize()
        {

        }

        protected override void Refresh()
        {
            material.SetData(info.Material);
            labelMaterialName.Text = info.Material.ItemName;
            result.SetData(info.Result);
            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            material.SetAmount(info.Count, min: 0);
            labelAmount.Text = info.Count.ToString("N0");
        }

        void OnClickedBtnMinus()
        {
            if (info == null)
                return;

            if (info.Count > 0)
            {
                --info.Count;
            }
            else
            {
                // 누르고 있을 때에는 처리하지 않음
                if (btnMinus.IsPressing)
                    return;

                info.Count = material.GetMaxAmount(min: 0); // 최대치로 변경
            }
        }

        void OnClickedBtnPlus()
        {
            if (info == null)
                return;

            if (info.Count < material.GetMaxAmount(min: 0))
            {
                ++info.Count;
            }
            else
            {
                // 누르고 있을 때에는 처리하지 않음
                if (btnPlus.IsPressing)
                    return;

                info.Count = 0; // 최소치로 변경
            }
        }
    }
}