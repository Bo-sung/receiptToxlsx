using UnityEngine;

namespace Ragnarok
{
    public class UICupetDetailProfile : UIInfo<ICupetModel>, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelLevelName;
        [SerializeField] UIButtonHelper iconElement;
        [SerializeField] UILabelHelper labelCupetTypeHeader;
        [SerializeField] UILabelHelper labelCupetType;
        [SerializeField] UILabelHelper labelExpHeader;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabelHelper labelProgress;
        [SerializeField] UIProgressBar expProgressBar;
        [SerializeField] UILabelHelper labelExpProgress;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(iconElement.OnClick, ShowElementInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(iconElement.OnClick, ShowElementInfo);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

#if UNITY_EDITOR
            string text = LocalizeKey._19007.ToText() // Lv. {LEVEL} {NAME}
                .Replace(ReplaceKey.LEVEL, info.Level)
                .Replace(ReplaceKey.NAME, info.Name);

            labelLevelName.Text = StringBuilderPool.Get()
                .Append(text) // Lv. {LEVEL} {NAME}
                .Append('(').Append(info.CupetID).Append(')')
                .Release();
#else
            // Lv.1 포링
            labelLevelName.Text = LocalizeKey._19007.ToText() // Lv. {LEVEL} {NAME}
                .Replace(ReplaceKey.LEVEL, info.Level)
                .Replace(ReplaceKey.NAME, info.Name);
#endif

            // 속성타입
            iconElement.SpriteName = info.ElementType.GetIconName();

            // 포지션
            labelCupetTypeHeader.Text = LocalizeKey._19008.ToText(); // 포지션
            labelCupetType.Text = info.CupetType.ToText();

            if (info.IsInPossession)
            {
                // 경험치
                labelExpHeader.Text = LocalizeKey._19009.ToText(); // 경험치

                labelExpProgress.SetActive(true);
                expProgressBar.gameObject.SetActive(true);
                labelProgress.SetActive(false);
                progressBar.gameObject.SetActive(false);

                if (info.IsNeedEvolution)
                {
                    labelExpProgress.LocalKey = LocalizeKey._19039; // 진화 필요
                    expProgressBar.value = 0f;
                }
                else
                {
                    labelExpProgress.Text = ExpDataManager.Instance.GetCupetExpString(info.Exp); // {VALUE}/{MAX} 또는 MAX
                    expProgressBar.value = ExpDataManager.Instance.GetCupetExpPercent(info.Exp);
                }
            }
            else
            {
                labelExpHeader.Text = LocalizeKey._19041.ToText(); // 조각

                labelExpProgress.SetActive(false);
                expProgressBar.gameObject.SetActive(false);
                labelProgress.SetActive(true);
                progressBar.gameObject.SetActive(true);

                int count = info.Count;
                int max = ExpDataManager.Instance.GetNeedSummonPieceCount();

                labelProgress.Text = StringBuilderPool.Get()
                    .Append(count).Append('/').Append(max)
                    .Release();

                progressBar.value = MathUtils.GetProgress(count, max);
            }
        }

        protected void ShowElementInfo()
        {
            UI.Show<UISelectPropertyPopup>().ShowElementView(info.ElementType);
        }
    }
}