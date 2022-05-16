using UnityEngine;

namespace Ragnarok.View
{
    public class JobPreview : UIView
    {
        [SerializeField] UILabelHelper label1st;
        [SerializeField] UILabelHelper label2st;

        [Header("1차 직업")]
        [SerializeField] UIButtonHelper btnSwordman;
        [SerializeField] UIButtonHelper btnArcher;
        [SerializeField] UIButtonHelper btnMagician;
        [SerializeField] UIButtonHelper btnThief;

        [Header("2차 직업")]
        [SerializeField] UIButtonHelper btnKnight;
        [SerializeField] UIButtonHelper btnCrusader;
        [SerializeField] UIButtonHelper btnDancer;
        [SerializeField] UIButtonHelper btnHunter;
        [SerializeField] UIButtonHelper btnSage;
        [SerializeField] UIButtonHelper btnWizard;
        [SerializeField] UIButtonHelper btnRogue;
        [SerializeField] UIButtonHelper btnAssassin;

        public event System.Action<Job> OnSelectJob;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnSwordman.OnClick, OnClickedBtnSwordman);
            EventDelegate.Add(btnArcher.OnClick, OnClickedBtnArcher);
            EventDelegate.Add(btnMagician.OnClick, OnClickedBtnMagician);
            EventDelegate.Add(btnThief.OnClick, OnClickedBtnThief);
            EventDelegate.Add(btnKnight.OnClick, OnClickedBtnKnight);
            EventDelegate.Add(btnCrusader.OnClick, OnClickedBtnCrusader);
            EventDelegate.Add(btnDancer.OnClick, OnClickedBtnDancer);
            EventDelegate.Add(btnHunter.OnClick, OnClickedBtnHunter);
            EventDelegate.Add(btnSage.OnClick, OnClickedBtnSage);
            EventDelegate.Add(btnWizard.OnClick, OnClickedBtnWizard);
            EventDelegate.Add(btnRogue.OnClick, OnClickedBtnRogue);
            EventDelegate.Add(btnAssassin.OnClick, OnClickedBtnAssassin);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnSwordman.OnClick, OnClickedBtnSwordman);
            EventDelegate.Remove(btnArcher.OnClick, OnClickedBtnArcher);
            EventDelegate.Remove(btnMagician.OnClick, OnClickedBtnMagician);
            EventDelegate.Remove(btnThief.OnClick, OnClickedBtnThief);
            EventDelegate.Remove(btnKnight.OnClick, OnClickedBtnKnight);
            EventDelegate.Remove(btnCrusader.OnClick, OnClickedBtnCrusader);
            EventDelegate.Remove(btnDancer.OnClick, OnClickedBtnDancer);
            EventDelegate.Remove(btnHunter.OnClick, OnClickedBtnHunter);
            EventDelegate.Remove(btnSage.OnClick, OnClickedBtnSage);
            EventDelegate.Remove(btnWizard.OnClick, OnClickedBtnWizard);
            EventDelegate.Remove(btnRogue.OnClick, OnClickedBtnRogue);
            EventDelegate.Remove(btnAssassin.OnClick, OnClickedBtnAssassin);
        }

        protected override void OnLocalize()
        {
            label1st.LocalKey = LocalizeKey._2002; // 1차 전직
            label2st.LocalKey = LocalizeKey._2003; // 2차 전직
        }

        void OnClickedBtnSwordman()
        {
            OnSelectJob?.Invoke(Job.Swordman);
        }

        void OnClickedBtnArcher()
        {
            OnSelectJob?.Invoke(Job.Archer);
        }

        void OnClickedBtnMagician()
        {
            OnSelectJob?.Invoke(Job.Magician);
        }

        void OnClickedBtnThief()
        {
            OnSelectJob?.Invoke(Job.Thief);
        }

        void OnClickedBtnKnight()
        {
            OnSelectJob?.Invoke(Job.Knight);
        }

        void OnClickedBtnCrusader()
        {
            OnSelectJob?.Invoke(Job.Crusader);
        }

        void OnClickedBtnDancer()
        {
            OnSelectJob?.Invoke(Job.Dancer);
        }

        void OnClickedBtnHunter()
        {
            OnSelectJob?.Invoke(Job.Hunter);
        }

        void OnClickedBtnSage()
        {
            OnSelectJob?.Invoke(Job.Sage);
        }

        void OnClickedBtnWizard()
        {
            OnSelectJob?.Invoke(Job.Wizard);
        }

        void OnClickedBtnRogue()
        {
            OnSelectJob?.Invoke(Job.Rogue);
        }

        void OnClickedBtnAssassin()
        {
            OnSelectJob?.Invoke(Job.Assassin);
        }
    }
}