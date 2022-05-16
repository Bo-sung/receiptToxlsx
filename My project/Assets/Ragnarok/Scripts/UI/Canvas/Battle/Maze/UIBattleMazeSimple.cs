using UnityEngine;

namespace Ragnarok.View.BattleMaze
{
    public class UIBattleMazeSimple : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UITexture thumbnail;
        [SerializeField] UILabel labelScore;
        [SerializeField] UILabel labelDescription;
        [SerializeField] UIAniProgressBar hp;
        [SerializeField] UILabel labelName;

        [SerializeField] UIPlayTween tweenHit;
        [SerializeField] UIPlayTween tweenScore;
        [SerializeField] UIPlayTween tweenDescription;
        [SerializeField] UIPlayTween attack, defense, change, buff, debuff, shield, curse, recovery;

        public void Show(Texture2D thumbnail, string name)
        {
            this.thumbnail.mainTexture = thumbnail;
            labelName.text = name;
        }

        public void SetHp(int cur, int max)
        {
            hp.Set(cur, max);
        }

        public void TweenHp(int cur, int max)
        {
            hp.Tween(cur, max);
        }

        public void SetScore(int score)
        {
            labelScore.text = score.ToString("N0");
            tweenScore.Play();
        }

        public void SetDescription(string text)
        {
            labelDescription.text = text;
            tweenDescription.Play();
        }

        public void PlayHit()
        {
            tweenHit.Play();
        }

        public void PlayAttack()
        {
            attack.Play();
        }

        public void PlayDefense()
        {
            defense.Play();
        }

        public void PlayChange()
        {
            change.Play();
        }

        public void PlayBuff()
        {
            buff.Play();
        }

        public void PlayDebuff()
        {
            debuff.Play();
        }

        public void PlayShield()
        {
            shield.Play();
        }

        public void PlayCurse()
        {
            curse.Play();
        }

        public void PlayRecovery()
        {
            recovery.Play();
        }

        bool IInspectorFinder.Find()
        {
            if (thumbnail)
                tweenHit = thumbnail.GetComponent<UIPlayTween>();

            if (labelScore)
                tweenScore = labelScore.GetComponent<UIPlayTween>();

            if (labelDescription)
                tweenDescription = labelDescription.GetComponent<UIPlayTween>();

            return true;
        }
    }
}