using Ragnarok.View;
using System;
using UnityEngine;

namespace Ragnarok
{
    public class UICardRestorePointInfo : UICanvas
    {
        public enum Mode { RestorePointInfo, RestorePointSave }

        public class Input : IUIData
        {
            public Mode mode;
            public CardItemInfo cardItem;
            public Action<UICardSmelt.RestoreCheckContext, bool> resultHandler;
            public UICardSmelt.RestoreCheckContext userCustomValue;
        }

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper title;
        [SerializeField] UILabelHelper beforeAfterPanelLabel;
        [SerializeField] UILabelHelper infoPanelLabel;
        [SerializeField] UIButtonHelper[] closeButton;
        [SerializeField] UIButtonHelper saveButton;
        [SerializeField] GameObject infoModeButtonRoot;
        [SerializeField] GameObject saveModeButtonRoot;

        [SerializeField] UICardProfileBase cardProfile;
        [SerializeField] UIGridHelper gridRate;
        [SerializeField] UILabelHelper labelCardName;
        [SerializeField] UIGrid beforeAfterGrid;
        [SerializeField] GameObject arrowIcon;
        [SerializeField] GameObject[] beforeAfterRoot;
        [SerializeField] UILabelHelper[] levelLabels;
        [SerializeField] UILabelHelper[] qualityLabels;
        [SerializeField] UICardBattleOptionList[] cardOptionLists;
        [SerializeField] UILabelHelper[] cardOptionListPanelLabels;
        [SerializeField] UILabelHelper noticeLabel;

        private UICardSmelt.RestoreCheckContext userCustomValue;
        private Action<UICardSmelt.RestoreCheckContext, bool> resultHandler;

        public void SetHandler(Action<UICardSmelt.RestoreCheckContext, bool> resultHandler)
        {
            this.resultHandler = resultHandler;
        }

        private void OnClickSave()
        {
            resultHandler?.Invoke(userCustomValue, true);
            UI.Close<UICardRestorePointInfo>();
        }

        private void OnClickClose()
        {
            resultHandler?.Invoke(userCustomValue, false);
            UI.Close<UICardRestorePointInfo>();
        }

        protected override void OnBack()
        {
            resultHandler?.Invoke(userCustomValue, false);
            base.OnBack();
        }

        protected override void OnInit()
        {
            EventDelegate.Add(closeButton[0].OnClick, OnClickClose);
            EventDelegate.Add(closeButton[1].OnClick, OnClickClose);
            EventDelegate.Add(closeButton[2].OnClick, OnClickClose);
            EventDelegate.Add(saveButton.OnClick, OnClickSave);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(closeButton[0].OnClick, OnClickClose);
            EventDelegate.Remove(closeButton[1].OnClick, OnClickClose);
            EventDelegate.Remove(closeButton[2].OnClick, OnClickClose);
            EventDelegate.Remove(saveButton.OnClick, OnClickSave);
        }

        protected override void OnShow(IUIData data = null)
        {
            Input input = data as Input;

            userCustomValue = input.userCustomValue;
            resultHandler = input.resultHandler;

            CardItemInfo cardItemInfo = input.cardItem;
            CardItemInfo reCardItemInfo = cardItemInfo.GetRestoredCard();

            cardProfile.SetData(cardItemInfo);
            gridRate.SetValue(cardItemInfo.Rating);
            labelCardName.Text = cardItemInfo.Name;

            levelLabels[0].Text = $"Lv.{reCardItemInfo.CardLevel}";
            qualityLabels[0].Text = $"[{MathUtils.ToInt(reCardItemInfo.OptionRate * 100)}%]";
            cardOptionLists[0].SetData(reCardItemInfo, reCardItemInfo);
            levelLabels[1].Text = $"Lv.{cardItemInfo.CardLevel}";
            qualityLabels[1].Text = $"[{MathUtils.ToInt(cardItemInfo.OptionRate * 100)}%]";
            cardOptionLists[1].SetData(cardItemInfo, cardItemInfo);

            infoModeButtonRoot.SetActive(false);
            saveModeButtonRoot.SetActive(false);

            if (input.mode == Mode.RestorePointInfo)
            {
                arrowIcon.SetActive(false);
                beforeAfterRoot[1].SetActive(false);
                cardOptionListPanelLabels[0].LocalKey = LocalizeKey._22205;
                cardOptionListPanelLabels[1].LocalKey = LocalizeKey._22206;
                beforeAfterPanelLabel.LocalKey = LocalizeKey._22209;
                noticeLabel.Text = LocalizeKey._22203.ToText().Replace(ReplaceKey.LEVEL, reCardItemInfo.CardLevel);
                infoModeButtonRoot.SetActive(true);
            }
            else
            {
                arrowIcon.SetActive(true);
                beforeAfterRoot[1].SetActive(true);
                cardOptionListPanelLabels[0].LocalKey = LocalizeKey._22207;
                cardOptionListPanelLabels[1].LocalKey = LocalizeKey._22208;
                beforeAfterPanelLabel.LocalKey = LocalizeKey._22210;
                noticeLabel.Text = LocalizeKey._22204.ToText().Replace(ReplaceKey.LEVEL, reCardItemInfo.CardLevel);
                saveModeButtonRoot.SetActive(true);
            }

            beforeAfterGrid.Reposition();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            title.LocalKey = LocalizeKey._22200;
            infoPanelLabel.LocalKey = LocalizeKey._22201;
            closeButton[1].LocalKey = LocalizeKey._2902;
            closeButton[2].LocalKey = LocalizeKey._2901;
            saveButton.LocalKey = LocalizeKey._22202;
        }
    }
}