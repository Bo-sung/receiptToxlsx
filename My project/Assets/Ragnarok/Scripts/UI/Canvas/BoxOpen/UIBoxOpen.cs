using AnimationOrTween;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBoxOpen : UICanvas, BoxOpenPresenter.IView, IAutoInspectorFinder
    {
        public class Input : IUIData
        {
            public readonly RewardPacket[] rewards;
            public readonly string boxIconName;

            public Input(RewardPacket[] rewards, string boxIconName)
            {
                this.rewards = rewards;
                this.boxIconName = boxIconName;
            }
        }

        private const string ANI_BOX_DROP = "UI_BoxOpen_Drop"; // 떨어지는 애니메이션
        private const string ANI_BOX_OPEN = "UI_BoxOpen_Open"; // 박스 오픈 애니메이션

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIEventTrigger background;
        [SerializeField] Animator animator;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UITextureHelper top;
        [SerializeField] UITextureHelper inside;
        [SerializeField] UITextureHelper bottom;

        BoxOpenPresenter presenter;
        private RewardPacket[] rewards;
        private bool isFinishedAni;
        private bool isTouchScreen;

        [SerializeField] int count = 5;

        protected override void OnInit()
        {
            presenter = new BoxOpenPresenter(this);

            presenter.AddEvent();
            EventDelegate.Add(background.onClick, CloseUI);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.onClick, CloseUI);

            if (presenter != null)
                presenter = null;

            Timing.KillCoroutines(gameObject); // 코르틴 삭제
            UI.RewardInfo(rewards); // 보상 팝업

            rewards = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            Input input = data as Input;

            if (input != null)
                rewards = input.rewards;

            if (rewards == null)
            {
                CloseUI();
                return;
            }

            const string defaultTop = "Ui_Texture_Box_Top_Item";
            const string defaultInside = "Ui_Texture_Box_Inside_Item";
            const string defaultBottom = "Ui_Texture_Box_Bottom_Item";

            string topName = defaultTop;
            string insideName = defaultInside;
            string bottomName = defaultBottom;

            if (input.boxIconName != null)
            {
                topName = string.Concat("Ui_Texture_Box_Top_", input.boxIconName);
                insideName = string.Concat("Ui_Texture_Box_Inside_", input.boxIconName);
                bottomName = string.Concat("Ui_Texture_Box_Bottom_", input.boxIconName);
            }

            top.SetBox(topName, isAsync: false);
            inside.SetBox(insideName, isAsync: false);
            bottom.SetBox(bottomName, isAsync: false);

            if (top.mainTexture == null)
                top.SetBox(defaultTop);

            if (inside.mainTexture == null)
                inside.SetBox(defaultInside);

            if (bottom.mainTexture == null)
                bottom.SetBox(defaultBottom);

            Timing.RunCoroutine(ShowReward(), Segment.RealtimeUpdate, gameObject);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Refresh()
        {
        }

        private void CloseUI()
        {
            UI.Close<UIBoxOpen>();
        }

        IEnumerator<float> ShowReward()
        {
            isTouchScreen = false;

            for (int i = 0; i < rewards.Length; i++)
            {
                RewardData rewardData = new RewardData(rewards[i].rewardType, rewards[i].rewardValue, rewards[i].rewardCount, rewards[i].rewardOption);
                rewardHelper.SetData(rewardData);

                PlayAnimation(i == 0 ? ANI_BOX_DROP : ANI_BOX_OPEN); // Drop
                yield return Timing.WaitUntilTrue(IsFinishedAni);
            }

            isTouchScreen = true;
        }

        private void PlayAnimation(string clipName)
        {
            isFinishedAni = false;

            // Play
            ActiveAnimation aa = ActiveAnimation.Play(animator, clipName, Direction.Forward, EnableCondition.DoNothing, DisableCondition.DoNotDisable);

            if (aa == null)
            {
                isFinishedAni = true;
                return;
            }

            EventDelegate.Add(aa.onFinished, OnFinishedAni);
        }

        void OnFinishedAni()
        {
            isFinishedAni = true;
        }

        private bool IsFinishedAni()
        {
            return isFinishedAni;
        }

        protected override void OnBack()
        {
            if (!isTouchScreen)
                return;

            CloseUI();
        }

        string GetBackSpriteName(int rating)
        {
            switch (rating)
            {
                case 1: return "Ui_Common_BG_Item_01";
                case 2: return "Ui_Common_BG_Item_02";
                case 3: return "Ui_Common_BG_Item_03";
                case 4: return "Ui_Common_BG_Item_04";
                case 5: return "Ui_Common_BG_Item_05";
            }
            return default;
        }
    }
}