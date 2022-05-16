using UnityEngine;

namespace Ragnarok.View
{
    public sealed class RpsResultView : UIView
    {
        public interface IInput
        {
            string GetPoringImage(RpsResultType type);
            int GetDialogueTextId(RpsResultType type);
            RewardData GetRewardData(RpsResultType type);
        }

        [SerializeField] UISprite iconResult;
        [SerializeField] UITextureHelper texture;
        [SerializeField] UILabelHelper labelDialogue;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnConfirm.OnClick, Hide);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnConfirm.OnClick, Hide);
        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        public void Show(IInput input, RpsResultType type)
        {
            if (input == null)
                return;

            Show();

            iconResult.spriteName = GetResultIconName(type);
            iconResult.MakePixelPerfect();
            texture.SetEvent(input.GetPoringImage(type));
            labelDialogue.LocalKey = input.GetDialogueTextId(type);
            const int WIN_LOCAL_KEY = LocalizeKey._11208; // 획득 보상
            const int LOSE_LOCAL_KEY = LocalizeKey._11209; // 재도전 해보세요.
            labelDescription.LocalKey = type == RpsResultType.Ready ? WIN_LOCAL_KEY : LOSE_LOCAL_KEY;
            rewardHelper.SetData(input.GetRewardData(type));
        }

        string GetResultIconName(RpsResultType type)
        {
            switch (type)
            {
                case RpsResultType.Ready:
                    return "Ui_Common_Icon_Result_Win";

                case RpsResultType.Draw:
                    return "Ui_Common_Icon_Result_Draw";

                //case RpsResultType.Defeat:
                default:
                    return "Ui_Common_Icon_Result_Lose";
            }
        }
    }
}