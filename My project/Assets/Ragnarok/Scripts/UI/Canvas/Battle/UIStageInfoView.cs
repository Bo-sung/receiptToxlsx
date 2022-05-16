using AnimationOrTween;
using UnityEngine;

namespace Ragnarok
{
    public class UIStageInfoView : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabelHelper labelFieldName;
        [SerializeField] UILabelHelper labelStageName;
        [SerializeField] UITextureHelper bossImage;
        [SerializeField] Animator anim;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Show(string mainText, string subText, string prefabName)
        {
            Show();

            labelFieldName.Text = mainText;
            labelStageName.Text = subText;
            bossImage.SetBoss(string.Concat("UI_Texture_Boss_", prefabName), isAsync: false); // 애니메이션이 Active를 제어하기 때문에 isAsync 를 false 처리

            const string CLIP_NAME = "UIStageInfo";
            ActiveAnimation aa = ActiveAnimation.Play(anim, CLIP_NAME, Direction.Forward, EnableCondition.EnableThenPlay, DisableCondition.DisableAfterForward);
            EventDelegate.Add(aa.onFinished, OnFinishedAnimation, oneShot: true);
        }

        void OnFinishedAnimation()
        {
            Hide();
        }
    }
}