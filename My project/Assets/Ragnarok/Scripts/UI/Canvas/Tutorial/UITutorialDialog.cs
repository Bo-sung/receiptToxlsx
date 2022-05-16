using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UITutorialDialog : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIWidget container;
        [SerializeField] UITextureHelper npcImage;
        [SerializeField] UILabelHelper labelNpcName, labelTalk;
        [SerializeField] TypewriterEffect typewriterEffect;
        [SerializeField] GameObject goNext;
        [SerializeField] float nextDelay = 0.4f;

        GameObject myGameObject;

        private bool isNextReady;
        private bool isFinishedTypewriterEffect;
        private bool? forcedNextActivation = null;

        void Awake()
        {
            myGameObject = gameObject;

            EventDelegate.Add(typewriterEffect.onFinished, OnFinishedTypewriterEffect);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(typewriterEffect.onFinished, OnFinishedTypewriterEffect);
            StopAllCoroutine();
        }

        void OnFinishedTypewriterEffect()
        {
            isFinishedTypewriterEffect = true;
            Timing.RunCoroutine(YieldShowNext(nextDelay), gameObject);
        }

        public bool Finish()
        {
            if (isNextReady && isFinishedTypewriterEffect)
                return false;

            typewriterEffect.Finish();
            return true;
        }

        public void Show(string dialog, UIWidget.Pivot pivot)
        {
            SetActive(true);

            // 완료가 되기 전에 시작하려 할 때
            if (!isFinishedTypewriterEffect)
            {
                typewriterEffect.Finish(); // 강제 완료
                StopAllCoroutine(); // 코루틴 중지
            }

            container.pivot = pivot;
            labelTalk.Text = dialog;

            StopAllCoroutine();
            SetActiveNext(false);
            typewriterEffect.ResetToBeginning();
            isFinishedTypewriterEffect = false;
        }

        public void SetNpc(Npc npc)
        {
            SetNpc(npc.nameLocalKey.ToText(), npc.imageName);
        }

        public void SetNpc(string name, string textureName)
        {
            labelNpcName.Text = name;
            npcImage.SetNPC(textureName);
        }

        public void Hide()
        {
            isFinishedTypewriterEffect = true;
            isNextReady = true;

            SetActive(false);
            StopAllCoroutine();
        }

        private void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }

        IEnumerator<float> YieldShowNext(float delay)
        {
            if (delay > 0f)
                yield return Timing.WaitForSeconds(delay);

            SetActiveNext(true);
        }

        private void SetActiveNext(bool isActive)
        {
            isNextReady = isActive;

            if (forcedNextActivation.HasValue)
                NGUITools.SetActive(goNext, forcedNextActivation.Value);
            else
                NGUITools.SetActive(goNext, isActive);
        }

        private void StopAllCoroutine()
        {
            Timing.KillCoroutines(gameObject);
        }

        public void ForceNextActivation(bool? value)
        {
            forcedNextActivation = value;

            if (value.HasValue)
                NGUITools.SetActive(goNext, value.Value);
            else
                NGUITools.SetActive(goNext, isNextReady);
        }
    }
}