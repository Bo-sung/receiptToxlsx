using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TreeBaseView : UISubCanvas<DailyCheckPresenter>, IAutoInspectorFinder
    {
        [SerializeField] UITabHelper tab;
        [SerializeField] CatCoinBaseView catCoinBaseView;
        [SerializeField] ZenyBaseView zenyBaseView;
        [SerializeField] MaterialBaseView materialBaseView;
        [SerializeField] DarkTreeBaseView darkTreeBaseView;
        [SerializeField] UIButtonHelper treeBuff;
        [SerializeField] UISprite backgroundTime;
        [SerializeField] UILabelHelper labelLimitTime;
        [SerializeField] Color32 backgroundTimeDayColor, labelTimeDayColor;
        [SerializeField] Color32 backgroundTimeHourColor, labelTimeHourColor;
        [SerializeField] UIButtonHelper btnTreePack;
        [SerializeField] GameObject goTreePack;

        UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            catCoinBaseView.Initialize(presenter);
            zenyBaseView.Initialize(presenter);
            materialBaseView.Initialize(presenter);
            darkTreeBaseView.Initialize(presenter);

            EventDelegate.Add(tab[0].OnChange, ShowCatCoinBaseView);
            EventDelegate.Add(tab[1].OnChange, ShowZenyBaseView);
            EventDelegate.Add(tab[2].OnChange, ShowMaterialBaseView);
            EventDelegate.Add(tab[3].OnChange, ShowDarkTreeBaseView);
            EventDelegate.Add(btnTreePack.OnClick, presenter.ShowTreePack);
            EventDelegate.Add(treeBuff.OnClick, presenter.ShowTreePack);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(tab[0].OnChange, ShowCatCoinBaseView);
            EventDelegate.Remove(tab[1].OnChange, ShowZenyBaseView);
            EventDelegate.Remove(tab[2].OnChange, ShowMaterialBaseView);
            EventDelegate.Remove(tab[3].OnChange, ShowDarkTreeBaseView);
            EventDelegate.Remove(btnTreePack.OnClick, presenter.ShowTreePack);
            EventDelegate.Remove(treeBuff.OnClick, presenter.ShowTreePack);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnHide() { }

        protected override void OnLocalize()
        {
            tab[0].LocalKey = LocalizeKey._9003; // 냥다래
            tab[1].LocalKey = LocalizeKey._9004; // 제니
            tab[2].LocalKey = LocalizeKey._9005; // 재료
            tab[3].LocalKey = LocalizeKey._9017; // 어둠의 나무
            btnTreePack.LocalKey = LocalizeKey._9015; // 2배
        }

        public void Refresh()
        {
            tab[0].SetNotice(presenter.IsCatCoinReward);
            tab[1].SetNotice(presenter.IsZenyTreeReward);
            tab[2].SetNotice(presenter.IsMaterialTreeReward);
            tab[3].SetNotice(presenter.IsDarkTreeReward);

            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Show();

            bool isActiveTreePack = !(currentSubCanvas is DarkTreeBaseView);
            goTreePack.SetActive(isActiveTreePack);

            Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void ShowCatCoinBaseView()
        {
            ShowSubCanvas(catCoinBaseView);
        }

        private void ShowZenyBaseView()
        {
            ShowSubCanvas(zenyBaseView);
        }

        private void ShowMaterialBaseView()
        {
            ShowSubCanvas(materialBaseView);
        }

        private void ShowDarkTreeBaseView()
        {
            ShowSubCanvas(darkTreeBaseView);
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            if (!UIToggle.current.value)
                return;

            currentSubCanvas = subCanvas;

            HideAllSubCanvas();
            Refresh();
        }

        private void HideAllSubCanvas()
        {
            catCoinBaseView.Hide();
            zenyBaseView.Hide();
            materialBaseView.Hide();
            darkTreeBaseView.Hide();
        }

        /// <summary>
        /// 나무 추가 보상 버프 남은 시간 표시
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldRemainTime()
        {
            while (true)
            {
                float time = presenter.GetTreePackRemainTime();
                if (time <= 0)
                    break;

                UpdateLimitTime(time);
                yield return Timing.WaitForSeconds(0.5f);
            }
            UpdateLimitTime(0);
        }

        private void UpdateLimitTime(float time)
        {
            if (time <= 0)
            {
                treeBuff.SetActive(false);
                btnTreePack.SetActive(true);
                return;
            }

            treeBuff.SetActive(true);
            btnTreePack.SetActive(false);

            // UI 표시에 1분을 추가해서 보여준다.
            TimeSpan span = TimeSpan.FromMilliseconds(time + 60000);

            int totalDays = (int)span.TotalDays;
            bool isDay = totalDays > 0;

            SetLimitTimeColor(isDay);

            if (isDay)
            {
                labelLimitTime.Text = LocalizeKey._8041.ToText().Replace(ReplaceKey.TIME, totalDays); // D-{TIME}
            }
            else
            {
                labelLimitTime.Text = span.ToString(@"hh\:mm");
            }
        }

        private void SetLimitTimeColor(bool isDay)
        {
            if (isDay)
            {
                labelLimitTime.uiLabel.color = labelTimeDayColor;
                backgroundTime.color = backgroundTimeDayColor;
            }
            else
            {
                // 하루 미만으로 남았을 경우 표시
                labelLimitTime.uiLabel.color = labelTimeHourColor;
                backgroundTime.color = backgroundTimeHourColor;
            }
        }
    }
}