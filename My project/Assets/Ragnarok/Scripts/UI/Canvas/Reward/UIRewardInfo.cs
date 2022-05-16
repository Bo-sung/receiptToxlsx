using UnityEngine;

namespace Ragnarok
{
    public class UIRewardInfoData : IUIData
    {
        public readonly RewardInfo[] infos;

        public UIRewardInfoData(RewardInfo[] infos)
        {
            this.infos = infos;
        }
    }

    public sealed class UIRewardInfo : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnExit;

        UIRewardInfoData Data;

        bool isClickedBtnConfirm;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is UIRewardInfoData)
            {
                Data = data as UIRewardInfoData;
            }

            UpdateView();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._9011; // 보상 목록
            btnConfirm.LocalKey = LocalizeKey._20001; // 확인
        }

        private void UpdateView()
        {
            wrapper.Resize(Data.infos.Length);
            wrapper.SetProgress(0);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIRewardInfoSlot ui = go.GetComponent<UIRewardInfoSlot>();
            ui.SetData(Data.infos[index]);
        }

        void CloseUI()
        {
            UI.Close<UIRewardInfo>();
            isClickedBtnConfirm = true;
        }

        public UIWidget GetBtnConfirm()
        {
            return btnConfirm.GetComponent<UIWidget>();
        }

        public bool IsClickedBtnConfirm()
        {
            if (isClickedBtnConfirm)
            {
                isClickedBtnConfirm = false;
                return true;
            }

            return false;
        }
    }
}