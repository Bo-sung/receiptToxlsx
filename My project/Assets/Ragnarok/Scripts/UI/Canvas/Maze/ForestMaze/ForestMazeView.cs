using UnityEngine;

namespace Ragnarok.View
{
    public class ForestMazeView : UIView, IInspectorFinder
    {
        public interface IInput
        {
            int GetNameKey(int index);
            int GetDescKey(int index);
        }

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelValue recommendPower;
        [SerializeField] UIIconLabelValue[] infos;
        [SerializeField] UIButtonHelper btnFree;
        [SerializeField] UIItemCostButtonHelper btnEnter;
        [SerializeField] UILabelHelper labelNotice;

        public event System.Action OnSelectEnter;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnFree.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnFree.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._39601; // 미궁숲
            recommendPower.TitleKey = LocalizeKey._48705; // 권장 전투력
            btnFree.LocalKey = LocalizeKey._39602; // 무료 입장
            btnEnter.LocalKey = LocalizeKey._39603; // 입장
            labelNotice.LocalKey = LocalizeKey._39604; // 입장권은 자정(GMT+8)에 초기화됩니다.
        }

        void OnClickedBtnEnter()
        {
            OnSelectEnter?.Invoke();
        }

        public void Initialize(IInput input, string needItemIcon)
        {
            if (input != null)
            {
                for (int i = 0; i < infos.Length; i++)
                {
                    infos[i].TitleKey = input.GetNameKey(i);
                    infos[i].ValueKey = input.GetDescKey(i);
                }
            }

            btnEnter.SetItemIcon(needItemIcon);
        }

        /// <summary>
        /// 전투력 세팅
        /// </summary>
        public void SetRecommandPower(int recommandPower)
        {
            recommendPower.Value = recommandPower.ToString("N0");
        }

        /// <summary>
        /// 무료입장 수 업데이트
        /// </summary>
        public void SetFreeEntryCount(int freeEntryCount)
        {
            bool canFreeEntry = freeEntryCount > 0; // 무료 입장 가능
            btnFree.SetActive(canFreeEntry);
            btnEnter.SetActive(!canFreeEntry);
        }

        /// <summary>
        /// 입장권 수 업데이트
        /// </summary>
        public void SetTicketCount(int ticketCount)
        {
            btnEnter.SetItemCount(ticketCount);
        }

        bool IInspectorFinder.Find()
        {
            infos = GetComponentsInChildren<UIIconLabelValue>();
            return true;
        }
    }
}