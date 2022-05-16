using UnityEngine;

namespace Ragnarok
{
    public sealed class UIContentsLauncher : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        [SerializeField] UIPlayTween tween;
        [SerializeField] UITextureHelper icon;
        [SerializeField] TweenPosition tweenPosition;
        [SerializeField] TweenWidth tweenWidth;
        [SerializeField] TweenHeight tweenHeight;

        private ContentType contentType;

        protected override void OnInit()
        {
            EventDelegate.Add(tween.onFinished, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(tween.onFinished, CloseUI);

            ShowContents(contentType);
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

        public void Launch(UIWidget from, ContentType type)
        {
            if (from == null)
                return;

            contentType = type;
            UIWidget to = GetMenuIcon(contentType);
            if (to == null)
            {
                CloseUI();
                return;
            }

            const string SPRITE_NAME = "ContentsUnlock_{VALUE}";
            icon.SetContentsUnlock(SPRITE_NAME.Replace(ReplaceKey.VALUE, (int)contentType));
            Transform parent = icon.cachedTransform.parent;
            tweenPosition.from = parent.worldToLocalMatrix.MultiplyPoint3x4(from.cachedTransform.position);
            tweenPosition.to = parent.worldToLocalMatrix.MultiplyPoint3x4(to.cachedTransform.position);
            tweenWidth.from = from.width;
            tweenWidth.to = to.width;
            tweenHeight.from = from.height;
            tweenHeight.to = to.height;
            tween.Play();
        }

        private void CloseUI()
        {
            UI.Close<UIContentsLauncher>();
        }

        private UIWidget GetMenuIcon(ContentType type)
        {
            switch (type)
            {
                case ContentType.Stage:
                    return null;

                case ContentType.Skill:
                    return GetMainMenuIcon(UIMain.MenuContent.Menu);

                case ContentType.Dungeon:
                    return GetMainMenuIcon(UIMain.MenuContent.Menu);

                case ContentType.Cupet:
                    return GetMainMenuIcon(UIMain.MenuContent.Menu);

                case ContentType.Make:
                    return GetMainMenuIcon(UIMain.MenuContent.Make);

                case ContentType.SecretShop:
                    return GetMainMenuIcon(UIMain.MenuContent.Shop);

                case ContentType.Rebirth:
                    return GetMainMenuIcon(UIMain.MenuContent.Inven);

                case ContentType.JobChange:
                    return GetJobChangeMenuIcon(UIJobChangeMenu.MenuContent.JobChange);

                case ContentType.CombatAgent:
                    return GetMainMenuIcon(UIMain.MenuContent.Menu);

                case ContentType.Sharing:
                    return GetQuickExpandMenuIcon(UIQuickExpandMenu.MenuContent.Share);

                case ContentType.ZenyDungeon:
                    return GetMainMenuIcon(UIMain.MenuContent.Menu);

                case ContentType.ExpDungeon:
                    return GetMainMenuIcon(UIMain.MenuContent.Menu);

                case ContentType.Buff:
                    return GetBattleMenuIcon(UIBattleMenu.MenuContent.Buff);

                case ContentType.TradeTown:
                    return GetBattleMenuIcon(UIBattleMenu.MenuContent.Square);

                case ContentType.ItemEnchant:
                    return GetMainMenuIcon(UIMain.MenuContent.Inven);

                case ContentType.Duel:
                    return GetBattleMenuIcon(UIBattleMenu.MenuContent.Duel);

                case ContentType.Explore:
                    return GetBattleMenuIcon(UIBattleMenu.MenuContent.Explore);

                case ContentType.Maze:
                    return GetBattleMenuIcon(UIBattleMenu.MenuContent.Maze);

                case ContentType.Boss:
                    return GetBattleStageMenuIcon(UIBattleStageMenu.MenuContent.BossSummon);

                case ContentType.Pvp:
                    return GetMainMenuIcon(UIMain.MenuContent.Menu);

                case ContentType.ShareControl:
                    return GetQuickExpandMenuIcon(UIQuickExpandMenu.MenuContent.Share);

                case ContentType.ShareLevelUp:
                    return GetQuickExpandMenuIcon(UIQuickExpandMenu.MenuContent.Share);

                case ContentType.ChangeElement:
                    return GetMainMenuIcon(UIMain.MenuContent.Make);

                case ContentType.TierUp:
                    return GetMainMenuIcon(UIMain.MenuContent.Make);

                case ContentType.ManageCard:
                    return GetMainMenuIcon(UIMain.MenuContent.Make);

                case ContentType.FreeFight:
                    return GetMainMenuIcon(UIMain.MenuContent.Menu);

                case ContentType.ShareHope:
                    return GetQuickExpandMenuIcon(UIQuickExpandMenu.MenuContent.Share);

                case ContentType.ShareVice2ndOpen:
                    return GetShare2ndIcon();

                case ContentType.AchieveMultiMaze:
                    return GetJobChangeMenuIcon(UIJobChangeMenu.MenuContent.JobChange);
            }

            return null;
        }

        private void ShowContents(ContentType type)
        {
            switch (type)
            {
                case ContentType.Stage:
                    break;

                case ContentType.Skill:
                    PlayTweenMainMenu(UIMain.MenuContent.Menu);
                    break;

                case ContentType.Dungeon:
                    PlayTweenMainMenu(UIMain.MenuContent.Menu);
                    break;

                case ContentType.Cupet:
                    PlayTweenMainMenu(UIMain.MenuContent.Menu);
                    break;

                case ContentType.Make:
                    PlayTweenMainMenu(UIMain.MenuContent.Make);
                    break;

                case ContentType.SecretShop:
                    PlayTweenMainMenu(UIMain.MenuContent.Shop);
                    break;

                case ContentType.Rebirth:
                    PlayTweenMainMenu(UIMain.MenuContent.Inven);
                    break;

                case ContentType.JobChange:
                    UpdateJobChangeMenu(UIJobChangeMenu.MenuContent.JobChange);
                    break;

                case ContentType.CombatAgent:
                    PlayTweenMainMenu(UIMain.MenuContent.Menu);
                    break;

                case ContentType.Sharing:
                    UpdateQuickExpandMenu(UIQuickExpandMenu.MenuContent.Share);
                    break;

                case ContentType.ZenyDungeon:
                    PlayTweenMainMenu(UIMain.MenuContent.Menu);
                    break;

                case ContentType.ExpDungeon:
                    PlayTweenMainMenu(UIMain.MenuContent.Menu);
                    break;

                case ContentType.Buff:
                    UpdateBattleMenu(UIBattleMenu.MenuContent.Buff);
                    break;

                case ContentType.TradeTown:
                    UpdateBattleMenu(UIBattleMenu.MenuContent.Square);
                    break;

                case ContentType.ItemEnchant:
                    PlayTweenMainMenu(UIMain.MenuContent.Inven);
                    break;

                case ContentType.Duel:
                    UpdateBattleMenu(UIBattleMenu.MenuContent.Duel);
                    break;

                case ContentType.Explore:
                    UpdateBattleMenu(UIBattleMenu.MenuContent.Explore);
                    break;

                case ContentType.Maze:
                    UpdateBattleMenu(UIBattleMenu.MenuContent.Maze);
                    break;

                case ContentType.Boss:
                    UpdateBattleStageMenu(UIBattleStageMenu.MenuContent.BossSummon);
                    break;

                case ContentType.Pvp:
                    PlayTweenMainMenu(UIMain.MenuContent.Menu);
                    break;

                case ContentType.ShareControl:
                    break;

                case ContentType.ShareLevelUp:
                    break;

                case ContentType.ChangeElement:
                    PlayTweenMainMenu(UIMain.MenuContent.Make);
                    break;

                case ContentType.TierUp:
                    PlayTweenMainMenu(UIMain.MenuContent.Make);
                    break;

                case ContentType.ManageCard:
                    PlayTweenMainMenu(UIMain.MenuContent.Make);
                    break;

                case ContentType.FreeFight:
                    PlayTweenMainMenu(UIMain.MenuContent.Menu);
                    break;

                case ContentType.ShareVice2ndOpen:
                    UpdateBattleShare2nd();
                    break;

                case ContentType.AchieveMultiMaze:
                    UpdateJobChangeMenu(UIJobChangeMenu.MenuContent.JobChange);
                    break;
            }
        }

        private UIWidget GetMainMenuIcon(UIMain.MenuContent content)
        {
            UIMain ui = UI.GetUI<UIMain>();
            if (ui == null || !ui.IsVisible)
                return null;

            return ui.GetMenuIcon(content);
        }

        private UIWidget GetQuickExpandMenuIcon(UIQuickExpandMenu.MenuContent content)
        {
            UIQuickExpandMenu ui = UI.GetUI<UIQuickExpandMenu>();
            if (ui == null || !ui.IsVisible)
                return null;

            return ui.GetMenuIcon(content);
        }

        private UIWidget GetJobChangeMenuIcon(UIJobChangeMenu.MenuContent content)
        {
            UIJobChangeMenu ui = UI.GetUI<UIJobChangeMenu>();
            if (ui == null || !ui.IsVisible)
                return null;

            return ui.GetMenuIcon(content);
        }

        private UIWidget GetBattleMenuIcon(UIBattleMenu.MenuContent content)
        {
            UIBattleMenu ui = UI.GetUI<UIBattleMenu>();
            if (ui == null || !ui.IsVisible)
                return null;

            return ui.GetMenuIcon(content);
        }

        private UIWidget GetMainShortcutMenuIcon(UIMainShortcut.MenuContent content)
        {
            UIMainShortcut ui = UI.GetUI<UIMainShortcut>();
            if (ui == null || !ui.IsVisible)
                return null;

            return ui.GetMenuIcon(content);
        }

        private UIWidget GetBattleStageMenuIcon(UIBattleStageMenu.MenuContent content)
        {
            UIBattleStageMenu ui = UI.GetUI<UIBattleStageMenu>();
            if (ui == null || !ui.IsVisible)
                return null;

            return ui.GetMenuIcon(content);
        }

        private UIWidget GetShare2ndIcon()
        {
            UIBattleShare2nd ui = UI.GetUI<UIBattleShare2nd>();
            if (ui == null || !ui.IsVisible)
                return null;

            return ui.GetShare2ndIcon();
        }

        private void PlayTweenMainMenu(UIMain.MenuContent content)
        {
            UIMain ui = UI.GetUI<UIMain>();
            if (ui == null || !ui.IsVisible)
                return;

            ui.PlayTweenEffect(content);
        }

        private void UpdateQuickExpandMenu(UIQuickExpandMenu.MenuContent content)
        {
            UIQuickExpandMenu ui = UI.GetUI<UIQuickExpandMenu>();
            if (ui == null || !ui.IsVisible)
                return;

            ui.UpdateOpenContent(content);
        }

        private void UpdateJobChangeMenu(UIJobChangeMenu.MenuContent content)
        {
            UIJobChangeMenu ui = UI.GetUI<UIJobChangeMenu>();
            if (ui == null || !ui.IsVisible)
                return;

            ui.UpdateOpenContent(content);
        }

        private void UpdateBattleMenu(UIBattleMenu.MenuContent content)
        {
            UIBattleMenu ui = UI.GetUI<UIBattleMenu>();
            if (ui == null || !ui.IsVisible)
                return;

            ui.UpdateOpenContent(content);
            ui.GridUpperRightReposition();
        }

        private void PlayTweenMainShortcutMenu(UIMainShortcut.MenuContent content)
        {
            UIMainShortcut ui = UI.GetUI<UIMainShortcut>();
            if (ui == null || !ui.IsVisible)
                return;

            ui.PlayTweenEffect(content);
        }

        private void UpdateBattleStageMenu(UIBattleStageMenu.MenuContent content)
        {
            UIBattleStageMenu ui = UI.GetUI<UIBattleStageMenu>();
            if (ui == null || !ui.IsVisible)
                return;

            ui.UpdateOpenContent(content);
        }

        private void UpdateBattleShare2nd()
        {
            UIBattleShare2nd ui = UI.GetUI<UIBattleShare2nd>();
            if (ui == null || !ui.IsVisible)
                return;

            ui.UpdateNewIcon();
        }

        public override bool Find()
        {
            base.Find();

            if (tweenPosition == null)
                tweenPosition = GetComponentInChildren<TweenPosition>();

            if (tweenWidth == null)
                tweenWidth = GetComponentInChildren<TweenWidth>();

            if (tweenHeight == null)
                tweenHeight = GetComponentInChildren<TweenHeight>();

            return true;
        }
    }
}