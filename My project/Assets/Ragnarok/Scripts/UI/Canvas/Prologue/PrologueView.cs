using MEC;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace Ragnarok.View
{
    public class PrologueView : UIView
    {
        [SerializeField] private UISprite barmund;
        [SerializeField] private UISprite novice_M;
        [SerializeField] private UISprite novice_F;
        [SerializeField] private UISprite novice_Face_M;
        [SerializeField] private UISprite novice_Face_F;

        private UISprite novice;
        private UISprite novice_Face;

        [SerializeField] private UISprite background;
        [SerializeField] private UIButton btnNext;
        [SerializeField] private UISprite backgroundTalk;
        [SerializeField] private GameObject nextDeco;

        [SerializeField] private UILabelHelper labelName;
        [SerializeField] private UILabelHelper labelTalk;
        [SerializeField] private TypewriterEffect typewriterTalk;
        [SerializeField] UILabelHelper labelNext;

        private string talkerColumnName;
        private string textColumnName;

        private bool isProceeding = false;
        private bool isNextStep = false;
        private bool isTypewriting = false;

        public event System.Action<ProloguePresenter.TalkerType> OnShowPopup;
        public event System.Action OnHideView;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(typewriterTalk.onFinished, FinishTypewriter);
            EventDelegate.Add(btnNext.onClick, OnClickedBtnNext);

            SetGender(false, "", "");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(typewriterTalk.onFinished, FinishTypewriter);
            EventDelegate.Remove(btnNext.onClick, OnClickedBtnNext);

            KillAllCoroutines();
        }

        public override void Hide()
        {
            base.Hide();

            KillAllCoroutines();
        }

        protected override void OnLocalize()
        {
            labelNext.LocalKey = LocalizeKey._923; // NEXT
        }

        public void Show(DataTable dt, bool isFirst = false)
        {
            base.Show();

            background.alpha = isFirst ? 1f : 0.5f;

            isNextStep = false;

            KillAllCoroutines();

            ActiveSprite(novice_Face, isFirst);
            Timing.RunCoroutine(YieldDialog(dt, isFirst), gameObject);
        }

        public void SetGender(bool isMale, string talkerName, string textName)
        {
            if (isMale)
            {
                ActiveSprite(novice_F, false); // 안쓰는 성별은 비활성화

                novice = novice_M;
                novice_Face = novice_Face_M;
            }
            else
            {
                ActiveSprite(novice_M, false);

                novice = novice_F;
                novice_Face = novice_Face_F;
            }

            talkerColumnName = talkerName;
            textColumnName = textName;
        }

        private void FinishTypewriter()
        {
            isTypewriting = false;

            nextDeco.SetActive(true);
            isProceeding = false;
        }

        private void OnClickedBtnNext()
        {
            if (!isProceeding)
            {
                isNextStep = true;
            }
            else
            {
                if (isTypewriting) typewriterTalk.Finish();
            }
        }

        private bool IsNextStep()
        {
            if (isNextStep)
            {
                isNextStep = false;
                return true;
            }

            return false;
        }

        public void ShowNextDialog()
        {
            OnClickedBtnNext();
        }

        private IEnumerator<float> YieldDialog(DataTable dt, bool isFirst = false)
        {
            nextDeco.SetActive(false);

            DataRow dr;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dr = dt.Rows[i];
                switch (dr[talkerColumnName].ToString().ToEnum<ProloguePresenter.TalkerType>())
                {
                    case ProloguePresenter.TalkerType.SelectPopup:
                        OnShowPopup.Invoke(ProloguePresenter.TalkerType.SelectPopup);
                        break;
                    case ProloguePresenter.TalkerType.GainPopup:
                        OnShowPopup.Invoke(ProloguePresenter.TalkerType.GainPopup);
                        break;
                    default: // 다이얼로그..
                        isTypewriting = true;

                        SetDialog(dr[talkerColumnName].ToString().ToEnum<ProloguePresenter.TalkerType>());
                        yield return Timing.WaitForOneFrame;

                        labelTalk.Text = dr[textColumnName].ToString();
                        typewriterTalk.ResetToBeginning();

                        nextDeco.SetActive(false);
                        break;
                }

                yield return Timing.WaitUntilTrue(IsNextStep);
            }

            OnHideView?.Invoke();
        }

        private void SetDialog(ProloguePresenter.TalkerType talker)
        {
            isProceeding = true;
            labelTalk.Text = "";

            // 말풍선 이미지
            backgroundTalk.spriteName = getDialogBoxName(talker);

            // 텍스트
            switch (talker)
            {
                case ProloguePresenter.TalkerType.AloneUser:
                case ProloguePresenter.TalkerType.User:
                    labelName.Text = string.Format("[5599ED][{0}]", LocalizeKey._903.ToText());// 나
                    break;

                case ProloguePresenter.TalkerType.AloneNPC:
                case ProloguePresenter.TalkerType.NPC:
                    labelName.Text = string.Format("[5A575B][{0}]", LocalizeKey._904.ToText());// 의문의 남자
                    break;

                default:
                    labelName.Text = "";
                    break;
            }

            // 캐릭터 이미지
            switch (talker)
            {
                case ProloguePresenter.TalkerType.OnlyText:
                    ActiveSprite(novice, false);
                    ActiveSprite(barmund, false);
                    break;

                case ProloguePresenter.TalkerType.AloneUser:
                    ActiveSprite(novice);
                    ActiveSprite(barmund, false);
                    break;

                case ProloguePresenter.TalkerType.AloneNPC:
                    ActiveSprite(novice);
                    ActiveSprite(barmund, false);
                    break;

                case ProloguePresenter.TalkerType.User:
                    ActiveSprite(novice);
                    ActiveSprite(barmund, isGray: true);
                    break;

                case ProloguePresenter.TalkerType.NPC:
                    ActiveSprite(novice, isGray: true);
                    ActiveSprite(barmund);
                    break;
            }
        }

        private string getDialogBoxName(ProloguePresenter.TalkerType talker)
        {
            switch (talker)
            {
                case ProloguePresenter.TalkerType.AloneUser:
                case ProloguePresenter.TalkerType.User:
                    return "Ui_Local_Prologue_Balloon_L";

                case ProloguePresenter.TalkerType.AloneNPC:
                case ProloguePresenter.TalkerType.NPC:
                    return "Ui_Local_Prologue_Balloon_R";

                default:
                    return "Ui_Local_Prologue_Balloon_C";
            }
        }

        private void ActiveSprite(UISprite spt, bool isActive = true, bool isGray = false)
        {
            if (isActive)
                spt.color = isGray ? Color.gray : Color.white;

            spt.cachedGameObject.SetActive(isActive);
        }

        private void KillAllCoroutines()
        {
            Timing.KillCoroutines(gameObject);
        }
    }
}