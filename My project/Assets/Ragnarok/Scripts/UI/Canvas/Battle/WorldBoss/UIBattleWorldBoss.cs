using UnityEngine;

namespace Ragnarok
{
    public class UIBattleWorldBoss : UICanvas
    {
        protected override UIType uiType => UIType.Hide;

        [SerializeField] UILabelHelper labelName;
        [SerializeField] UITextureHelper iconBoss;
        [SerializeField] UISprite iconElement;
        [SerializeField] UISlider bossHp;

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

        public void ShowBossInfo(string name, string thumbnail, string elementIcon)
        {
            Show();

            labelName.Text = name;
            iconBoss.Set(thumbnail);
            iconElement.spriteName = elementIcon;
        }

        public void SetBossHp(float value)
        {
            bossHp.value = value;
        }
    }
}