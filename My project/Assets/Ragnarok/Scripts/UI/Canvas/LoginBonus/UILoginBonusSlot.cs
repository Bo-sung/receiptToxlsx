using UnityEngine;

namespace Ragnarok
{
    public class UILoginBonusSlot : MonoBehaviour
    {
        public enum State { Already, New, Not }

        [SerializeField] UILabel dayLabel;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabel rewardLabel;
        [SerializeField] GameObject completeMark;
        [SerializeField] Animator completeMarkAnimator;
        [SerializeField] GameObject newCompleteEffect;

        [SerializeField] GameObject card_FX;

        [SerializeField] UILabelHelper labelRewarded;
        [SerializeField] UILabelHelper labelReward;
        
        public void Init(EventLoginBonusData data, State state)
        {            
            if(data.special == 1) // 4일차 보상 특수 처리
            {
                dayLabel.text = LocalizeKey._3201.ToText().Replace(ReplaceKey.VALUE, data.day); // {VALUE}일차 보상
            }
            else
            {
                dayLabel.text = LocalizeKey._3200.ToText().Replace(ReplaceKey.VALUE, data.day); // {VALUE}일차
            }
            var rewardData = new RewardData(data.reward_type, data.reward_value, data.reward_count);            

            string rewardName = null;

            if (rewardHelper != null)
            {
                rewardHelper.SetData(rewardData);                
            }

            if (rewardData.Count <= 1)
                rewardName = rewardData.ItemName;
            else
                rewardName = string.Format("{0} x {1}", rewardData.ItemName, rewardData.Count);

            rewardLabel.text = rewardName;

            if (completeMark != null)
                completeMark.SetActive(false);
            if (newCompleteEffect != null)
                newCompleteEffect.SetActive(false);
            if (completeMarkAnimator != null)
                completeMarkAnimator.enabled = false;
            if (card_FX != null)
                card_FX.SetActive(true);

            labelRewarded.LocalKey = LocalizeKey._11003; // 획득완료
            labelReward.LocalKey = LocalizeKey._11002; // 보상획득
            labelRewarded.SetActive(false);
            labelReward.SetActive(false);
        }

        public void SetCompleteState(State state)
        {
            if (state == State.Already)
            {
                if (completeMark != null)
                    completeMark.SetActive(true);
                if (card_FX != null)
                    card_FX.SetActive(false);

                labelRewarded.SetActive(true);
            }
            else if (state == State.New)
            {
                if (completeMark != null)
                    completeMark.SetActive(true);
                if (newCompleteEffect != null)
                    newCompleteEffect.SetActive(true);
                if (completeMarkAnimator != null)
                    completeMarkAnimator.enabled = true;
                if (card_FX != null)
                    card_FX.SetActive(false);

                labelRewarded.SetActive(true);
            }
            else if (state == State.Not)
            {
                if (completeMark != null)
                    completeMark.SetActive(false);
                if (card_FX != null)
                    card_FX.SetActive(true);

                labelReward.SetActive(true);
            }
        }
    }
}