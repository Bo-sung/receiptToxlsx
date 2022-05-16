using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMakeItemDetile : UIInfo<MakePresenter, MakeInfo>, IInspectorFinder
    {
        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UILabelHelper labelCost;
        [SerializeField] UIButtonHelper btnMake;
        [SerializeField] UIRewardHelper makeItem;
        [SerializeField] GameObject goWarning;
        [SerializeField] UILabelHelper labelWarning;
        [SerializeField] UILabelHelper labelSuccessRate;
        [SerializeField] UIGridHelper materialGrid;
        [SerializeField] UIMakeMaterialSlot[] material;
        [SerializeField] ElementCount elementCount;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnMake.OnClick, OnClickedBtnMake);
            elementCount.OnRefresh += OnChangeCount;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnMake.OnClick, OnClickedBtnMake);
            elementCount.OnRefresh -= OnChangeCount;
        }

        protected override void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            btnMake.LocalKey = LocalizeKey._28000; // 제작
            labelItemName.Text = info.ItemName;

            int needZeny = info.Zeny * presenter.MakeCount;
            if (presenter.Zeny >= needZeny)
            {
                labelCost.Text = $"[64A2EE]{needZeny:N0}[-]";
            }
            else
            {
                // 제니 부족
                labelCost.Text = $"[D76251]{needZeny:N0}[-]";
            }

            makeItem.SetData(info.Reward);
            if (info.Rate > 5000)
            {
                labelSuccessRate.Text = LocalizeKey._28020.ToText()
                    .Replace("{RATE}", (info.Rate / 100f).ToString("0.##")); // [4C4A4D]제작 성공 확률 : [-][63A1EE]{RATE}%[-]
            }
            else
            {
                labelSuccessRate.Text = LocalizeKey._28021.ToText()
                    .Replace("{RATE}", (info.Rate / 100f).ToString("0.##")); // [4C4A4D]제작 성공 확률 : [-][FF0000]{RATE}%[-]
            }
            materialGrid.SetValue(info.MaterialInfos.Length);

            for (int i = 0; i < info.MaterialInfos.Length; i++)
            {
                material[i].SetData(presenter, info.MaterialInfos[i]);
            }
            btnMake.IsEnabled = info.IsMake;

            // 제한 관련
            string warningMessage = info.GetMakeWarningMessage(isPopupMessage: false);
            NGUITools.SetActive(goWarning, !string.IsNullOrEmpty(warningMessage));

            // 제한 메시지
            if (labelWarning)
                labelWarning.Text = warningMessage;

            if (!info.IsStackable)
            {
                // 재료에 장비가 포함된 제작 1회씩만 제작가능
                presenter.SetMakeCount(1, isEvent: false);
                elementCount.SetEnable(false);
            }
            else if (info.MaxMakeCount <= 0)
            {
                elementCount.SetEnable(false);
            }
            else
            {
                elementCount.SetEnable(true);
            }
            elementCount.Initiallize(presenter.MakeCount, info.MaxMakeCount);
        }

        void OnClickedBtnMake()
        {
            // 가방 무게 체크
            if (info.IsWeight() && !UI.CheckInvenWeight())
                return;

            // 제작시 장비 선택 여부 체크
            if (!info.IsStackable)
            {
                for (int i = 0; i < info.MaterialInfos.Length; i++)
                {
                    if (!info.MaterialInfos[i].IsStackable)
                    {
                        int count = presenter.GetSelectItemCount(info.MaterialInfos[i].SlotIndex);
                        if (count < info.MaterialInfos[i].Count)
                        {
                            // 장비 선택
                            presenter.SetSelectMaterialInfo(info.MaterialInfos[i]);
                            UI.Show<UIMakeSelectPart>();
                            return;
                        }
                    }
                }
            }

            string warningMessage = info.GetMakeWarningMessage(isPopupMessage: true);
            if (!string.IsNullOrEmpty(warningMessage))
            {
                UI.ShowToastPopup(warningMessage);
                return;
            }

            presenter.MakeItem();
        }

        void OnChangeCount()
        {
            presenter.SetMakeCount(elementCount.Count);
            info.SetItemCount();
        }

        bool IInspectorFinder.Find()
        {
            material = GetComponentsInChildren<UIMakeMaterialSlot>();
            return true;
        }
    }
}