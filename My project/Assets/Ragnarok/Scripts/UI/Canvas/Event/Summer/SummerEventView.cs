using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UISummerEvent"/>
    /// </summary>
    public class SummerEventView : UIView
    {
        [SerializeField] UISummerEventElement[] elements;
        [SerializeField] UIButtonHelper btnComplete;
        [SerializeField] UIButtonWithIconValueHelper btnSkip;
        [SerializeField] UILabelHelper labelNotice;

        public event System.Action OnSelectBtnComplete;
        public event System.Action OnSelectBtnSkip;
        public event System.Action<int> OnSelectElementBtnComplete;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnComplete.OnClick, OnClickedBtnComplete);
            EventDelegate.Add(btnSkip.OnClick, OnClickedBtnSkip);
            foreach (var item in elements)
            {
                item.OnSelectBtnComplete += InvokeSelectBtnComplete;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnComplete.OnClick, OnClickedBtnComplete);
            EventDelegate.Remove(btnSkip.OnClick, OnClickedBtnSkip);
            foreach (var item in elements)
            {
                item.OnSelectBtnComplete -= InvokeSelectBtnComplete;
            }
        }

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._7301; // 일일 퀘스트를 모두 완료 시 보상을 받을 수 있습니다.
            btnSkip.LocalKey = LocalizeKey._7304; // 획득
        }

        protected override void Start()
        {
            base.Start();

            btnSkip.SetValue(BasisType.DAILY_QUEST_EVENT_SKIP_CAT_COIN.GetInt().ToString());
        }

        public void SetData(UISummerEventElement.IInput[] inputs)
        {
            bool isStandByReward = false;
            int receivedRewardCount = 0;
            bool isConnectionRewardQuest = false; // 냥다래 나무 모든 보상 획득 퀘스트 여부

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].SetData(inputs[i]);
                if (!isStandByReward)
                {
                    if (inputs[i].CompleteType == QuestInfo.QuestCompleteType.StandByReward)
                        isStandByReward = true;
                }

                if (inputs[i].CompleteType == QuestInfo.QuestCompleteType.ReceivedReward)
                    receivedRewardCount++;

                if(!isConnectionRewardQuest)
                {
                    if (inputs[i].QuestType == QuestType.CONNECT_TIME_ALL_REWARD)
                        isConnectionRewardQuest = true;
                }
            }

            // 모든 보상 획득
            if (receivedRewardCount == elements.Length)
            {
                btnSkip.SetActive(false);
                btnComplete.SetActive(true);
                btnComplete.IsEnabled = false;
                btnComplete.LocalKey = LocalizeKey._10014; // 획득완료
            }
            else
            {
                // 획득 가능한 보상 있음
                if (isStandByReward)
                {
                    btnSkip.SetActive(false);
                    btnComplete.SetActive(true);
                    btnComplete.IsEnabled = true;
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                }
                else if (isConnectionRewardQuest) // 냥다래 나무 모든 보상 획득 퀘스트일떄만 냥다래 스킵 가능
                {
                    btnSkip.SetActive(true);
                    btnComplete.SetActive(false);
                }
                else
                {
                    btnSkip.SetActive(false);
                    btnComplete.SetActive(true);
                    btnComplete.IsEnabled = false;
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                }
            }
        }

        void OnClickedBtnComplete()
        {
            OnSelectBtnComplete?.Invoke();
        }

        void OnClickedBtnSkip()
        {
            OnSelectBtnSkip?.Invoke();
        }

        void InvokeSelectBtnComplete(int questId)
        {
            OnSelectElementBtnComplete?.Invoke(questId);
        }
    }
}