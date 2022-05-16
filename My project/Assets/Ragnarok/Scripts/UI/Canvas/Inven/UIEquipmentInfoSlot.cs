using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEquipmentInfoSlot : UIInfo<InvenPresenter, ItemInfo>
    {
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UIButtonHelper btnShowInfo;
        [SerializeField] GameObject select;
        [SerializeField] TweenAlpha select_tween;
        [SerializeField] GameObject iconNotice;

        static TweenAlpha publicTweenAlpha = null;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnShowInfo.OnClick, OnClickedBtnShowInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnShowInfo.OnClick, OnClickedBtnShowInfo);
        }

        public override void SetData(InvenPresenter presenter, ItemInfo info)
        {
            base.SetData(presenter, info);
            equipmentProfile.SetData(info);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            select.SetActive(presenter.IsDisassemble(info.ItemNo));

            // Tween Alpha를 동일하게 적용
            if (publicTweenAlpha == null)
                publicTweenAlpha = presenter.GetEquipmentItemView().publicTweenAlpha;

            select_tween.tweenFactor = publicTweenAlpha.tweenFactor;

            if (publicTweenAlpha.amountPerDelta > 0)
                select_tween.PlayForward();
            else
                select_tween.PlayReverse();

            iconNotice.SetActive(info.IsNew);

            // 전투력 증가 화살표
            equipmentProfile.SetPowerUpIcon(presenter.IsStrongerEquipment(info as EquipmentItemInfo));
        }

        /// <summary>
        /// 아이템 정보 보기 버튼 클릭, 분해 선택
        /// </summary>
        void OnClickedBtnShowInfo()
        {
            if (presenter.IsDisassembleMode)
            {
                presenter.SelectDisassemble(info);
                Refresh();
            }
            else
            {
                if (info.IsNew)
                {
                    presenter.HideNew(info);
                    iconNotice.SetActive(info.IsNew);
                }
                UI.Show<UIEquipmentInfo>().Set(info.ItemNo);
            }
        }
    }
}