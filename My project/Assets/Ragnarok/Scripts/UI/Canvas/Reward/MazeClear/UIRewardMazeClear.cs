using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRewardMazeClear : UICanvas
    {
        public class Input : IUIData
        {
            public RewardData[] rewardDatas;
            public string title, description;

            public Input(RewardData[] rewardDatas, string title, string description)
            {
                this.rewardDatas = rewardDatas;
                this.title = title;
                this.description = description;
            }
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UIGridHelper grid;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabelHelper labelTitle, labelDescription;
        [SerializeField] UILabelHelper labelTouch;

        Input input;

        protected override void OnInit()
        {
            EventDelegate.Add(btnClose.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClose.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is Input input)
            {
                this.input = input;
            }
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTouch.LocalKey = LocalizeKey._47102; // 아무곳이나 터치하세요
        }

        void CloseUI()
        {
            UI.Close<UIRewardMazeClear>();
        }

        void Refresh()
        {
            if (input == null)
            {
                CloseUI();
                return;
            }

            labelTitle.Text = input.title;
            labelDescription.Text = input.description;

            int count = input.rewardDatas.Length;
            grid.SetValue(count);
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < count ? input.rewardDatas[i] : null);
            }
        }
    }
}