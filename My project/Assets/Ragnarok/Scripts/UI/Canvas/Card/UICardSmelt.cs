//#define USE_SELECT_VIEW

using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class UICardSmelt : UICanvas
    {
        public enum RestoreCheckContext { BeforeNormalSmelt, BeforeContinousSmelt, DuringContinousSmelt }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] CardSmeltInfoView cardSmeltInfoView;
        [SerializeField] CardSmeltOptionView cardSmeltOptionView;
        [SerializeField] CardSmeltMaterialView cardSmeltMaterialView;
        [SerializeField] CardSmeltMaterialSelectView cardSmeltMaterialSelectView;
        [SerializeField] CardSmeltView cardSmeltView;
        [SerializeField] CardSmeltProgressView cardSmeltProgressView;
        [SerializeField] CardSmeltResultView cardSmeltResultView;
        [SerializeField] CardSmeltWaringView cardSmeltWaringView;
        [SerializeField] float fxSuccessDuration, fxFailDuration;

        CardSmeltPresenter presenter;
        UICardRestorePointInfo restorePointView;
        bool autoSaveRestorePoint;
#if !USE_SELECT_VIEW
        bool isContinuousSmelt;
#endif

        protected override void OnInit()
        {
            presenter = new CardSmeltPresenter();

#if USE_SELECT_VIEW
            cardSmeltMaterialSelectView.OnSmelt += OnSmeltCardSmeltMaterialSelectView;
#endif
            cardSmeltView.OnCancel += OnBack;
            cardSmeltView.OnSmelt += OnSmelt;
            cardSmeltView.OnContinuousSmelt += OnContinuousSmelt;
            cardSmeltView.OnAutoSaveRestorePointToggle += OnAutoSaveRestorePointToggle;
            cardSmeltProgressView.OnStopSmelt += OnStopSmelt;
            cardSmeltProgressView.OnFinishProgress += OnFinishProgress;
            cardSmeltWaringView.OnConfirm += OnSmeltConfirm;
            presenter.OnUpdateView += UpdateView;
            presenter.OnUpdateGoodsZeny += UpdateZeny;
            presenter.OnSmeltProgress += SmeltProgress;
            presenter.OnSmeltResultEffect += OnSmeltResultEffect;
            presenter.OnSmeltResult += SmeltResult;
#if USE_SELECT_VIEW
            presenter.OnSelectMaterials += OnSelectMaterials;
#else
            presenter.OnShowWarning += OnShowWarning;
#endif
            presenter.OnCheckRestorePoint += OnCheckRestorePoint;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
#if USE_SELECT_VIEW
            cardSmeltMaterialSelectView.OnSmelt -= OnSmeltCardSmeltMaterialSelectView;
#endif
            cardSmeltView.OnCancel -= OnBack;
            cardSmeltView.OnSmelt -= OnSmelt;
            cardSmeltView.OnContinuousSmelt -= OnContinuousSmelt;
            cardSmeltView.OnAutoSaveRestorePointToggle -= OnAutoSaveRestorePointToggle;
            cardSmeltProgressView.OnStopSmelt -= OnStopSmelt;
            cardSmeltProgressView.OnFinishProgress -= OnFinishProgress;
            cardSmeltWaringView.OnConfirm -= OnSmeltConfirm;
            presenter.OnUpdateView -= UpdateView;
            presenter.OnUpdateGoodsZeny -= UpdateZeny;
            presenter.OnSmeltProgress -= SmeltProgress;
            presenter.OnSmeltResultEffect -= OnSmeltResultEffect;
            presenter.OnSmeltResult -= SmeltResult;
#if USE_SELECT_VIEW
            presenter.OnSelectMaterials -= OnSelectMaterials;
#else
            presenter.OnShowWarning -= OnShowWarning;
#endif
            presenter.OnCheckRestorePoint -= OnCheckRestorePoint;

            presenter.RemoveEvent();

            if (restorePointView != null)
                restorePointView.SetHandler(null);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.SetCardInfo(data as CardItemInfo);
            cardSmeltMaterialSelectView.Hide();
            cardSmeltView.Show();
            cardSmeltProgressView.Hide();
            cardSmeltResultView.Hide();
            cardSmeltWaringView.Hide();
            autoSaveRestorePoint = presenter.GetAutoSaveRestorePointSetting();
            cardSmeltView.SetAutoSaveRestorePoint(autoSaveRestorePoint);
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._18500; // 카드 제련
        }

        protected override void OnBack()
        {
            // 연속 제련 진행중 백버튼
            if (cardSmeltProgressView.IsShow)
            {
                presenter.StopContinuousSmeltCard();
                return;
            }

#if USE_SELECT_VIEW
            // 재료 선택 팝업
            if (cardSmeltMaterialSelectView.IsShow)
            {
                cardSmeltMaterialSelectView.Hide();
                return;
            }
#endif

            // 제련 결과 팝업
            if (cardSmeltResultView.IsShow)
            {
                cardSmeltResultView.Hide();
                return;
            }

            // 경고 팝업 
            if (cardSmeltWaringView.IsShow)
            {
                cardSmeltWaringView.Hide();
                return;
            }

            base.OnBack();
        }

        void UpdateView()
        {
            var info = presenter.info;

            cardSmeltInfoView.Set(info);
            cardSmeltOptionView.Set(info);
            cardSmeltMaterialView.Set(presenter.GetCardSmeltMaterial());
            cardSmeltMaterialView.SetNeedZeny(presenter.NeedZeny);
            UpdateZeny();
        }

        void UpdateZeny()
        {
            cardSmeltMaterialView.SetHaveZeny(presenter.HaveZeny);
            cardSmeltView.SetCanSmelt(presenter.CanSmelt());
        }

        void OnSmelt()
        {
            ItemInfo item = presenter.GetCardSmeltMaterial();

            if (item == null)
                return;

#if !USE_SELECT_VIEW
            isContinuousSmelt = false;
#endif

            if (presenter.IsWaringPopup())
            {
                cardSmeltWaringView.Set(item.IconName, item.ItemCount);
                cardSmeltWaringView.Show();
            }
            else
            {
                OnSmeltConfirm(isConfirm: true);
            }
        }

        void OnSmeltConfirm(bool isConfirm)
        {
#if !USE_SELECT_VIEW
            if (presenter.IsContinuousShowWarning())
            {
                presenter.ConfirmWarning(isConfirm);
                return;
            }

            if (isContinuousSmelt)
            {
                isContinuousSmelt = false;

                if (isConfirm)
                    StartContinousSmelt();

                return;
            }
#endif
            if (!isConfirm)
                return;

            CardItemInfo cardInfo = presenter.info as CardItemInfo;
            if (cardInfo.CardLevel % 10 == 0 && cardInfo.RestorePointLevel < cardInfo.CardLevel)
                ShowCheckRestorePointView(RestoreCheckContext.BeforeNormalSmelt);
            else
                presenter.RequestSmeltCard();
        }

        void OnContinuousSmelt()
        {
#if USE_SELECT_VIEW
            // 보유중인 재료 아이템 세팅
            presenter.ClearMaterials();

            cardSmeltMaterialSelectView.Set(presenter.GetMaterialsSelectSlotInfos());
            cardSmeltMaterialSelectView.SetEnabledBtnSmelt(presenter.IsSmeltMaterials());
            cardSmeltMaterialSelectView.Show();
#else
            ItemInfo item = presenter.GetCardSmeltMaterial();

            if (item == null)
                return;

            isContinuousSmelt = true;

            if (presenter.IsWaringPopup())
            {
                cardSmeltWaringView.Set(item.IconName, item.ItemCount);
                cardSmeltWaringView.Show();
            }
            else
            {
                OnSmeltConfirm(isConfirm: true);
            }
#endif
        }

        private void OnAutoSaveRestorePointToggle(bool value)
        {
            autoSaveRestorePoint = value;
            presenter.SetAutoSaveRestorePointSetting(value);
        }

#if USE_SELECT_VIEW
        void OnSelectMaterials()
        {
            cardSmeltMaterialSelectView.SetEnabledBtnSmelt(presenter.IsSmeltMaterials());
        }

        private void OnSmeltCardSmeltMaterialSelectView()
        {
            CardItemInfo cardInfo = presenter.info as CardItemInfo;
            if (presenter.info.CardLevel % 10 == 0 && cardInfo.RestorePointLevel < cardInfo.CardLevel)
                ShowCheckRestorePointView(RestoreCheckContext.BeforeContinousSmelt);
            else
                StartContinousSmelt();
        }
#endif

        private void StartContinousSmelt()
        {
            cardSmeltView.Hide();
            cardSmeltProgressView.Show();
            presenter.RequestContinuousSmeltCard();
        }

        void SmeltProgress()
        {
            cardSmeltView.Hide();
            cardSmeltProgressView.Show();
            cardSmeltProgressView.StartWaitSmelt();
        }

        /// <summary>
        /// 다음 제련 진행
        /// </summary>
        void OnFinishProgress()
        {
            presenter.NextSmelt();
        }

        /// <summary>
        /// 제련 중지
        /// </summary>
        void OnStopSmelt()
        {
            presenter.StopContinuousSmeltCard();
        }

        /// <summary>
        /// 결과 연출
        /// </summary>
        void OnSmeltResultEffect(bool isSuccess)
        {
            Timing.KillCoroutines(gameObject);

            if (isSuccess)
            {
                Timing.RunCoroutine(YieldSuccessFx(), gameObject);
            }
            else
            {
                Timing.RunCoroutine(YieldFailFx(), gameObject);
            }
        }

        IEnumerator<float> YieldSuccessFx()
        {
            var optionCollDiff = (presenter.AfterItem as CardItemInfo).GetCardBattleOptionCollection();
            var optionCollBefore = (presenter.BeforeItem as CardItemInfo).GetCardBattleOptionCollection().GetEnumerator();
            float diffRate = 0;

            foreach (var each in optionCollDiff)
            {
                optionCollBefore.MoveNext();
                long nextMaxValue = optionCollBefore.Current.nextMaxValue;
                long nextMinValue = optionCollBefore.Current.nextMinValue;
                long diff = each.serverValue;

                if (diff > 0)
                {
                    if (nextMaxValue > nextMinValue)
                        diffRate = (float)(diff - nextMinValue) / (nextMaxValue - nextMinValue);
                    else
                        diffRate = 1.0f;
                }
            }

            cardSmeltInfoView.ShowFx(true, diffRate);
            cardSmeltOptionView.SetFx(presenter.BeforeItem, presenter.AfterItem);

            yield return Timing.WaitForSeconds(fxSuccessDuration);
            presenter.FinishResultEffect();
        }

        IEnumerator<float> YieldFailFx()
        {
            cardSmeltInfoView.ShowFx(false);

            yield return Timing.WaitForSeconds(fxFailDuration);
            presenter.FinishResultEffect();
        }

        /// <summary>
        /// 제련 결과
        /// </summary>
        void SmeltResult()
        {
            UpdateView();
            cardSmeltView.Show();
            cardSmeltProgressView.Hide();
            cardSmeltResultView.Show();
            cardSmeltResultView.Set(presenter.StartItem, presenter.EndItem);
            cardSmeltResultView.SetUseGoods(presenter.GetUseMaterials(), presenter.UseZeny());
        }

        void OnShowWarning()
        {
            ItemInfo item = presenter.GetCardSmeltMaterial();

            if (item == null)
                return;

            cardSmeltWaringView.Set(item.IconName, item.ItemCount);
            cardSmeltWaringView.Show();
        }

        private void OnCheckRestorePoint()
        {
            // 연속 강화 연출 도중에 복원점 팝업 체크
            ShowCheckRestorePointView(RestoreCheckContext.DuringContinousSmelt);
        }

        private void ShowCheckRestorePointView(RestoreCheckContext context)
        {
            if (autoSaveRestorePoint)
            {
                RestoreViewResultHandler(context, true);
            }
            else
            {
                restorePointView = UI.Show<UICardRestorePointInfo>(new UICardRestorePointInfo.Input()
                {
                    mode = UICardRestorePointInfo.Mode.RestorePointSave,
                    cardItem = presenter.info as CardItemInfo,
                    resultHandler = RestoreViewResultHandler,
                    userCustomValue = context
                });
            }
        }

        private void RestoreViewResultHandler(RestoreCheckContext context, bool result)
        {
            restorePointView = null;

            if (result)
            {
                if (context == RestoreCheckContext.BeforeNormalSmelt)
                {
                    presenter.SaveRestoreCheckPoint();
                    presenter.RequestSmeltCard();
                }
                else if (context == RestoreCheckContext.BeforeContinousSmelt) // 연속 강화를 하려고 하는데, 복원 지점 확인 팝업을 먼저 보여준 경우
                {
                    StartContinousSmelt();
                    presenter.SaveRestoreCheckPoint(); // StartContinousSmelt 를 하면 현재 복원 단계가 초기화되므로 다시 저장해준다.
                }
                else if (context == RestoreCheckContext.DuringContinousSmelt)
                {
                    presenter.SaveRestoreCheckPoint();
                    presenter.OnUserAgreeToSaveCheckPoint();
                }
            }
            else
            {
                if (context == RestoreCheckContext.DuringContinousSmelt)
                    presenter.StopContinuousSmeltCard();
            }
        }
    }
}
