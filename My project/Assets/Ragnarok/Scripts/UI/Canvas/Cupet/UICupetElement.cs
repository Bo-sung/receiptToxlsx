using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="CupetListView"/>
    /// </summary>
    public class UICupetElement : UIElement<UICupetElement.IInput>
    {
        public interface IInput
        {
            CupetModel CupetModel { get; }
            bool IsNotice { get; } // 소환 || 진화 가능시 빨콩
        }

        [SerializeField] UICupetProfile cupetProfile;
        [SerializeField] UIWidget iconNotice;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabelHelper labelProgress;
        [SerializeField] UIButtonHelper btnSelect;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            cupetProfile.SetData(info.CupetModel);

            int count = info.CupetModel.Count;

            if (info.CupetModel.IsMaxRank())
            {
                progressBar.value = 1f;
                labelProgress.Text = count.ToString();
            }
            else
            {
                int needCount;
                if (info.CupetModel.IsInPossession)
                {
                    needCount = info.CupetModel.GetNeedEvolutionPieceCount(); // 진화에 필요한 몬스터조각 수
                }
                else
                {
                    needCount = info.CupetModel.GetNeedSummonPieceCount(); // 소환에 필요한 몬스터조각 수
                }

                progressBar.value = MathUtils.GetProgress(count, needCount);
                labelProgress.Text = StringBuilderPool.Get()
                    .Append('(').Append(count).Append('/').Append(needCount).Append(')')
                    .Release();
            }

            iconNotice.enabled = info.IsNotice;
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.CupetModel.CupetID);
        }
    }
}