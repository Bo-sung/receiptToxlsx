using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class WorldBossInfoView : UISubCanvasListener<WorldBossInfoView.IListener>
    {
        public interface IListener
        {

        }

        [SerializeField] UITextureHelper iconBoss;
        [SerializeField] UILabelHelper labelBossName;
        [SerializeField] UILabelHelper labelBossLevel;

        WorldBossDungeonElement element;

        protected override void OnInit()
        {

        }

        protected override void OnClose()
        {

        }

        protected override void OnShow()
        {

        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {

        }

        public void SetData(WorldBossDungeonElement element)
        {
            this.element = element;
            Refresh();
        }

        public void Refresh()
        {
            if (element == null)
                return;

            var monsterInfo = element.GetMonsterInfo();

            iconBoss.SetBoss($"UI_Texture_Boss_{monsterInfo.Data.prefab_name}");
            labelBossName.Text = monsterInfo.Name;
            labelBossLevel.Text = $"Lv.{monsterInfo.Level}";
        }

    }
}