using UnityEngine;

namespace Ragnarok.View
{
    public class CardSmeltInfoView : UIView
    {
        [SerializeField] UICardProfileBase cardProfile;
        [SerializeField] UIGridHelper gridRate;
        [SerializeField] UILabelHelper labelCardName;
        [SerializeField] UILabelHelper labelSuccessRate;
        [SerializeField] GameObject fxSmeltGreatSuccess;
        [SerializeField] GameObject fxSmeltSuccess;
        [SerializeField] GameObject fxSmeltFail;
        [SerializeField] GameObject smeltResultTextPaneltRoot;
        [SerializeField] Animation smeltResultAnim;
        [SerializeField] UILabel labelResult;
        [SerializeField] UILabel labelResultGlow;

        private void OnEnable()
        {
            smeltResultTextPaneltRoot.SetActive(false);
            fxSmeltGreatSuccess.SetActive(false);
            fxSmeltSuccess.SetActive(false);
            fxSmeltFail.SetActive(false);
        }

        protected override void OnLocalize()
        {
            
        }

        public void Set(ItemInfo info)
        {           
            cardProfile.SetData(info);
            gridRate.SetValue(info.Rating);
            labelCardName.Text = info.Name;

            if (info.SuccessRate > 0.5f)
            {
                labelSuccessRate.Text = LocalizeKey._18505.ToText() // [4C4A4D]강화 성공 확률 : [-][63A1EE]{VALUE}%[-]
                    .Replace(ReplaceKey.VALUE, (info.SuccessRate * 100).ToString("0.##"));
            }
            else
            {
                labelSuccessRate.Text = LocalizeKey._18506.ToText() // [4C4A4D]강화 성공 확률 : [-][FF0000]{VALUE}%[-]
                    .Replace(ReplaceKey.VALUE, (info.SuccessRate * 100f).ToString("0.##"));
            }
        }

        public void ShowFx(bool isSuccess, float rate = 0f)
        {
            int intVal = MathUtils.ToInt(rate * 100);
            bool isGreat = intVal >= 70;
            
            fxSmeltGreatSuccess.SetActive(false);
            fxSmeltSuccess.SetActive(false);
            fxSmeltFail.SetActive(false);

            if (isSuccess)
            {
                smeltResultTextPaneltRoot.SetActive(true);
                if (isGreat)
                    fxSmeltGreatSuccess.SetActive(true);
                else
                    fxSmeltSuccess.SetActive(true);

                labelResult.text = StringBuilderPool.Get()
                    .Append(intVal).Append("%")
                    .Release();

                Color resultColor = GetResultColor(isGreat);
                labelResult.color = resultColor;
                labelResultGlow.color = resultColor;
            }
            else
            {
                smeltResultTextPaneltRoot.SetActive(false);
                fxSmeltFail.SetActive(true);
            }

            smeltResultAnim.Stop();
            smeltResultAnim.Play();
        }

        private Color GetResultColor(bool isGreat)
        {
            return isGreat ? new Color32(128, 216, 245, 255) : new Color32(226, 187, 63, 255);
        }
    }
}