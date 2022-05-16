using UnityEngine;

namespace Ragnarok
{
    public class UINoCard : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper title;
        [SerializeField] UILabelHelper desc;
        [SerializeField] UICostButtonHelper enterButton;
        [SerializeField] UIButton closeButton;

        protected override void OnInit()
        {
            EventDelegate.Add(closeButton.onClick, CloseThis);
            EventDelegate.Add(enterButton.OnClick, TryEnter);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(closeButton.onClick, CloseThis);
            EventDelegate.Remove(enterButton.OnClick, TryEnter);
        }

        protected override void OnLocalize()
        {
            title.LocalKey = LocalizeKey._44100;
            desc.LocalKey = LocalizeKey._44101;
            enterButton.LocalKey = LocalizeKey._44102;
        }

        protected override void OnShow(IUIData data = null)
        {
            if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.Stage)
            {
                UI.ShowToastPopup(LocalizeKey._90200.ToText());
                UI.Close<UINoCard>();
                return;
            }

            int c = Entity.player.Dungeon.GetFreeEntryCount(DungeonType.MultiMaze);
            int mc = Entity.player.Dungeon.GetFreeEntryMaxCount(DungeonType.MultiMaze);
            enterButton.CostText = $"{c} / {mc}";
            enterButton.IsEnabled = c > 0;
        }

        protected override void OnHide()
        {
        }

        private void TryEnter()
        {
            if (!Entity.player.Quest.IsOpenContent(ContentType.Maze, isShowPopup: true))
                return;

            UI.Show<UIAdventureMazeSelect>();
            UI.Close<UINoCard>();
        }

        private void CloseThis()
        {
            UI.Close<UINoCard>();
        }
    }
}