using UnityEngine;

namespace Ragnarok.View
{
    public abstract class UIBasePassRewardElement : UIElement<UIBasePassRewardElement.IInput>
    {
        public interface IInput
        {
            int Level { get; }
            int LastPassLevel { get; }
            int CurLevel { get; }
            RewardData[] FreeRewards { get; }
            RewardData[] PayRewards { get; }
            int FreeStep { get; }
            int PayStep { get; }
            bool IsActivePass { get; }
            bool IsReceivePassFree { get; }
            bool IsReceivePassPay { get; }
            int LastRewardCount { get; }
        }

        [SerializeField] GameObject goCover;
        [SerializeField] UIGraySprite icon;
        [SerializeField] protected UILabelHelper labelLevel;
        [SerializeField] GameObject goBackDown;
        [SerializeField] GameObject goProgressUp, goProgressDown;
        [SerializeField] protected PassReward freeReward, passReward;
        [SerializeField] UIWidget blind;

        public event System.Action<(byte passType, int level)> OnSelectReceive; // 1 : 무료 , 2: 유료
        public event System.Action OnSelectBuyExp;

        protected override void Awake()
        {
            base.Awake();

            freeReward.OnSelectReceive += InvokeSelectReceiveFree;
            passReward.OnSelectReceive += InvokeSelectReceivePay;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            freeReward.OnSelectReceive -= InvokeSelectReceiveFree;
            passReward.OnSelectReceive -= InvokeSelectReceivePay;
        }

        protected void InvokeSelectReceiveFree()
        {
            OnSelectReceive?.Invoke((1, info.Level));
        }

        protected void InvokeSelectReceivePay()
        {
            OnSelectReceive?.Invoke((2, info.Level));
        }

        protected void InvokeSelectBuyExp()
        {
            OnSelectBuyExp?.Invoke();
        }

        protected override void Refresh()
        {
            bool isPreLastLevel = info.Level == info.LastPassLevel - 1;
            bool isLastLevel = info.Level == info.LastPassLevel;
            bool isCurLevel = info.Level == info.CurLevel;
            bool isOpenLevel = info.Level <= info.CurLevel;

            goBackDown.SetActive(!isLastLevel);
            goCover.SetActive(!isOpenLevel);
            icon.Mode = isOpenLevel ? UIGraySprite.SpriteMode.None : UIGraySprite.SpriteMode.Grayscale;
            goProgressUp.SetActive(isOpenLevel);
            goProgressDown.SetActive(isOpenLevel && !isCurLevel);

            blind.alpha = isCurLevel && !isLastLevel && !isPreLastLevel ? 1f : 0f; // 트윈 동기화를 위해 알파값으로 처리
            blind.gameObject.SetActive(!isLastLevel); // 마지막 레벨은 액티브를 꺼준다

            RefreshReward(isPreLastLevel, isLastLevel, isCurLevel, isOpenLevel);
        }

        protected virtual void RefreshReward(bool isPreLastLevel, bool isLastLevel, bool isCurLevel, bool isOpenLevel)
        {
            freeReward.SetRewardData(info.FreeRewards);
            passReward.SetRewardData(info.PayRewards);
            labelLevel.Text = info.Level.ToString();

            if (isOpenLevel)
            {
                freeReward.SetBtnReceice(!info.IsReceivePassFree);
                freeReward.SetComplete(info.IsReceivePassFree);
                freeReward.SetNotice(!info.IsReceivePassFree);
            }
            else
            {
                freeReward.SetBtnReceice(false);
                freeReward.SetComplete(false);
                freeReward.SetNotice(false);
            }
            freeReward.SetLock(false); // 무료 보상은 잠금 없음

            // 유료 보상
            if (info.IsActivePass)
            {
                if (isOpenLevel)
                {
                    passReward.SetBtnReceice(!info.IsReceivePassPay);
                    passReward.SetComplete(info.IsReceivePassPay);
                    passReward.SetNotice(!info.IsReceivePassPay);
                }
                else
                {
                    passReward.SetBtnReceice(false);
                    passReward.SetComplete(false);
                    passReward.SetNotice(false);
                }
                passReward.SetLock(false);
            }
            else
            {
                passReward.SetBtnReceice(false);
                passReward.SetComplete(false);
                passReward.SetNotice(false);
                passReward.SetLock(true);
            }
        }
    }
}