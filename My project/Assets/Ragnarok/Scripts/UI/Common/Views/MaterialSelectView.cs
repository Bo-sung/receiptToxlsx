using UnityEngine;

namespace Ragnarok.View
{
    public abstract class MaterialSelectView : UIView
    {
        [SerializeField] UILabelValue needPoint;
        [SerializeField] UILabelHelper labelWarning;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] DarkTreeMaterialElement element;
        [SerializeField] protected UILabelHelper labelNoData;
        [SerializeField] protected UILabelHelper labelNotice;

        private SuperWrapContent<DarkTreeMaterialElement, DarkTreeMaterialElement.IInput> wrapContent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<DarkTreeMaterialElement, DarkTreeMaterialElement.IInput>(element);
        }

        protected override void OnLocalize()
        {
            needPoint.TitleKey = LocalizeKey._9020; // 필요 포인트
            labelWarning.LocalKey = LocalizeKey._9037; // (초과 포인트가 존재합니다.)
        }

        public void SetData(DarkTreeMaterialElement.IInput[] inputs)
        {
            int dataSize = inputs == null ? 0 : inputs.Length;
            wrapContent.SetData(inputs);
            labelNoData.SetActive(dataSize == 0);
        }

        public abstract void UpdatePoint(int curPoint);

        protected void SetNeedPoint(int point)
        {
            needPoint.Value = point.ToString("N0"); // 필요 포인트 세팅
        }

        protected void SetActiveWarning(bool isActive)
        {
            labelWarning.SetActive(isActive);
        }
    }
}