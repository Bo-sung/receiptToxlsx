using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public abstract class BasePassView : UIView
    {
        [SerializeField] UILabelHelper labelSeason;
        [SerializeField] protected UILabelHelper labelPassTitle;
        [SerializeField] UILabelHelper labelRemainTime;
        [SerializeField] UIAniProgressBar passExp;
        [SerializeField] UILabelHelper labelPassLevel;
        [SerializeField] UITextureHelper lastRewardIcon;
        [SerializeField] protected UIButtonHelper btnBuyPass;
        [SerializeField] UIButtonHelper btnBattlePassActive;

        public event System.Action OnSelectBuyPass;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnBuyPass.OnClick, OnClickedBtnBuyPass);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnBuyPass.OnClick, OnClickedBtnBuyPass);
        }

        protected override void OnLocalize()
        {
            btnBattlePassActive.LocalKey = LocalizeKey._39816; // 패스 이용 중
        }

        public void SetSeason(int season)
        {
            labelSeason.Text = LocalizeKey._39801.ToText().Replace(ReplaceKey.COUNT, season); // 시즌 {COUNT}
        }

        public void SetRemainTime(RemainTime remainTime)
        {
            Timing.RunCoroutineSingleton(YieldRemainTime(remainTime).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldRemainTime(RemainTime remainTime)
        {
            while (true)
            {
                float time = remainTime.ToRemainTime();

                if (time <= 0)
                    break;

                labelRemainTime.Text = LocalizeKey._39803.ToText().Replace(ReplaceKey.TIME, time.ToStringTimeConatinsDay()); // 남은 시간 : {TIME}
                yield return Timing.WaitForSeconds(0.1f);
            }
            labelRemainTime.Text = LocalizeKey._39803.ToText().Replace(ReplaceKey.TIME, "00:00:00"); // 남은 시간 : {TIME}
        }

        public void SetNextLevel(int level)
        {
            labelPassLevel.Text = level.ToString();
        }

        public void SetLastRewardIconName(string name)
        {
            lastRewardIcon.Set(name);
        }

        protected virtual void TurnOffNextRewardImage()
        {
            labelPassLevel.SetActive(false);
            lastRewardIcon.SetActive(false);
        }

        public virtual void SetIsLastReward(bool isLastReward)
        {
            labelPassLevel.SetActive(!isLastReward);
            lastRewardIcon.SetActive(isLastReward);
        }

        public void SetExp(int cur, int max)
        {
            passExp.Set(cur, max);
        }

        public void SetIsActivePass(bool isActive)
        {
            btnBuyPass.SetActive(!isActive);
            btnBattlePassActive.SetActive(isActive);
        }

        void OnClickedBtnBuyPass()
        {
            OnSelectBuyPass?.Invoke();
        }
    }
}