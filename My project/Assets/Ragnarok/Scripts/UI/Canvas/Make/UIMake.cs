using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIMake : UICanvas
    {
        public enum Mode { None, Craft, Hextech }
        public enum InputType { None, GoToRestoreCard, ShortCut }

        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] GameObject makeDefault;
        [SerializeField] UILabelValue makeLock;
        [SerializeField] UICraft uiCraft;
        [SerializeField] UIHextechView uiHextech;
        [SerializeField] NPCStyle craftNPC;
        [SerializeField] NPCStyle hextechNPC;
        [SerializeField] UITexture craftNPCTexture;
        [SerializeField] UITexture hextechNPCTexture;
        [SerializeField] UIButtonHelper modeChangeButton;
        [SerializeField] UIWidget hextechBtnWidget;

        private GoodsModel goodsModel;
        private QuestModel questModel;
        private Mode curMode = Mode.None;

        private float modeAnimTimer = 0;
        private bool isPlayingModeAnim = false;
        private bool isHidingPhase = true;
        private Mode targetMode;

        public UIWidget HextechBtn => hextechBtnWidget;
        public bool IsHextechShowing() { return curMode == Mode.Hextech; }

        /// <summary>
        /// (optional : 바로가기 정보)
        /// </summary>
        public class Input : IUIData
        {
            public int itemId; // 특정 아이템
            public InputType inputType;
            public object data;
            public int tab;
            public int materialItemId;

            public Input(int itemId, int materialItemId = 0)
            {
                this.itemId = itemId;
                this.materialItemId = materialItemId;
            }

            public Input(InputType inputType, long no = 0)
            {
                this.inputType = inputType;
                data = no;
            }

            public Input(InputType inputType, int tab)
            {
                this.inputType = inputType;
                this.tab = tab;
            }
        }

        protected override void OnInit()
        {
            goodsModel = Entity.player.Goods;
            questModel = Entity.player.Quest;

            goodsModel.OnUpdateZeny += OnUpdateZeny;
            goodsModel.OnUpdateCatCoin += OnUpdateCatCoin;

            uiCraft.OnInit();
            uiHextech.OnInit();

            uiCraft.onTabChange = OnCraftViewTabChanged;
            uiHextech.onTabChange = OnHextechViewTabChanged;

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            EventDelegate.Add(modeChangeButton.OnClick, ToggleMode);
        }

        protected override void OnClose()
        {
            goodsModel.OnUpdateZeny -= OnUpdateZeny;
            goodsModel.OnUpdateCatCoin -= OnUpdateCatCoin;

            uiCraft.OnClose();
            uiHextech.OnClose();

            EventDelegate.Remove(modeChangeButton.OnClick, ToggleMode);
        }

        protected override void OnShow(IUIData data = null)
        {
            Input input = data as Input;

            titleView.ShowZeny(goodsModel.Zeny);
            titleView.ShowCatCoin(goodsModel.CatCoin);
            modeChangeButton.gameObject.SetActive(true);

            if (input == null || input.inputType == InputType.None)
            {
                curMode = Mode.Craft;
                SetActiveCraft(true);
                craftNPC.PlayTalk();
                uiCraft.OnShow(data);
                modeChangeButton.LocalKey = LocalizeKey._28031;
            }
            else if (input.inputType == InputType.GoToRestoreCard)
            {
                curMode = Mode.Hextech;
                SetActiveCraft(false);
                hextechNPC.PlayTalk();
                uiHextech.OnShow(input);
                modeChangeButton.LocalKey = LocalizeKey._3003;
            }
            else if (input.inputType == InputType.ShortCut)
            {
                curMode = Mode.Hextech;
                SetActiveCraft(false);
                hextechNPC.PlayTalk();
                uiHextech.OnShow(input);
                modeChangeButton.LocalKey = LocalizeKey._3003;
            }

            UpdateOpenContent();
        }

        private void SetActiveCraft(bool value)
        {
            uiCraft.gameObject.SetActive(value);
            uiHextech.gameObject.SetActive(!value);
            craftNPC.gameObject.SetActive(value);
            hextechNPC.gameObject.SetActive(!value);
        }

        protected override void OnHide()
        {
            if (curMode == Mode.Craft)
                uiCraft.OnHide();
            else if (curMode == Mode.Hextech)
                uiHextech.OnHide();

            isPlayingModeAnim = false;
            uiCraft.transform.localRotation = Quaternion.identity;
            uiHextech.transform.localRotation = Quaternion.identity;
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._28000.ToText()); // 제작
            makeLock.TitleKey = LocalizeKey._28029; // 제작 잠금 해제
            makeLock.Value = questModel.OpenContentText(ContentType.Make);
            uiCraft.OnLocalize();
            uiHextech.OnLocalize();
        }

        protected override void OnBack()
        {
            if (uiCraft.OnBack())
                return;
            base.OnBack();
        }

        private void UpdateOpenContent()
        {
            // 컨텐츠 오픈 체크
            bool isOpenContent = questModel.IsOpenContent(ContentType.Make, isShowPopup: false);
            makeDefault.SetActive(isOpenContent);
            makeLock.SetActive(!isOpenContent);
        }

        /// <summary>
        /// 제니 변경 이벤트
        /// </summary>
        /// <param name="totalZeny"></param>
        private void OnUpdateZeny(long totalZeny)
        {
            titleView.ShowZeny(totalZeny);
        }

        /// <summary>
        /// 냥다래 변경 이벤트
        /// </summary>
        /// <param name="totalCatCoin"></param>
        private void OnUpdateCatCoin(long totalCatCoin)
        {
            titleView.ShowCatCoin(totalCatCoin);
        }

        private void OnCraftViewTabChanged()
        {
            craftNPC.PlayTalk();
        }

        private void OnHextechViewTabChanged()
        {
            hextechNPC.PlayTalk();
        }

        private void OnClickedBtnZeny()
        {
            UI.ShowZenyShop();
        }

        private void OnClickedBtnCatCoin()
        {
            UI.ShowCashShop();
        }

        private void ToggleMode()
        {
            if (curMode == Mode.Craft)
                StartModeChanging(Mode.Hextech);
            else if (curMode == Mode.Hextech)
                StartModeChanging(Mode.Craft);
        }

        private void StartModeChanging(Mode targetMode)
        {
            modeAnimTimer = 0;
            isPlayingModeAnim = true;
            isHidingPhase = true;
            this.targetMode = targetMode;

            modeChangeButton.gameObject.SetActive(false);
            craftNPC.gameObject.SetActive(false);
            hextechNPC.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (isPlayingModeAnim)
            {
                modeAnimTimer += Time.deltaTime;
                float prog = Mathf.Clamp01(modeAnimTimer / 0.15f);
                Transform hidingView = targetMode == Mode.Hextech ? uiCraft.transform : uiHextech.transform;
                Transform targetView = targetMode == Mode.Hextech ? uiHextech.transform : uiCraft.transform;

                if (isHidingPhase)
                {
                    hidingView.localRotation = Quaternion.Euler(0, prog * -90f, 0);

                    if (prog == 1f)
                    {
                        isHidingPhase = false;
                        modeAnimTimer = 0;
                        hidingView.gameObject.SetActive(false);
                        targetView.gameObject.SetActive(true);
                        targetView.transform.localRotation = Quaternion.Euler(0, 90f, 0);
                        curMode = targetMode;

                        if (targetMode == Mode.Hextech)
                        {
                            uiCraft.OnHide();
                            uiHextech.OnShow();
                        }
                        else
                        {
                            uiHextech.OnHide();
                            uiCraft.OnShow();
                        }
                    }
                }
                else
                {
                    targetView.transform.localRotation = Quaternion.Euler(0, 90f * (1 - prog), 0);

                    if (prog == 1f)
                    {
                        isPlayingModeAnim = false;
                        modeChangeButton.gameObject.SetActive(true);

                        if (targetMode == Mode.Hextech)
                        {
                            modeChangeButton.LocalKey = LocalizeKey._3003;
                            hextechNPC.gameObject.SetActive(true);
                            hextechNPC.PlayTalk();
                        }
                        else
                        {
                            modeChangeButton.LocalKey = LocalizeKey._28031;
                            craftNPC.gameObject.SetActive(true);
                            craftNPC.PlayTalk();
                        }
                    }
                }
            }
        }

        public void SetMode(Mode mode)
        {
            if (mode == Mode.Craft)
            {
                curMode = Mode.Craft;
                SetActiveCraft(true);
                craftNPC.PlayTalk();
                uiCraft.OnShow(null);
                modeChangeButton.LocalKey = LocalizeKey._28031;
            }
            else if (mode == Mode.Hextech)
            {
                curMode = Mode.Hextech;
                SetActiveCraft(false);
                hextechNPC.PlayTalk();
                uiHextech.OnShow(null);
                modeChangeButton.LocalKey = LocalizeKey._3003;
            }
        }

        public void SetHextechTab(UIHextechView.Mode tab)
        {
            uiHextech.SetTab(tab);
        }

        public void ForceHextechContentsOpen(bool value)
        {
            uiHextech.ForceHextechContentsOpen(value);
        }

        public void StartGiveElementContentsOpenEffect()
        {
            uiHextech.StartGiveElementContentsOpenEffect();
        }

        public void StartTierUpContentsOpenEffect()
        {
            uiHextech.StartTierUpContentsOpenEffect();
        }

        public bool IsContentsOpenEffectFinished()
        {
            return uiHextech.IsContentsOpenEffectFinished();
        }
    }
}