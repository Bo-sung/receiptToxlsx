using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class NPCStyle : MonoBehaviour, IAutoInspectorFinder
    {
        private const string ANI_NAME = "UI_TextBalloon";

        [SerializeField] NPCConfig config;
        [SerializeField] UISprite eye;
        [SerializeField] float eyeAnimationTime;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelTalk;
        [SerializeField] Animator animator;

        private int lastTalkIndex = -1;

        void OnDestroy()
        {
            Timing.KillCoroutines(gameObject);
        }

        void OnEnable()
        {
            PlayTalk();
        }

        void OnDisable()
        {
            Timing.KillCoroutines(gameObject);
        }

        public void SetConfig(NPCConfig config)
        {
            this.config = config;
            SetStyle();
        }

        void OnLocalize()
        {
            if (config == null)
                return;

            // 이름
            if (labelName)
                labelName.Text = $"[{config.NPCName}]";
        }

        public void SetStyle()
        {
            if (config == null)
                return;            

            // NPC 눈
            if (eye)
            {                
                Timing.RunCoroutine(AnimationEye(), gameObject);
            }

            // 이름
            if (labelName)
                labelName.Text = $"[{config.NPCName}]";

            // 대사
            PlayTalk();           
        }

        IEnumerator<float> AnimationEye()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(Random.Range(0, eyeAnimationTime));
                eye.enabled = false;
                yield return Timing.WaitForSeconds(0.1f);
                eye.enabled = true;
            }
        }

        public void PlayTalk()
        {
            if (labelTalk == null)
                return;

            OnLocalize();

            int[] npcTalkIDs = config.GetNpcTalkIDs();
            int length = npcTalkIDs.Length;
            int randNum = Random.Range(0, length);

            if (length >= 2)
                while(randNum == lastTalkIndex)
                    randNum = Random.Range(0, length);

            lastTalkIndex = randNum;
            labelTalk.Text = npcTalkIDs[randNum].ToText();
            if(animator)
                animator.Play(ANI_NAME, 0, 0f);
        }       
    }
}