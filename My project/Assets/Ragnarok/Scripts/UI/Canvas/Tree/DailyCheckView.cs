using UnityEngine;

namespace Ragnarok
{
    public sealed class DailyCheckView : UISubCanvas<DailyCheckPresenter>, IInspectorFinder
    {
        [SerializeField] DailyCheckSlot[] slots;

        DailyCheckInfo[] arrayInfo;

        protected override void OnInit()
        {
            arrayInfo = presenter.GetDailyCheckInfos();
        }

        protected override void OnClose()
        {

        }

        protected override void OnShow()
        {
            bool isNewDaily = presenter.IsNewDaily;

            int day = presenter.DailyCount;
            for (int i = 0; i < arrayInfo.Length; i++)
            {
                slots[i].SetData(presenter, arrayInfo[i]);

                if (isNewDaily)
                {
                    slots[i].SetActiveComplete(i + 1 < day);
                    slots[i].SetFxCompleteBase(i + 1 == day);
                }
                else
                {
                    slots[i].SetActiveComplete(i < day);
                }
            }

            presenter.SetNewDaily(false);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        bool IInspectorFinder.Find()
        {
            slots = transform.GetComponentsInChildren<DailyCheckSlot>();
            return true;
        }
    }
}
