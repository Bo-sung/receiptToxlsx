using System;
using UnityEngine;

namespace Ragnarok
{
    public class UIHextechSlot : MonoBehaviour
    {
        public enum PlusPos { Center, UpperRight }

        [SerializeField] UITextureHelper iconHelper;
        [SerializeField] UILabelHelper countLabel;
        [SerializeField] GameObject plusIcon;
        [SerializeField] GameObject upperRightPlusIcon;
        [SerializeField] UIButton touchButton;
        [SerializeField] UISprite iconElement;
        [SerializeField] UILabelHelper labelElementLevel;

        private int id;
        private Action<int> clickEventHandler;
        public ItemData ShowingItemData { get; private set; }

        private void Start()
        {
            EventDelegate.Add(touchButton.onClick, OnTouch);
        }

        public void Init(int id, Action<int> clickEventHandler)
        {
            this.id = id;
            this.clickEventHandler = clickEventHandler;
        }

        public UIHextechSlot Show()
        {
            ShowingItemData = null;
            gameObject.SetActive(true);
            iconHelper.SetActive(false);
            upperRightPlusIcon.SetActive(false);
            plusIcon.SetActive(false);
            countLabel.gameObject.SetActive(false);

            if (iconElement)
            {
                iconElement.cachedGameObject.SetActive(false);
            }

            return this;
        }

        public UIHextechSlot SetIcon(ItemInfo itemInfo)
        {
            iconHelper.SetActive(true);
            iconHelper.Set(itemInfo.IconName);

            if (iconElement)
            {
                iconElement.cachedGameObject.SetActive(itemInfo.IsElementStone);
                if (itemInfo.IsElementStone)
                {
                    labelElementLevel.Text = itemInfo.GetElementLevelText();
                }
            }

            return this;
        }

        public UIHextechSlot SetIcon(ItemData itemData)
        {
            ShowingItemData = itemData;
            iconHelper.SetActive(true);
            iconHelper.Set(itemData.icon_name);

            if (iconElement)
            {
                int elementStoneLevel = itemData.GetElementStoneLevel();
                bool isElementStone = elementStoneLevel >= 0;
                iconElement.cachedGameObject.SetActive(isElementStone);
                if (isElementStone)
                {
                    labelElementLevel.Text = (elementStoneLevel + 1).ToString(); // 1을 추가하여 보여줌
                }
            }

            return this;
        }

        public UIHextechSlot SetRequiredCount(int selectedCount, int requiredCount)
        {
            countLabel.gameObject.SetActive(true);
            if (selectedCount < requiredCount)
                countLabel.Text = $"[c][D76251]{selectedCount}[-][/c]/{requiredCount}";
            else
                countLabel.Text = $"{selectedCount}/{requiredCount}";
            return this;
        }

        public UIHextechSlot SetRewardCount(RewardData rewardData)
        {
            countLabel.gameObject.SetActive(true);
            countLabel.Text = $"x {rewardData.Count}";
            return this;
        }

        public UIHextechSlot ShowPlus(PlusPos plusPos)
        {
            if (plusPos == PlusPos.UpperRight)
                upperRightPlusIcon.SetActive(true);
            else if (plusPos == PlusPos.Center)
                plusIcon.SetActive(true);
            return this;
        }

        private void OnTouch()
        {
            clickEventHandler?.Invoke(id);
        }
    }
}