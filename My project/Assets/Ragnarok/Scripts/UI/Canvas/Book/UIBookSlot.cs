using System;
using UnityEngine;

namespace Ragnarok
{
    public class UIBookSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] GameObject root;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UISprite bgSprite;
        [SerializeField] UIGridHelper rate;
        [SerializeField] GameObject mask;
        [SerializeField] UIButtonHelper slotButton;
        [SerializeField] UISprite element;
        [SerializeField] UISizeType unitSize;

        private BookStateDecoratedData curData;
        private ItemInfo itemInfoData;
        private MonsterData monsterData;

        private int index;
        private Action<int> onClick;

        private void Start()
        {
            EventDelegate.Add(slotButton.OnClick, OnClickSlot);
        }

        public void SetData(int index, BookStateDecoratedData data, Action<int> onClick)
        {
            this.index = index;
            root.SetActive(true);
            curData = data;
            this.onClick = onClick;

            itemInfoData = data.GetData<ItemInfo>();
            monsterData = data.GetData<MonsterData>();

            mask.SetActive(!data.isRecorded);

            if (itemInfoData != null)
            {
                bgSprite.spriteName = itemInfoData.GetBackSpriteName(false);
                icon.Set(itemInfoData.IconName);
                rate.SetActive(itemInfoData.ItemType == ItemType.Equipment || itemInfoData.ItemType == ItemType.Card);
                rate.SetValue(itemInfoData.Rating);
                element.gameObject.SetActive(false);
                unitSize.gameObject.SetActive(false);
            }

            if (monsterData != null)
            {
                bgSprite.spriteName = "Ui_Common_BG_Item_01";
                icon.SetMonster(monsterData.icon_name);
                element.gameObject.SetActive(true);
                element.spriteName = monsterData.element_type.ToEnum<ElementType>().GetIconName();
                unitSize.gameObject.SetActive(true);
                unitSize.Set(monsterData.cost.ToUnitSizeType());
                rate.SetActive(false);
            }
        }

        public void Refresh()
        {
            if (curData == null)
                return;

            SetData(index, curData, onClick);
        }

        public void SetEmpty()
        {
            curData = null;
            root.SetActive(false);
        }

        private void OnClickSlot()
        {
            if (curData == null)
                return;

            onClick?.Invoke(index);
        }
    }
}