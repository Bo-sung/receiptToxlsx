using UnityEngine;

namespace Ragnarok
{
    public class UISingleReward : UICanvas
    {
        public static float attackPowerInfoDelay = 0f;

        protected override UIType uiType => UIType.Back | UIType.Hide;

        public enum Mode
        {
            JOB_REWARD,
            GUIDE_QUEST_REWARD,
            COSTUME_REWARD,
            JUST_REWARD,
            DICE_REWARD,
            DICE_COMPLETE_REWARD,
            WORD_COLLECTION_COMPLETE_REWARD,
        }

        public class Input : IUIData
        {
            public Mode mode;
            public RewardData rewardData;
            public string iconName;

            public Input(Mode mode, RewardData rewardData, string iconName = "")
            {
                this.mode = mode;
                this.rewardData = rewardData;
                this.iconName = iconName;
            }
        }

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UITextureHelper itemIcon;
        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnConfirm;

        Input input;

        protected override void OnInit()
        {
            EventDelegate.Add(btnConfirm.OnClick, OnBack);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is Input input)
            {
                this.input = input;
            }
            else
            {
                CloseUI();
                return;
            }

            SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess);

            OnLocalize();
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            if (IsInvalid())
                return;

            switch (input.mode)
            {
                case Mode.JOB_REWARD:
                    labelMainTitle.LocalKey = LocalizeKey._30500; // 전직 보상
                    labelDesc.LocalKey = LocalizeKey._30501; // 전직보상은 우편함에서 확인해주세요.
                    btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
                    break;

                case Mode.GUIDE_QUEST_REWARD:
                    labelMainTitle.LocalKey = LocalizeKey._30503; // 미션 보상
                    //labelDesc.Text = string.Empty;
                    btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
                    Refresh();
                    break;

                case Mode.COSTUME_REWARD:
                    labelMainTitle.LocalKey = LocalizeKey._30504; // 코스튬 해금
                    labelDesc.LocalKey = LocalizeKey._30505; // 코스튬이 해금되었습니다.
                    labelItemName.LocalKey = LocalizeKey._30506; // 코스튬
                    itemIcon.Set(input.iconName);
                    btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
                    break;

                case Mode.JUST_REWARD:
                    labelMainTitle.LocalKey = LocalizeKey._11101; // 룰렛 보상
                    labelDesc.Text = string.Empty;
                    labelItemName.Text = GetItemNameWithCount();
                    itemIcon.Set(input.iconName);
                    btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
                    break;

                case Mode.DICE_REWARD:
                    labelMainTitle.LocalKey = LocalizeKey._11310; // 주사위 보상
                    labelDesc.Text = string.Empty;
                    labelItemName.Text = GetItemNameWithCount();
                    itemIcon.Set(input.iconName);
                    btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
                    break;

                case Mode.DICE_COMPLETE_REWARD:
                    labelMainTitle.LocalKey = LocalizeKey._11311; // 완주 보상
                    labelDesc.Text = string.Empty;
                    labelItemName.Text = GetItemNameWithCount();
                    itemIcon.Set(input.iconName);
                    btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
                    break;

                case Mode.WORD_COLLECTION_COMPLETE_REWARD:
                    labelMainTitle.LocalKey = LocalizeKey._7404; // 완성 보상
                    labelDesc.Text = string.Empty;
                    labelItemName.Text = GetItemNameWithCount();
                    itemIcon.Set(input.iconName);
                    btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
                    break;
            }
        }

        private string GetItemNameWithCount()
        {
            return StringBuilderPool.Get().Append(input.rewardData.ItemName).Append(" x").Append(input.rewardData.Count).Release();
        }

        public void Refresh()
        {
            if (IsInvalid())
                return;

            RewardData reward = input.rewardData;

            if (reward is null)
                return;

            itemIcon.Set(reward.IconName);
            if (reward.Count > 1)
            {
                labelItemName.Text = $"{reward.ItemName} x{reward.Count}";
            }
            else
            {
                labelItemName.Text = reward.ItemName;
            }

            if (input.mode == Mode.GUIDE_QUEST_REWARD)
            {
                labelDesc.Text = reward.GetDescription();
            }

            if (input.mode == Mode.JUST_REWARD || input.mode == Mode.DICE_REWARD || input.mode == Mode.DICE_COMPLETE_REWARD)
            {
                labelItemName.Text = GetItemNameWithCount();
                itemIcon.Set(input.iconName);
            }
        }


        private bool IsInvalid()
        {
            return (input == null);
        }

        private void CloseUI()
        {
            UI.Close<UISingleReward>();
        }
    }
}