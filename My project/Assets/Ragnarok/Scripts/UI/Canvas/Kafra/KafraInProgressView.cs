using UnityEngine;

namespace Ragnarok.View
{
    public class KafraInProgressView : UIView, IInspectorFinder
    {
        [SerializeField] private UILabelHelper labelTitle;
        [SerializeField] private UILabelHelper labelDesc;
        [SerializeField] private UIKafraInProgressElement[] elements;
        [SerializeField] private UILabelHelper labelResultTitle;
        [SerializeField] private UITextureHelper iconTotalResult;
        [SerializeField] private UILabelHelper labelTotalResult;
        [SerializeField] private UIButtonhWithGrayScale btnInProgress;

        protected override void OnLocalize()
        {
            labelResultTitle.LocalKey = LocalizeKey._19109; // 퀘스트 보상
            btnInProgress.LocalKey = LocalizeKey._19111; // 진행중
        }

        public void Set(KafraType kafraType, UIKafraInProgressElement.IInput[] arrayData)
        {
            int totalRewardCount = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                if (i >= arrayData.Length)
                    continue;

                totalRewardCount += arrayData[i].GetRewardCount();

                elements[i].SetData(arrayData[i]);

                // 보상 아이콘 세팅
                iconTotalResult.SetItem(arrayData[i].Result.IconName);
            }
            labelTotalResult.Text = totalRewardCount.ToString("N0");

            if (kafraType == KafraType.RoPoint)
            {
                labelTitle.LocalKey = LocalizeKey._19103; // 귀금속 전달
                labelDesc.LocalKey = LocalizeKey._19107; // "귀금속 전달"에 사용할 아이템을 선택해 주세요.
            }
            else if (kafraType == KafraType.Zeny)
            {
                labelTitle.LocalKey = LocalizeKey._19105; // 긴급! 도움 요청
                labelDesc.LocalKey = LocalizeKey._19108; // "긴급! 도움 요청"에 사용할 아이템을 선택해 주세요.
            }

            btnInProgress.SetMode(UIGraySprite.SpriteMode.Grayscale);
            btnInProgress.IsEnabled = false;
        }

        bool IInspectorFinder.Find()
        {
            elements = transform.GetComponentsInChildren<UIKafraInProgressElement>();
            return true;
        }
    }
}