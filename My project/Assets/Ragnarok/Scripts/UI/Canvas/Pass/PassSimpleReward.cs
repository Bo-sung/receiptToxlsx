using UnityEngine;

namespace Ragnarok.View
{
    public sealed class PassSimpleReward : MonoBehaviour
    {
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] GameObject goComplete;
        [SerializeField] GameObject goNotice;
        [SerializeField] GameObject goLock;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void SetData(RewardData data)
        {
            if (data == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            rewardHelper.SetData(data);
        }

        public void SetComplete(bool isActive)
        {
            goComplete.SetActive(isActive);
        }

        public void SetNotice(bool isActive)
        {
            goNotice.SetActive(isActive);
        }

        public void SetLock(bool isActive)
        {
            goLock.SetActive(isActive);
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }
    }
}