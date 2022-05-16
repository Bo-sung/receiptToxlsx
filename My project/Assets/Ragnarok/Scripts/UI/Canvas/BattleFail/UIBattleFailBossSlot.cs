using System;
using UnityEngine;

namespace Ragnarok
{
    public class UIBattleFailBossSlot : MonoBehaviour
    {
        [SerializeField] UILabel titleLabel;
        [SerializeField] UILabel descLabel;
        [SerializeField] UIButton button;

        [SerializeField] GameObject skillSprite;
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UITextureHelper monsterSprite;
        [SerializeField] GameObject dungeonSprite;
        [SerializeField] GameObject chattingSprite;
        [SerializeField] GameObject statusSprite;
        [SerializeField] GameObject secretSprite;
        [SerializeField] GameObject dailySprite;
        [SerializeField] UICardProfile cardProfile;

        private Action action;

        void Awake()
        {
            EventDelegate.Add(button.onClick, OnClickButton);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(button.onClick, OnClickButton);
        }

        public void SetTitle(string title)
        {
            titleLabel.text = title;
        }

        public void SetDesc(string desc)
        {
            descLabel.text = desc;
        }

        public void OpenSkill()
        {
            HideAll();
            skillSprite.SetActive(true);
        }

        public void OpenStat()
        {
            HideAll();
            statusSprite.SetActive(true);
        }

        public void OpenCard(ItemInfo info)
        {
            HideAll();
            cardProfile.gameObject.SetActive(true);
            cardProfile.SetData(info);
        }

        public void OpenSecret()
        {
            HideAll();
            secretSprite.SetActive(true);
        }

        public void OpenDaily()
        {
            HideAll();
            dailySprite.SetActive(true);
        }

        public void OpenItem(ItemInfo info)
        {
            HideAll();
            equipmentProfile.gameObject.SetActive(true);
            equipmentProfile.SetData(info);
        }

        public void OpenMonster(string monsterIcon)
        {
            HideAll();
            monsterSprite.gameObject.SetActive(true);
            monsterSprite.SetMonster(monsterIcon);
        }

        public void OpenDungeon()
        {
            HideAll();
            dungeonSprite.SetActive(true);
        }

        public void OpenChatting()
        {
            HideAll();
            chattingSprite.SetActive(true);
        }

        private void HideAll()
        {
            skillSprite.SetActive(false);
            equipmentProfile.gameObject.SetActive(false);
            monsterSprite.gameObject.SetActive(false);
            dungeonSprite.SetActive(false);
            chattingSprite.SetActive(false);
            statusSprite.SetActive(false);
            secretSprite.SetActive(false);
            dailySprite.SetActive(false);
            cardProfile.gameObject.SetActive(false);
        }

        public void SetOnClick(Action action)
        {
            this.action = action;
        }

        private void OnClickButton()
        {
            action?.Invoke();
        }
    }
}