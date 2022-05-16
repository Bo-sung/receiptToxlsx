using UnityEngine;

namespace Ragnarok
{
    public class UIHextechView : MonoBehaviour, IAutoInspectorFinder
    {
        public enum Mode { None, GiveElemental, TierUp, RestoreCard, ResetCard }

        [SerializeField] UITabHelper tab;
        [SerializeField] UILabelHelper labelNoti;

        // Selected Item
        [SerializeField] UIEquipmentProfile targetEquipItem;
        [SerializeField] UICardProfile targetCardItem;

        // List Panel
        [SerializeField] GameObject materialListPanelRoot;
        [SerializeField] UIHextechSlot[] hextechSlots;
        [SerializeField] UIGridHelper hextechSlotGrid;
        [SerializeField] UIButtonHelper btnHelp;

        // Tier Up View
        [SerializeField] UIChangeElementView changeElement;
        [SerializeField] UITierUpView tierUp;
        [SerializeField] UICardRestore uiCardRestore;
        [SerializeField] UICardReset uiCardReset;

        [SerializeField] GameObject contentsOpenEffect;
        [SerializeField] GameObject contentsOpenEffect2;
        [SerializeField] Animation contentsOpenAnim;
        [SerializeField] Animation contentsOpenAnim2;
        [SerializeField] float contentsOpenEffectTime;

        private Mode curMode = Mode.None;

        public System.Action onTabChange;
        private Animation curPlayingAnim;

        public void OnInit()
        {
            EventDelegate.Add(tab[0].OnChange, ShowGiveElement);
            EventDelegate.Add(tab[1].OnChange, ShowSuperLevelUp);
            EventDelegate.Add(tab[2].OnChange, ShowCardRestore);
            EventDelegate.Add(tab[3].OnChange, ShowCardReset);
            EventDelegate.Add(btnHelp.OnClick, ShowHelpPopup);

            changeElement.OnInit();
            tierUp.OnInit();
            uiCardRestore.OnInit();
            uiCardReset.OnInit();

            changeElement.gameObject.SetActive(false);
            tierUp.gameObject.SetActive(false);
            uiCardRestore.gameObject.SetActive(false);
            uiCardReset.gameObject.SetActive(false);
        }

        public void OnClose()
        {
            EventDelegate.Remove(tab[0].OnChange, ShowGiveElement);
            EventDelegate.Remove(tab[1].OnChange, ShowSuperLevelUp);
            EventDelegate.Remove(tab[2].OnChange, ShowCardRestore);
            EventDelegate.Remove(tab[3].OnChange, ShowCardReset);
            EventDelegate.Remove(btnHelp.OnClick, ShowHelpPopup);

            changeElement.OnClose();
            tierUp.OnClose();
            uiCardRestore.OnClose();
            uiCardReset.OnClose();
        }

        public void OnShow(UIMake.Input input = null)
        {
            if (input == null)
            {
                if (curMode == Mode.None)
                {
                    tab[0].Value = true;
                    tab[1].Value = false;
                    tab[2].Value = false;
                    tab[3].Value = false;
                    SelectMode(Mode.GiveElemental, true);
                }
                else
                {
                    SelectMode(curMode, true);
                }
            }
            else if (input.inputType == UIMake.InputType.GoToRestoreCard)
            {
                tab[0].Value = false;
                tab[1].Value = false;
                tab[2].Value = true;
                tab[3].Value = false;
                SelectMode(Mode.RestoreCard, true);
                uiCardRestore.SelectCard(Entity.player.Inventory.GetItemInfo((long)input.data) as CardItemInfo);
            }
            else if (input.inputType == UIMake.InputType.ShortCut)
            {
                Mode mode = (input.tab + 1).ToEnum<Mode>();
                tab[input.tab].Value = true;
                SelectMode(mode, true);
            }

            contentsOpenEffect.SetActive(false);
            contentsOpenEffect2.SetActive(false);
            contentsOpenEffectTimer = 0;
            curPlayingAnim = null;
        }

        public void OnHide()
        {
            if (curMode == Mode.GiveElemental)
            {
                changeElement.gameObject.SetActive(false);
                changeElement.OnHide();
            }
            else if (curMode == Mode.TierUp)
            {
                tierUp.gameObject.SetActive(false);
                tierUp.OnHide();
            }
            else if (curMode == Mode.RestoreCard)
            {
                uiCardRestore.gameObject.SetActive(false);
                uiCardRestore.OnHide();
            }
            else if (curMode == Mode.ResetCard)
            {
                uiCardReset.gameObject.SetActive(false);
                uiCardReset.OnHide();
            }
        }

        public void OnLocalize()
        {
            labelNoti.LocalKey = LocalizeKey._28032;
            tab[0].LocalKey = LocalizeKey._28033;
            tab[1].LocalKey = LocalizeKey._28034;
            tab[2].LocalKey = LocalizeKey._28035;
            tab[3].LocalKey = LocalizeKey._28036;
        }

        private void ShowGiveElement()
        {
            if (!tab[0].Value)
                return;

            SelectMode(Mode.GiveElemental);
        }

        private void ShowSuperLevelUp()
        {
            if (!tab[1].Value)
                return;

            SelectMode(Mode.TierUp);
        }

        private void ShowCardRestore()
        {
            if (!tab[2].Value)
                return;

            SelectMode(Mode.RestoreCard);
        }

        private void ShowCardReset()
        {
            if (!tab[3].Value)
                return;

            SelectMode(Mode.ResetCard);
        }

        private void ShowHelpPopup()
        {
            switch (curMode)
            {
                case Mode.GiveElemental:
                    UI.ConfirmPopup(LocalizeKey._28062.ToText(), LocalizeKey._28059.ToText()); // 무기나 갑옷에 속성을 부여하면 [62AEE4][C]해당 장비의 속성이 변경[/c][-]됩니다.\n\n재료로 사용되는 속성석은 제작을 통해 획득 가능합니다.
                    break;

                case Mode.TierUp:
                    UI.Show<UITierUpHelpPopup>();
                    break;

                case Mode.RestoreCard:
                    UI.ConfirmPopup(LocalizeKey._28063.ToText(), LocalizeKey._28060.ToText()); // 카드 레벨을 저장된 구간으로 복원시킵니다.\n\n복원 구간은 [62AEE4][C]레벨 10 단위마다 저장[/c][-]되고, 복원 구간이 저장됐으면, 저장된 구간 밑으로는 복원할 수 없습니다.
                    break;

                case Mode.ResetCard:
                    UI.ConfirmPopup(LocalizeKey._28064.ToText(), LocalizeKey._28061.ToText()); // 카드 재구성 시, 어느 레벨의 카드던 [62AEE4][C]레벨 1[/c][-]로 돌아갑니다.
                    break;
            }
        }

        private void SelectMode(Mode mode, bool forResetView = false)
        {
            if (!forResetView)
            {
                if (curMode == mode)
                    return;

                onTabChange?.Invoke();
            }

            var prevMode = curMode;
            curMode = mode;

            if (prevMode == Mode.GiveElemental)
            {
                changeElement.gameObject.SetActive(false);
                changeElement.OnHide();
            }
            else if (prevMode == Mode.TierUp)
            {
                tierUp.gameObject.SetActive(false);
                tierUp.OnHide();
            }
            else if (prevMode == Mode.RestoreCard)
            {
                uiCardRestore.gameObject.SetActive(false);
                uiCardRestore.OnHide();
            }
            else if (prevMode == Mode.ResetCard)
            {
                uiCardReset.gameObject.SetActive(false);
                uiCardReset.OnHide();
            }

            targetEquipItem.gameObject.SetActive(false);
            targetCardItem.gameObject.SetActive(false);
            materialListPanelRoot.SetActive(false);

            HideAllHexTechSlots();

            if (curMode == Mode.GiveElemental)
            {
                changeElement.gameObject.SetActive(true);
                changeElement.OnShow();
            }
            else if (curMode == Mode.TierUp)
            {
                tierUp.gameObject.SetActive(true);
                tierUp.OnShow();
            }
            else if (curMode == Mode.RestoreCard)
            {
                uiCardRestore.gameObject.SetActive(true);
                uiCardRestore.OnShow();
            }
            else if (curMode == Mode.ResetCard)
            {
                uiCardReset.gameObject.SetActive(true);
                uiCardReset.OnShow();
            }
        }

        private void HideAllHexTechSlots()
        {
            for (int i = 0; i < hextechSlots.Length; ++i)
                hextechSlots[i].gameObject.SetActive(false);
        }

        public void SetTab(Mode value)
        {
            tab[0].Value = value == Mode.GiveElemental;
            tab[1].Value = value == Mode.TierUp;
            tab[2].Value = value == Mode.RestoreCard;
            tab[3].Value = value == Mode.ResetCard;
            SelectMode(value, true);
        }

        public void ForceHextechContentsOpen(bool value)
        {
            if (curMode == Mode.GiveElemental)
                changeElement.SetContentsOpen(value);
            else if (curMode == Mode.TierUp)
                tierUp.SetContentsOpen(value);
            else if (curMode == Mode.RestoreCard)
                uiCardRestore.SetContentsOpen(value);
            else if (curMode == Mode.ResetCard)
                uiCardReset.SetContentsOpen(value);
        }

        public void StartGiveElementContentsOpenEffect()
        {
            contentsOpenEffect.SetActive(true);
            contentsOpenEffectTimer = contentsOpenEffectTime;
            contentsOpenAnim.Play();
            curPlayingAnim = contentsOpenAnim;
        }

        public void StartTierUpContentsOpenEffect()
        {
            contentsOpenEffect2.SetActive(true);
            contentsOpenEffectTimer = contentsOpenEffectTime;
            contentsOpenAnim2.Play();
            curPlayingAnim = contentsOpenAnim2;
        }

        public bool IsContentsOpenEffectFinished()
        {
            return curPlayingAnim == null || !curPlayingAnim.isPlaying;
        }

        private float contentsOpenEffectTimer = 0;
        private void Update()
        {
            if (contentsOpenEffectTimer > 0)
            {
                contentsOpenEffectTimer -= Time.deltaTime;
                if (contentsOpenEffectTimer <= 0)
                {
                    ForceHextechContentsOpen(true);
                }
            }
        }
    }
}