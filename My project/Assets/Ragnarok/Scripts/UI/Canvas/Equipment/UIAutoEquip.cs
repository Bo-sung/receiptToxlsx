using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIAutoEquip : UICanvas<AutoEquipPresenter>, AutoEquipPresenter.IView, IInspectorFinder, TurotialJobChange.IAutoEquipImpl
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UILabelHelper labelEquip; // 바로 장착 라벨
        [SerializeField] UILabelHelper labelPower; // 전투력 변동치
        [SerializeField] UIProgressBar prgProgressBar; // 시간제한 프로그레스 바
        [SerializeField] GameObject goActive;
        [SerializeField] UIButtonHelper btnEquip;
        [SerializeField] UIButtonHelper btnClose;

        float timerElapsedTime;

        public bool IsShowEquip { get; private set; }

        protected override void OnInit()
        {
            presenter = new AutoEquipPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnEquip.OnClick, OnClickedBtnEquip);
            EventDelegate.Add(btnClose.OnClick, presenter.PutNextItem);

            timerElapsedTime = 0;
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnEquip.OnClick, OnClickedBtnEquip);
            EventDelegate.Remove(btnClose.OnClick, presenter.PutNextItem);
        }

        protected override void OnHide()
        {
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {
            labelEquip.LocalKey = LocalizeKey._6100; // 바로 장착
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.UpdateRecommendingItem();
        }

        public void Refresh()
        {
            ItemInfo recommendItem = presenter.GetRecommendItem();
            if (recommendItem == null)
            {
                if (IsVisible)
                    Hide();

                return;
            }

            if (!IsVisible)
                Show();

            equipmentProfile.SetData(recommendItem);

            int attackPowerDiff = presenter.GetAttackPowerDifference();
            labelPower.Text = GetPowerDifferenceText(attackPowerDiff);

            Timing.KillCoroutines(gameObject);

            if (isTutorialMode)
            {
                // Do Nothing
            }
            else
            {
                Timing.RunCoroutine(YieldTimer(Constants.AutoEquip.VIEW_TIMER_DURATION), gameObject);
            }
        }

        /// <summary>
        /// 뷰를 띄우는 제한 시간 타이머.
        /// </summary>
        /// <param name="duration">뷰 띄우는 제한 시간 (단위: 초)</param>
        IEnumerator<float> YieldTimer(float duration)
        {
            while (true)
            {
                timerElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(timerElapsedTime / duration);
                prgProgressBar.value = (1f - t);

                if (t == 1f)
                {
                    break;
                }

                yield return Timing.WaitForSeconds(0.5f);
                timerElapsedTime += 0.5f;
            }

            presenter.PutNextItem();
        }

        /// <summary>
        /// 전투력 변동치 텍스트 반환
        /// </summary>
        private string GetPowerDifferenceText(int diff)
        {
            if (diff < 0)
                return $"▼ {diff}";
            return $"▲ {diff}";
        }

        void AutoEquipPresenter.IView.ResetTimer()
        {
            timerElapsedTime = 0f;
            //Refresh();
        }

        void OnClickedBtnEquip()
        {
            isClickedBtnEquip = true;
            presenter.OnClickedBtnEquip();
        }

        #region Tutorial

        bool isTutorialMode;
        bool isClickedBtnEquip;

        void TurotialJobChange.IAutoEquipImpl.SetTutorialMode(bool isTutorialMode)
        {
            this.isTutorialMode = isTutorialMode;

            if (isTutorialMode)
            {
                // 자동장착 타이머 Stop
                Timing.KillCoroutines(gameObject);
                prgProgressBar.value = 1f;
            }
        }

        UIWidget TurotialJobChange.IAutoEquipImpl.GetBackgroundWidget()
        {
            return goActive.GetComponentInChildren<UIWidget>();
        }

        bool TurotialJobChange.IAutoEquipImpl.IsClickedBtnEquip()
        {
            if (isClickedBtnEquip)
            {
                isClickedBtnEquip = false;
                return true;
            }

            return false;
        }

        #endregion
    }
}