using UnityEngine;

namespace Ragnarok
{
    public class UIRewardHelper : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] protected UITextureHelper icon;
        [SerializeField] protected UIButtonHelper button;
        [SerializeField] protected UIGridHelper rate;
        [SerializeField] protected UISprite background; // 로그인 보너스에서는 배경 사용하지 않음.
        [SerializeField] protected UILabelHelper tierupRate;
        [SerializeField] UISprite iconAgent;
        [SerializeField] UISprite iconElement;
        [SerializeField] UILabelHelper labelElementLevel;
        [SerializeField] GameObject goEvent;

        protected RewardData rewardData;

        public bool IsEnabled
        {
            get => button.IsEnabled;
            set => button.IsEnabled = value;
        }

        public bool UseDefaultButtonEvent { get; set; } = true;

        protected virtual void Awake()
        {
            if (button)
                EventDelegate.Add(button.OnClick, OnClickedBtnShowInfo);
        }

        protected virtual void OnDestroy()
        {
            if (button)
                EventDelegate.Remove(button.OnClick, OnClickedBtnShowInfo);

            RemoveEvent();
        }

        protected virtual void OnEnable()
        {
            if (IsInvalid())
                return;

            if (icon)
                icon.Set(rewardData.IconName);
        }

        protected virtual void AddEvent()
        {
        }

        protected virtual void RemoveEvent()
        {
        }

        public void SetData(RewardData rewardData)
        {
            RemoveEvent();
            this.rewardData = rewardData;
            AddEvent();

            UpdateView();
        }

        protected bool IsInvalid()
        {
            return rewardData == null || rewardData.RewardType == RewardType.None;
        }

        protected virtual void UpdateView()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            if (icon)
                icon.Set(rewardData.IconName);

            if (button)
            {
                if (rewardData.RewardType == RewardType.Agent)
                {
                    bool isNew = Entity.player.Agent.IsNewAgent(rewardData.ItemId);

                    if (isNew)
                    {
                        if (rewardData.Count == 0)
                        {
                            button.LocalKey = LocalizeKey._4200; // New
                        }
                        else
                        {
                            button.Text = LocalizeKey._4201.ToText().Replace(ReplaceKey.COUNT, rewardData.Count); // New+{COUNT}
                        }
                    }
                    else if (rewardData.Count > 1)
                    {
                        button.Text = rewardData.Count.ToString();
                    }
                    else
                    {
                        button.Text = string.Empty;
                    }
                }
                else if (rewardData.RewardType == RewardType.Item)
                {
                    if (rewardData.Count > 1)
                    {
                        button.Text = rewardData.Count.ToString();
                    }
                    else
                    {
                        button.Text = string.Empty;
                    }
                }
                else
                {
                    button.Text = rewardData.Count.ToString();
                }

                // 장비 아이템의 경우 배경 스프라이트 변경
                if (background)
                {
                    if (IsEquipment())
                    {
                        button.SpriteName = GetBackSpriteName(rewardData.Rating, tierupRate != null && rewardData.Transcend > 0, rewardData.IsShadow());
                    }
                    else if(IsCard())
                    {
                        button.SpriteName = GetBackSpriteName(rating: 1, isTierUp: false, rewardData.IsShadow());
                    }
                    else
                    {
                        button.SpriteName = GetBackSpriteName(rating: 1, isTierUp: false, isShadow: false);
                    }
                }
            }

            if (tierupRate != null)
                tierupRate.gameObject.SetActive(false);

            if (rate)
            {
                if (IsEquipment() && rewardData.Transcend > 0 && tierupRate != null)
                {
                    rate.SetValue(0);
                    tierupRate.gameObject.SetActive(true);
                    tierupRate.Text = rewardData.Transcend.ToString();
                }
                else
                {
                    rate.SetValue(rewardData.Rating);
                }
            }

            // 장비 아이템의 경우 배경 스프라이트 변경
            if (background)
            {
                if (IsEquipment())
                {
                    background.spriteName = GetBackSpriteName(rewardData.Rating, tierupRate != null && rewardData.Transcend > 0, rewardData.IsShadow());
                }
                else if (IsCard())
                {
                    background.spriteName = GetBackSpriteName(rating: 1, isTierUp: false, rewardData.IsShadow());
                }
                else
                {
                    background.spriteName = GetBackSpriteName(rating: 1, isTierUp: false, isShadow: false);
                }
            }

            if (iconAgent)
            {
                AgentType agentType = rewardData.GetAgentType();
                if (agentType == AgentType.None)
                {
                    NGUITools.SetActive(iconAgent.cachedGameObject, false);
                }
                else
                {
                    NGUITools.SetActive(iconAgent.cachedGameObject, true);
                    iconAgent.spriteName = GetAgentIconName(agentType);
                }
            }

            if (iconElement)
            {
                int elementStoneLevel = rewardData.GetElementStoneLevel();
                bool isElementStone = elementStoneLevel >= 0;
                iconElement.cachedGameObject.SetActive(isElementStone);
                if (isElementStone)
                {
                    labelElementLevel.Text = (elementStoneLevel + 1).ToString(); // 1을 추가하여 보여줌
                }
            }

            NGUITools.SetActive(goEvent, rewardData.IsEvent);
        }

        protected void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        // 보상 상세정보 보기
        public virtual void OnClickedBtnShowInfo()
        {
            if (Tutorial.isInProgress)
            {
                UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
                return;
            }

            if (IsInvalid())
                return;

            if (!UseDefaultButtonEvent)
                return;

            if (rewardData.RewardType == RewardType.Item)
            {
                switch (rewardData.ItemGroupType)
                {
                    case ItemGroupType.Equipment:
                        {
                            EquipmentItemInfo info = new EquipmentItemInfo(Entity.player.Inventory);
                            info.SetData(rewardData.ItemData);
                            info.ForceSetItemTranscend(rewardData.Transcend);
                            info.ForceSetChangedElement(rewardData.ChangedElement, rewardData.ElementLevel);
                            UI.Show<UIEquipmentInfoSimple>(info);
                        }
                        break;

                    case ItemGroupType.Card:
                        {
                            ItemInfo info = new CardItemInfo();
                            info.SetData(rewardData.ItemData);
                            UI.Show<UICardInfoSimple>().Show(info);
                        }
                        break;

                    case ItemGroupType.MonsterPiece:
                        {
                            ItemInfo info = new MonsterPieceItemInfo();
                            info.SetData(rewardData.ItemData);
                            UI.Show<UIPartsInfo>(info);
                        }
                        break;

                    case ItemGroupType.ProductParts:
                    case ItemGroupType.ConsumableItem:
                        {
                            ItemInfo info = new PartsItemInfo();
                            info.SetData(rewardData.ItemData);
                            UI.Show<UIPartsInfo>(info);
                        }
                        break;
                    case ItemGroupType.Costume:
                        {
                            ItemInfo info = new CostumeItemInfo();
                            info.SetData(rewardData.ItemData);
                            UI.Show<UICostumeInfo>().Set(info);
                        }
                        break;
                }
            }

            if (rewardData.RewardType == RewardType.Agent)
            {
                UI.Show<UIAgentDetailInfo>(new UIAgentDetailInfo.Input(rewardData.AgentData, true));
            }
            else
            {
                switch (rewardData.RewardType)
                {
                    case RewardType.Zeny:
                    case RewardType.CatCoin:
                    case RewardType.CatCoinFree:
                    case RewardType.JobExp:
                    case RewardType.LevelExp:
                    case RewardType.ROPoint:
                    case RewardType.CharacterShareChargeItem1:
                    case RewardType.CharacterShareChargeItem2:
                    case RewardType.CharacterShareChargeItem3:
                    case RewardType.MultiMazeTicket:
                    case RewardType.SummonMvpTicket:
                    case RewardType.GuildCoin:
                    case RewardType.ShareReward:
                    case RewardType.InvenWeight:
                    case RewardType.TreeReward:
                    case RewardType.SkillPoint:
                    case RewardType.EventMultiMazeTicket:
                    case RewardType.DefDungeonTicket:
                    case RewardType.ExpEungeonTicket:
                    case RewardType.ZenyDungeonTicket:
                    case RewardType.PassExp:
                    case RewardType.BattlePass:
                    case RewardType.OnBuffPassExp:
                    case RewardType.OnBuffPoint:
                    case RewardType.OnBuffPointMail:
                    case RewardType.OnBuffPass:
                        UI.Show<UIGoodsInfo>().SetData(rewardData.RewardType);
                        break;
                }
            }
        }

        public void Launch(UIRewardLauncher.GoodsDestination destination)
        {
            if (rewardData == null)
                return;

            switch (rewardData.RewardType)
            {
                case RewardType.Zeny:
                case RewardType.CatCoin:
                case RewardType.JobExp:
                case RewardType.LevelExp:
                case RewardType.ROPoint:
                    UI.LaunchReward(transform.position, rewardData.RewardType, rewardData.Count, destination);
                    break;

                default:
                    UI.LaunchReward(transform.position, rewardData.IconName, rewardData.Count, destination);
                    break;
            }

            SetData(null);
        }

        bool IsEquipment()
        {
            if (rewardData.RewardType == RewardType.Item && rewardData.ItemGroupType == ItemGroupType.Equipment)
                return true;

            return false;
        }

        bool IsCard()
        {
            if (rewardData.RewardType == RewardType.Item && rewardData.ItemGroupType == ItemGroupType.Card)
                return true;

            return false;
        }

        string GetBackSpriteName(int rating, bool isTierUp, bool isShadow)
        {
            if(isShadow)
                return Constants.CommonAtlas.UI_COMMON_BG_ITEM_SHADOW;

            if (isTierUp)
                return Constants.CommonAtlas.UI_COMMON_BG_ITEM_06;

            switch (rating)
            {
                case 1: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_01;
                case 2: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_02;
                case 3: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_03;
                case 4: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_04;
                case 5: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_05;
            }
            return default;
        }

        private string GetAgentIconName(AgentType agentType)
        {
            switch (agentType)
            {
                case AgentType.CombatAgent:
                    return Constants.CommonAtlas.UI_COMMON_INFO;
                case AgentType.ExploreAgent:
                    return Constants.CommonAtlas.UI_COMMON_AGENT;
            }

            return string.Empty;
        }
    }
}