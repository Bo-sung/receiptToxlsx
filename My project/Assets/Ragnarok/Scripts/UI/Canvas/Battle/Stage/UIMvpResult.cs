using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMvpResult : UICanvas
    {
        private class MvpReward : UIMvpRewardItem.IInput
        {
            private System.Action onUpdate;
            event System.Action UIMvpRewardItem.IInput.OnUpdate
            {
                add { onUpdate += value; }
                remove { onUpdate -= value; }
            }

            private readonly RewardData rewardData;
            private readonly bool isWasted;
            private bool isShow = false;

            public MvpReward(RewardData rewardData, bool isWasted)
            {
                this.rewardData = rewardData;
                this.isWasted = isWasted;
            }

            bool UIMvpRewardItem.IInput.IsShow()
            {
                return isShow;
            }

            void UIMvpRewardItem.IInput.SetShow(bool isShow)
            {
                this.isShow = isShow;
                onUpdate?.Invoke();
            }

            RewardData UIMvpRewardItem.IInput.GetRewardData()
            {
                return rewardData;
            }

            bool UIMvpRewardItem.IInput.IsWasted()
            {
                return isWasted;
            }
        }

        public enum Mode
        {
            MVP,
            TimePatrolBoss,
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        const int VIEW_COUNT = 5;

        [SerializeField] UIButton btnClose;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIEventTrigger middle;
        [SerializeField] float waitTime = 0.3f;
        [SerializeField] GameObject goMVP;

        List<UIMvpRewardItem.IInput> dataList;
        private bool isFinished;

        public List<EventDelegate> onClose = new List<EventDelegate>();

        protected override void OnInit()
        {
            dataList = new List<UIMvpRewardItem.IInput>();
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnClose.onClick, OnBack);
            EventDelegate.Add(middle.onClick, OnBack);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClose.onClick, OnBack);
            EventDelegate.Remove(middle.onClick, OnBack);

            KillAllCoroutine();
        }

        protected override void OnShow(IUIData data)
        {
        }

        protected override void OnHide()
        {
            dataList.Clear();

            UIMvpRewardItem[] items = wrapper.GetComponentsInChildren<UIMvpRewardItem>();
            foreach (var item in items)
            {
                item.Release(isLaunch: true, destination: UIRewardLauncher.GoodsDestination.Basic);
            }

            KillAllCoroutine();
        }

        protected override void OnLocalize()
        {
        }

        public void SetMode(Mode mode)
        {
            goMVP.SetActive(mode == Mode.MVP);
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIMvpRewardItem ui = go.GetComponent<UIMvpRewardItem>();
            ui.SetData(dataList[index]);
        }

        public void SetData(RewardData[] rewards, RewardData[] wasted)
        {
            KillAllCoroutine();

            if (rewards != null)
            {
                foreach (var item in rewards)
                {
                    dataList.Add(new MvpReward(item, isWasted: false));
                }
            }

            if (wasted != null)
            {
                foreach (var item in wasted)
                {
                    dataList.Add(new MvpReward(item, isWasted: true));
                }
            }

            wrapper.Resize(dataList.Count);
            Timing.RunCoroutine(YieldShowListEffect(), gameObject);
        }

        IEnumerator<float> YieldShowListEffect()
        {
            isFinished = false;
            labelNotice.SetActive(false);

            wrapper.SetProgress(0f);
            yield return Timing.WaitForSeconds(waitTime);

            for (int i = 0; i < dataList.Count; i++)
            {
                if (i >= VIEW_COUNT)
                {
                    wrapper.SetProgress(((i + 1) - VIEW_COUNT) / (float)(dataList.Count - VIEW_COUNT));
                }

                dataList[i].SetShow(true);
                yield return Timing.WaitForSeconds(waitTime);
            }

            Finish();
        }

        private void Finish()
        {
            KillAllCoroutine();

            for (int i = 0; i < dataList.Count; i++)
            {
                dataList[i].SetShow(true);
            }

            isFinished = true;

            Timing.RunCoroutine(YieldAutoHide(), gameObject);
        }

        IEnumerator<float> YieldAutoHide()
        {
            labelNotice.SetActive(true);

            labelNotice.LocalKey = LocalizeKey._48400; // 닫으려면 터치하세요
            int index = 5;
            do
            {
                if (index <= 3)
                {
                    labelNotice.Text = LocalizeKey._48401.ToText() // {SECONDS}초 후 닫힙니다.
                        .Replace(ReplaceKey.SECONDS, index);
                }

                yield return Timing.WaitForSeconds(1f);
            } while (--index > 0);

            HideUI();
        }

        private void HideUI()
        {
            UI.Close<UIMvpResult>();
            EventDelegate.Execute(onClose);
        }

        private void KillAllCoroutine()
        {
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnBack()
        {
            if (!isFinished)
                return;

            HideUI();
        }
    }
}