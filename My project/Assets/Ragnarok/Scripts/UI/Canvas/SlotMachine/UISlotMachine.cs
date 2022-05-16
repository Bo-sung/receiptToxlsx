using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISlotMachine : UICanvas
    {
        protected override UIType uiType => UIType.Destroy;

        private readonly string TAG = nameof(UISlotMachine);

        [SerializeField] TweenHeight twHeight;
        [SerializeField] SlotMachineSpinView[] spinView;
        [SerializeField] UIButtonHelper btnStart;

        bool isPulled;

        protected override void OnInit()
        {
            EventDelegate.Add(btnStart.OnClick, OnClickedBtnStart);
        }

        protected override void OnClose()
        {
            isPulled = false;
            Timing.KillCoroutines(TAG);

            EventDelegate.Remove(btnStart.OnClick, OnClickedBtnStart);
            btnStart.IsEnabled = true;
            twHeight.Finish();
            twHeight.tweenFactor = 0f;
        }

        protected override void OnShow(IUIData data = null)
        {
            SkillData[] totalSkillData = SkillDataManager.Instance.GetArray();

            spinView[0].SetData(GetRandomArray(totalSkillData, count: 30));
            spinView[1].SetData(GetRandomArray(totalSkillData, count: 30));
            spinView[2].SetData(GetRandomArray(totalSkillData, count: 30));
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnClickedBtnStart()
        {
            if (isPulled)
                return;

            isPulled = true;

            btnStart.IsEnabled = false;

            Timing.RunCoroutine(YieldPlay(), TAG);
        }

        private IEnumerator<float> YieldPlay()
        {
            twHeight.PlayForward();
            yield return Timing.WaitForSeconds(0.25f);


            spinView[0].Play();
            yield return Timing.WaitForSeconds(0.2f);
            spinView[1].Play();
            yield return Timing.WaitForSeconds(0.2f);
            spinView[2].Play();

            yield return Timing.WaitForSeconds(4f);
            UI.Close<UISlotMachine>();
        }

        private SkillData[] GetRandomArray(SkillData[] datas, int count)
        {
            SkillData[] cloneArray = datas.Clone() as SkillData[];
            cloneArray.Shuffle();
            List<SkillData> ret = new List<SkillData>();
            for (int i = 0; i < count; ++i)
            {
                ret.Add(cloneArray[i]);
            }
            return ret.ToArray();
        }
    }
}