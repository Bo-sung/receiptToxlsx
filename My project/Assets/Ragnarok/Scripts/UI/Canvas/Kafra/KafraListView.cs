using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class KafraListView : UIView, IInspectorFinder
    {
        [SerializeField] private UILabelHelper labelTitle;
        [SerializeField] private UILabelHelper labelDesc;
        [SerializeField] private UIKafraListElement[] elements;
        [SerializeField] private UILabelHelper labelResultTitle;
        [SerializeField] private UITextureHelper iconTotalResult;
        [SerializeField] private UILabelHelper labelTotalResult;
        [SerializeField] private UIButtonhWithGrayScale btnAccept;

        public event Action OnAccept;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnAccept.OnClick, OnClickedBtnAccept);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnAccept.OnClick, OnClickedBtnAccept);
        }

        protected override void OnLocalize()
        {
            labelResultTitle.LocalKey = LocalizeKey._19109; // 퀘스트 보상
            btnAccept.LocalKey = LocalizeKey._19110; // 수락
        }

        private void OnClickedBtnAccept()
        {
            OnAccept?.Invoke();
        }

        public void Set(KafraType kafraType, UIKafraListElement.IInput[] arrayData)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (i >= arrayData.Length)
                    continue;

                elements[i].SetData(arrayData[i]);

                // 보상 아이콘 세팅
                iconTotalResult.SetItem(arrayData[i].Result.IconName);
            }
            UpdateRewardCount(0);

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
        }

        public void UpdateRewardCount(int count)
        {
            labelTotalResult.Text = count.ToString("N0");
            btnAccept.SetMode(count == 0 ? UIGraySprite.SpriteMode.Grayscale : UIGraySprite.SpriteMode.None);
            btnAccept.IsEnabled = count != 0;
        }

        bool IInspectorFinder.Find()
        {
            elements = transform.GetComponentsInChildren<UIKafraListElement>();
            return true;
        }
    }
}