using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleToggleOption : UICanvas, IEqualityComparer<UIBattleToggleOption.MenuContent>
    {
        public enum MenuContent
        {
            /// <summary>
            /// 타겟팅 (포탑)
            /// </summary>
            TurretTargeting = 1,

            /// <summary>
            /// 타겟팅 (엠펠리움)
            /// </summary>
            BossTurretTargeting = 2,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIGrid grid;
        [SerializeField] UIButtonWithIcon btnTurretTargeting;
        [SerializeField] UIButtonWithIcon btnBossTurretTargeting;

        private BetterList<MenuContent> contents;
        private Dictionary<MenuContent, bool> toggleValueDic;

        public event System.Action<MenuContent, bool> OnChange;

        protected override void OnInit()
        {
            contents = new BetterList<MenuContent>();
            toggleValueDic = new Dictionary<MenuContent, bool>(this);

            // Initialize
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                contents.Add(item);
                toggleValueDic.Add(item, false); // OnShow 에서 기본값으로 세팅함
            }

            EventDelegate.Add(btnTurretTargeting.OnClick, OnClickedBtnTurretTargeting);
            EventDelegate.Add(btnBossTurretTargeting.OnClick, OnClickedBtnBossTurretTargeting);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnTurretTargeting.OnClick, OnClickedBtnTurretTargeting);
            EventDelegate.Remove(btnBossTurretTargeting.OnClick, OnClickedBtnBossTurretTargeting);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            foreach (MenuContent item in contents)
            {
                UpdateBtnText(item);
            }
        }

        void OnClickedBtnTurretTargeting()
        {
            Toggle(MenuContent.TurretTargeting);
        }

        void OnClickedBtnBossTurretTargeting()
        {
            Toggle(MenuContent.BossTurretTargeting);
        }

        public void SetMode(params (MenuContent content, bool value)[] args)
        {
            HideButtons(); // 모든 버튼 Hide

            // 특정 버튼 Show
            int size = args == null ? 0 : args.Length;
            for (int i = 0; i < size; i++)
            {
                MenuContent content = args[i].content;
                UIButtonHelper button = GetButton(content);
                toggleValueDic[content] = args[i].value;
                button.SetActive(true);
                button.SetAsLastSibling(); // 가장 마지막 Child로 세팅

                Refresh(content);
            }

            grid.Reposition();
        }

        public bool GetToggleValue(MenuContent content)
        {
            return toggleValueDic[content];
        }

        /// <summary>
        /// 토글
        /// </summary>
        private void Toggle(MenuContent content)
        {
            toggleValueDic[content] = !GetToggleValue(content);
            Refresh(content);

            OnChange?.Invoke(content, GetToggleValue(content));
        }

        /// <summary>
        /// 새로고침
        /// </summary>
        private void Refresh(MenuContent content)
        {
            UpdateBtnText(content);
            GetButton(content).GetIcon().color = GetToggleValue(content) ? Color.white : Color.gray;
            GetButton(content).SetIconName(GetBtnIconName(content));
        }

        /// <summary>
        /// 버튼 Text 업데이트
        /// </summary>
        private void UpdateBtnText(MenuContent content)
        {
            GetButton(content).LocalKey = GetBtnLocalKey(content);
        }

        private void HideButtons()
        {
            foreach (MenuContent item in contents)
            {
                UIButtonHelper button = GetButton(item);
                if (button == null)
                    continue;

                button.SetActive(false);
            }
        }

        private UIButtonWithIcon GetButton(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.TurretTargeting: return btnTurretTargeting;
                case MenuContent.BossTurretTargeting: return btnBossTurretTargeting;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            }
        }

        public int GetBtnLocalKey(MenuContent content)
        {
            bool value = GetToggleValue(content);

            switch (content)
            {
                // 타겟팅 On
                // 타겟팅 Off
                case MenuContent.TurretTargeting: return value ? LocalizeKey._65100 : LocalizeKey._65101;

                // 타겟팅 On
                // 타겟팅 Off
                case MenuContent.BossTurretTargeting: return value ? LocalizeKey._65102 : LocalizeKey._65103;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            }
        }

        public string GetBtnIconName(MenuContent content)
        {
            bool value = GetToggleValue(content);

            switch (content)
            {
                case MenuContent.TurretTargeting: return value ? "Ui_Battle_Option_Turret_On" : "Ui_Battle_Option_Turret_Off";
                case MenuContent.BossTurretTargeting: return value ? "Ui_Battle_Option_Emperium_On" : "Ui_Battle_Option_Emperium_Off";

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            }
        }

        bool IEqualityComparer<MenuContent>.Equals(MenuContent x, MenuContent y)
        {
            return x == y;
        }

        int IEqualityComparer<MenuContent>.GetHashCode(MenuContent obj)
        {
            return obj.GetHashCode();
        }
    }
}