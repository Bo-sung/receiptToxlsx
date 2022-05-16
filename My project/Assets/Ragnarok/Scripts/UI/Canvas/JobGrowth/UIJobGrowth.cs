using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobGrowth : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;

        [SerializeField] UIScrollBar scrollBar;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject jobGrowthSlot;

        JobGrowthPresenter presenter;
        int listLength;
        int curJobGrowthStateIdx;

        protected override void OnInit()
        {
            presenter = new JobGrowthPresenter();
            
            listLength = System.Enum.GetValues(typeof(JobGrowthState)).Length;
            wrapper.SetRefreshCallback(OnJobGrowSlotRefresh);
            wrapper.SpawnNewList(jobGrowthSlot, 0, 0);

            EventDelegate.Add(btnExit.OnClick, OnClickBtnExit);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, OnClickBtnExit);
        }

        protected override void OnShow(IUIData data = null)
        {
            // 현재 진행중인 상태값
            var state = presenter.GetCurrentGrowthState();
            if (state.HasValue)
            {
                curJobGrowthStateIdx = (int)state.Value;
                RepositionList(curJobGrowthStateIdx);
            }
            else
            {
                curJobGrowthStateIdx = -1;
                RepositionList(listLength - 1);
            }

            wrapper.Resize(listLength);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._2023; // 성장 보상
        }

        void OnJobGrowSlotRefresh(GameObject go, int index)
        {
            UIJobGrowthSlot ui = go.GetComponent<UIJobGrowthSlot>();
            bool activeTween = index == curJobGrowthStateIdx;
            ui.SetData(presenter, index, activeTween);
        }

        void RepositionList(int val)
        {
            int maxIdx = listLength - 1;
            int idx = maxIdx - val; // 10 ~ 0
            float progress = (float)idx / maxIdx;
            
            Timing.RunCoroutineSingleton(YieldReposition(progress).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        IEnumerator<float> YieldReposition(float progress)
        {
            yield return Timing.WaitForSeconds(0.1f);

            scrollBar.value = progress;
        }

        void OnClickBtnExit()
        {
            UI.Close<UIJobGrowth>();
        }
    }
}