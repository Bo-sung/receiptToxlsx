using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBossComing : UICanvas, IAutoInspectorFinder
    {
        public interface IInput
        {
            int GetMonsterId();
            string GetDescription();
            string GetSpriteName();
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override bool IsVisible => base.IsVisible && animationBase.gameObject.activeSelf;

        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper labelBossName;
        [SerializeField] UILabelHelper labelBossName_1;
        [SerializeField] UITextureHelper bossImage;
        [SerializeField] Animator animationBase;
        [SerializeField] UISprite bossRareTypeIcon;

        float timer = 0;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data)
        {
            animationBase.gameObject.SetActive(false);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void ShowBoss(IInput input)
        {
            animationBase.gameObject.SetActive(true);

            animationBase.Play("UI_BossComing");
            var monster = MonsterDataManager.Instance.Get(input.GetMonsterId());

            labelDesc.Text = input.GetDescription();

            var bossName = monster.name_id.ToText();
            labelBossName.Text = bossName;
            labelBossName_1.Text = bossName;
            bossImage.SetBoss(string.Concat("UI_Texture_Boss_", monster.prefab_name), isAsync: false); // 애니메이션이 Active를 제어하기 때문에 isAsync 를 false 처리
            bossRareTypeIcon.spriteName = input.GetSpriteName();

            timer = 5;
        }

        private void Update()
        {
            timer -= Time.deltaTime;

            if (timer > 0 || !animationBase.gameObject.activeSelf)
                return;

            animationBase.gameObject.SetActive(false);
        }
    }
}