using UnityEngine;

namespace Ragnarok.View
{
    public class SimpleFreeFightHelpView : UIView, IInspectorFinder
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UILabelValue labelFreeFightHelp;
        [SerializeField] UIGrid gridTime;
        [SerializeField] UILabelHelper[] labelTimes;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UIFreeFightReward first, second;
        [SerializeField] UIFreeFightReward[] freeFightRewards;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnConfirm.OnClick, Hide);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnConfirm.OnClick, Hide);
        }

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._49512; // 라운드 종료 시 처치 수에 따라 이벤트 보상이 우편함으로 지급됩니다.
            labelRewardTitle.LocalKey = LocalizeKey._49513; // 보상
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        public void SetText(string mainTitle, string title, string desc)
        {
            labelMainTitle.Text = mainTitle;
            labelFreeFightHelp.Title = title;
            labelFreeFightHelp.Value = desc;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void InitializeTime(string[] arrTimeText)
        {
            // Times
            int timeLength = arrTimeText == null ? 0 : arrTimeText.Length;
            for (int i = 0; i < labelTimes.Length; i++)
            {
                if (i < timeLength)
                {
                    labelTimes[i].SetActive(true);
                    labelTimes[i].Text = arrTimeText[i];
                }
                else
                {
                    labelTimes[i].SetActive(false);
                }
            }

            gridTime.Reposition();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void InitializeReward(UIFreeFightReward.IInput[] arrReward)
        {
            // Rewards
            int rewardLength = arrReward == null ? 0 : arrReward.Length;
            first.SetData(rewardLength > 0 ? arrReward[0] : null); // 첫번째보상
            second.SetData(rewardLength > 1 ? arrReward[1] : null); // 두번째보상

            int lastRewardLength = rewardLength - 2; // 나머지 보상
            for (int i = 0; i < freeFightRewards.Length; i++)
            {
                freeFightRewards[i].SetData(i < lastRewardLength ? arrReward[i + 2] : null);
            }
        }

        public virtual bool Find()
        {
            if (gridTime)
            {
                labelTimes = gridTime.GetComponentsInChildren<UILabelHelper>();
            }

            freeFightRewards = GetComponentsInChildren<UIFreeFightReward>();

#if UNITY_EDITOR
            if (first)
                UnityEditor.ArrayUtility.Remove(ref freeFightRewards, first);

            if (second)
                UnityEditor.ArrayUtility.Remove(ref freeFightRewards, second);
#endif
            return true;
        }
    }
}