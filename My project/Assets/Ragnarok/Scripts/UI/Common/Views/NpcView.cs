using UnityEngine;

namespace Ragnarok.View
{
    public class NpcView : MonoBehaviour, IInspectorFinder
    {
        private const string ANI_NAME = "UI_TextBalloon";

        [SerializeField] UILabelHelper labelName, labelTalk;
        [SerializeField] Animator animator;
        [SerializeField] UILabelHelper labelNotice;

        BetterList<int> talkLocalKeyList;

        void Awake()
        {
            talkLocalKeyList = new BetterList<int>();
        }

        void OnEnable()
        {
            PlayTalk();
        }

        public void ShowNpcName(string name)
        {
            labelName.Text = StringBuilderPool.Get()
                    .Append("[").Append(name).Append("]")
                    .Release();
        }

        public void AddTalkLocalKey(int talkLocalKey)
        {
            talkLocalKeyList.Add(talkLocalKey);
            ShowRandomTalk();
        }

        public void ShowNotice(string notice)
        {
            if (string.IsNullOrEmpty(notice))
            {
                labelNotice.SetActive(false);
            }
            else
            {
                labelNotice.SetActive(true);
                labelNotice.Text = notice;
            }
        }

        public void PlayTalk()
        {
            ShowRandomTalk();

            if (animator)
                animator.Play(ANI_NAME, 0, 0f);
        }

        private void ShowRandomTalk()
        {
            int talkLocalKeySize = talkLocalKeyList.size;
            if (talkLocalKeySize == 0)
                return;

            int randNum = Random.Range(0, talkLocalKeySize);
            labelTalk.Text = talkLocalKeyList[randNum].ToText();
        }

        bool IInspectorFinder.Find()
        {
            animator = GetComponentInChildren<Animator>();
            return true;
        }
    }
}