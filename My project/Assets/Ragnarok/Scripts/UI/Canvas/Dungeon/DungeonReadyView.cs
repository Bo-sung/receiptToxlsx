using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class DungeonReadyView : UIView, IInspectorFinder
    {
        [SerializeField] UIDungeonDifficulty difficulty;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] UILabelHelper labelFreeReward;
        [SerializeField] UILabelHelper labelRewardList;
        [SerializeField] UIButtonHelper btnFreeReward;
        [SerializeField] UIGraySprite iconFreeReward;
        [SerializeField] UITextureHelper dungeonImage;
        [SerializeField] SuperScrollListWrapper wrapperReward;
        [SerializeField] GameObject prefabReward;
        [SerializeField] UILabelHelper labelItemTitle;
        [SerializeField] UIItemCostButtonHelper btnFastClear, btnEnter;
        [SerializeField] UIButton btnFastClearLock, btnEnterLock;
        [SerializeField] UIDungeonDetailInfoSlot[] infos;

        DungeonDetailElement element;
        (RewardInfo info, bool isBoss)[] rewardInfos;

        DungeonPresenter presenter;

        public event System.Action<DungeonType, int> OnSelectFastClearBattle;
        public event System.Action<DungeonType, int> OnSelectStartBattle;

        protected override void Awake()
        {
            base.Awake();
            wrapperReward.SetRefreshCallback(OnRewardElementRefresh);
            wrapperReward.SpawnNewList(prefabReward, 0, 0);

            difficulty.OnSelect += OnSelect;

            EventDelegate.Add(btnFastClear.OnClick, OnClickedBtnFastClear);
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnFastClearLock.onClick, OnClickedBtnFastClearLock);
            EventDelegate.Add(btnEnterLock.onClick, OnClickedBtnEnterLock);
            EventDelegate.Add(btnFreeReward.OnClick, OnClickedBtnFreeReward);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            difficulty.OnSelect -= OnSelect;

            EventDelegate.Remove(btnFastClear.OnClick, OnClickedBtnFastClear);
            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnFastClearLock.onClick, OnClickedBtnFastClearLock);
            EventDelegate.Remove(btnEnterLock.onClick, OnClickedBtnEnterLock);
            EventDelegate.Remove(btnFreeReward.OnClick, OnClickedBtnFreeReward);
        }

        protected override void OnLocalize()
        {
            labelItemTitle.LocalKey = LocalizeKey._7025; // 아이템
            labelReward.LocalKey = LocalizeKey._7049; // 보상 리스트
            labelFreeReward.LocalKey = LocalizeKey._7050; // 무료 보상
            labelRewardList.LocalKey = LocalizeKey._7051; // 던전 보상
            btnFreeReward.LocalKey = LocalizeKey._7052; // 보상 받기
            btnFastClear.Text = LocalizeKey._48309.ToText(); // 소탕
            btnEnter.LocalKey = LocalizeKey._7027; // 던전 입장
        }

        void OnSelect(DungeonDetailElement element)
        {
            this.element = element;
            Refresh();
        }

        void OnRewardElementRefresh(GameObject go, int index)
        {
            UIMonsterRewardSlot ui = go.GetComponent<UIMonsterRewardSlot>();
            ui.Set(rewardInfos[index]);
        }

        void OnClickedBtnFastClear()
        {
            if (element == null)
                return;

            OnSelectFastClearBattle?.Invoke(element.DungeonType, element.Id);
        }

        void OnClickedBtnEnter()
        {
            if (element == null)
                return;

            OnSelectStartBattle?.Invoke(element.DungeonType, element.Id);
        }

        void OnClickedBtnFastClearLock()
        {
            string description = LocalizeKey._90222.ToText(); // 던전 소탕 기능은 클리어한 던전에서만 이용 가능합니다.
            UI.ShowToastPopup(description);
        }

        void OnClickedBtnEnterLock()
        {
            string description = LocalizeKey._90221.ToText(); // 이전 난이도를 클리어해야 합니다.
            UI.ShowToastPopup(description);
        }

        void OnClickedBtnFreeReward()
        {
            if (PossibleFreeReward())
            {
                presenter.RequestFreeReward(element.DungeonType);
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._7053.ToText());
            }
        }

        public void SetData(DungeonDetailElement[] elements)
        {
            element = null;
            difficulty.SetData(elements);
        }

        public void Initialize(DungeonPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void Refresh()
        {
            if (element == null)
                return;

            dungeonImage.SetDungeon(GetDungeonImage(element.DungeonType), isAsync: false);

            bool isCleared = element.IsCleardDungeon();
            int clearTicketCount = element.GetClearTicketCount();
            bool hasClearTicket = clearTicketCount > 0; // 소탕권 존재
            btnFastClear.IsEnabled = hasClearTicket; // 소탕권 존재
            btnFastClear.SetItemCount(clearTicketCount);
            btnFastClear.SetCountColor(hasClearTicket);
            btnFastClearLock.isEnabled = hasClearTicket && !isCleared; // 소탕권은 존재하지만 클리어하지 않았음

            bool canEnter = element.CanEnter(isShowPopup: false); // 입장 가능
            int freeCount = element.GetFreeCount();

            if (isCleared && freeCount > 0)
            {
                btnEnter.LocalKey = LocalizeKey._7055; // 무료 소탕
            }
            else
            {
                btnEnter.LocalKey = LocalizeKey._7027; // 던전 입장
            }

            btnEnter.SetItemCount(StringBuilderPool.Get()
                .Append(freeCount).Append("/").Append(element.GetFreeMaxCount())
                .Release());
            btnEnter.SetCountColor(freeCount > 0);
            btnEnterLock.isEnabled = !canEnter; // 대기 상태이지만 입장 불가능
            btnEnter.SetNotice(freeCount > 0);
            rewardInfos = element.GetRewardInfos(); // 보상 정보

            wrapperReward.Resize(rewardInfos.Length);

            // 던전 상세정보 셋팅
            var infoId = GetDungeonInfoId();
            if (infoId < 0)
            {
                Debug.LogError("DungeonType Error _____ " + element.DungeonType);
                return;
            }

            SetDungeonFreeItem();

            var icons = presenter.GetIcons(infoId);
            var titles = presenter.GetTitles(infoId);
            var descs = presenter.GetDescriptions(infoId);
            for (int i = 0; i < infos.Length; i++)
            {
                infos[i].SetIcon(icons[i]);
                infos[i].SetTitle(titles[i]);
                infos[i].SetDesc(descs[i]);
            }
        }

        private string GetDungeonImage(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return Constants.UITexute.UI_MAP_INFO_MEMORIAL_01;

                case DungeonType.ExpDungeon:
                    return Constants.UITexute.UI_MAP_INFO_MEMORIAL_02;

                case DungeonType.Defence:
                    return Constants.UITexute.UI_MAP_INFO_MEMORIAL_03;

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        private int GetDungeonInfoId()
        {
            switch (element.DungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return DungeonInfoType.ZenyDungeon.GetDungeonInfoId(element.Id);

                case DungeonType.ExpDungeon:
                    return DungeonInfoType.ExpDungeon.GetDungeonInfoId(element.Id);

                case DungeonType.Defence:
                    return DungeonInfoType.Defence.GetDungeonInfoId(element.Id);

                default:
                    return -1;
            }
        }

        private void SetDungeonFreeItem()
        {
            if (PossibleFreeReward())
            {
                iconFreeReward.Mode = UIGraySprite.SpriteMode.None;
                btnFreeReward.SetNotice(true);
            }
            else
            {
                iconFreeReward.Mode = UIGraySprite.SpriteMode.Grayscale;
                btnFreeReward.SetNotice(false);
            }

            switch (element.DungeonType)
            {
                case DungeonType.ZenyDungeon:
                    iconFreeReward.spriteName = Constants.MapAtlas.UI_ICON_REWARD_ZENY;
                    break;

                case DungeonType.ExpDungeon:
                    iconFreeReward.spriteName = Constants.MapAtlas.UI_ICON_REWARD_EXP;
                    break;

                case DungeonType.Defence:
                    iconFreeReward.spriteName = Constants.MapAtlas.UI_ICON_REWARD_AIRSHIP;
                    break;
            }
        }

        private bool PossibleFreeReward()
        {
            return presenter.PossibleFreeReward(element.DungeonType);
        }

        bool IInspectorFinder.Find()
        {
            infos = GetComponentsInChildren<UIDungeonDetailInfoSlot>();
            if (btnFreeReward != null) iconFreeReward = btnFreeReward.GetComponentInChildren<UIGraySprite>();
            return true;
        }
    }
}