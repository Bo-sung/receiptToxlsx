using UnityEngine;

namespace Ragnarok.View
{
    public sealed class MyGuildBattleRankView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIGuildBattleMyRankElement element;
        [SerializeField] UILabelHelper labelNoData;

        SuperWrapContent<UIGuildBattleMyRankElement, UIGuildBattleMyRankElement.IInput> wrapContent;

        public event System.Action<UIGuildBattleMyRankElement.IInput> OnSelectInfo;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIGuildBattleMyRankElement, UIGuildBattleMyRankElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelectBtnInfo += OnSelectBtnInfo;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelectBtnInfo -= OnSelectBtnInfo;
            }
        }

        protected override void OnLocalize()
        {
            labelNoData.LocalKey = LocalizeKey._47009; // 랭킹 정보가 없습니다.
        }

        void OnSelectBtnInfo(UIGuildBattleMyRankElement.IInput input)
        {
            OnSelectInfo?.Invoke(input);
        }

        public void SetData(UIGuildBattleMyRankElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);

            int length = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(length == 0);
        }
    }
}