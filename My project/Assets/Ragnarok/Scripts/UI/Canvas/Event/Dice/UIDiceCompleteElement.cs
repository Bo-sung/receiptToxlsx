using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIDiceCompleteElement : UIElement<UIDiceCompleteElement.IInput>
    {
        public interface IInput
        {
            int Seq { get; }
            RewardData Reward { get; }
        }

        private static readonly Color NORMAL_TITLE_COLOR = new Color32(0x4C, 0x4A, 0x4D, 0xFF);
        private static readonly Color CAN_RECIEVE_TITLE_COLOR = Color.white;

        [SerializeField] UISprite background;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] GameObject effect;
        [SerializeField] GameObject complete;
        [SerializeField] UIButtonHelper btnReceive;

        private int index;
        private int completeCount;
        private int rewardStep;

        public System.Action OnSelectReceive;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnReceive.OnClick, OnClickedBtnReceive);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnReceive.OnClick, OnClickedBtnReceive);
        }

        protected override void OnLocalize()
        {
            btnReceive.LocalKey = LocalizeKey._11306; // 받기
            UpdateTitle();
        }

        protected override void Refresh()
        {
            UpdateTitle();
            rewardHelper.SetData(info.Reward);

            RefreshState();
        }

        void OnClickedBtnReceive()
        {
            OnSelectReceive?.Invoke();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(int index)
        {
            this.index = index;
        }

        /// <summary>
        /// 완주 정보 세팅
        /// </summary>
        public void SetRewardStep(int completeCount, int rewardStep)
        {
            this.completeCount = completeCount;
            this.rewardStep = rewardStep;
            RefreshState();
        }

        private void UpdateTitle()
        {
            int seq = info == null ? 0 : info.Seq;
            labelTitle.Text = LocalizeKey._11305.ToText() // {COUNT}회
                .Replace(ReplaceKey.COUNT, seq);
        }

        private void RefreshState()
        {
            const string NORMAL_BACKGROUND_NAME = "Ui_Common_BG_Tree_01";
            const string CAN_RECIEVE_BACKGROUND_NAME = "Ui_Common_BG_Tree_02";

            int seq = info == null ? 0 : info.Seq;
            bool hasAlreadyRecieved = index < rewardStep;
            bool isOverSeq = seq <= completeCount;
            bool canRecieve = isOverSeq && index == rewardStep;

            complete.SetActive(hasAlreadyRecieved);
            btnReceive.SetActive(!hasAlreadyRecieved);
            btnReceive.IsEnabled = canRecieve;
            effect.SetActive(canRecieve);
            background.spriteName = isOverSeq ? CAN_RECIEVE_BACKGROUND_NAME : NORMAL_BACKGROUND_NAME;
            labelTitle.IsOutline = isOverSeq;
            labelTitle.Color = isOverSeq ? CAN_RECIEVE_TITLE_COLOR : NORMAL_TITLE_COLOR;
        }
    }
}