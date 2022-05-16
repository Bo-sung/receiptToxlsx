using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class UserBookState
    {
        private byte[] itemData;
        private byte[] cardData;
        private byte[] monsterData;
        private byte[] costumeData;

        private int getItemNum;
        private int getCardNum;
        private int getMonsterNum;
        private int getCostumeNum;

        private int itemTabLevel;
        private int cardTabLevel;
        private int monsterTabLevel;
        private int costumeTabLevel;

        private byte[] idxtobit = { 1, 2, 4, 8, 16, 32, 64, (byte)128 };

        public void Initialize(byte[] itemData, byte[] cardData, byte[] monsterData, byte[] costumeData, int itemTabLevel, int cardTabLevel, int monsterTabLevel, int costumeTabLevel)
        {
            this.itemData = itemData;
            this.cardData = cardData;
            this.monsterData = monsterData;
            this.costumeData = costumeData;

            getItemNum = 0;
            foreach (var each in itemData)
                for (int i = 0; i < idxtobit.Length; ++i)
                    if ((each | idxtobit[i]) > 0)
                        ++getItemNum;

            getCardNum = 0;
            foreach (var each in cardData)
                for (int i = 0; i < idxtobit.Length; ++i)
                    if ((each | idxtobit[i]) > 0)
                        ++getCardNum;

            getMonsterNum = 0;
            foreach (var each in monsterData)
                for (int i = 0; i < idxtobit.Length; ++i)
                    if ((each | idxtobit[i]) > 0)
                        ++getMonsterNum;

            getCostumeNum = 0;
            foreach (var each in costumeData)
                for (int i = 0; i < idxtobit.Length; ++i)
                    if ((each | idxtobit[i]) > 0)
                        ++getCostumeNum;

            this.itemTabLevel = itemTabLevel;
            this.cardTabLevel = cardTabLevel;
            this.monsterTabLevel = monsterTabLevel;
            this.costumeTabLevel = costumeTabLevel;
        }
        
        public bool IsRecordedItem(int bookIndex)
        {
            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            return (itemData[arrayidx] & idxtobit[bitidx]) > 0;
        }

        public bool IsRecordedCard(int bookIndex)
        {
            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            return (cardData[arrayidx] & idxtobit[bitidx]) > 0;
        }

        public bool IsRecordedMonster(int bookIndex)
        {
            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            return (monsterData[arrayidx] & idxtobit[bitidx]) > 0;
        }

        public bool IsRecordedCostume(int bookIndex)
        {
            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            return (costumeData[arrayidx] & idxtobit[bitidx]) > 0;
        }

        public bool InputItemDic(int bookIndex)
        {
            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            if ((itemData[arrayidx] & idxtobit[bitidx]) == 0)
            {
                itemData[arrayidx] |= idxtobit[bitidx];
                getItemNum++;
                return true;
            }

            return false;
        }

        public bool InputCardDic(int bookIndex)
        {
            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            if ((cardData[arrayidx] & idxtobit[bitidx]) == 0)
            {
                cardData[arrayidx] |= idxtobit[bitidx];
                getCardNum++;
                return true;
            }

            return false;
        }

        public bool InputMonsterDic(int bookIndex)
        {
            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            if ((monsterData[arrayidx] & idxtobit[bitidx]) == 0)
            {
                monsterData[arrayidx] |= idxtobit[bitidx];
                getMonsterNum++;
                return true;
            }

            return false;
        }

        public bool InputCostumeDic(int bookIndex)
        {
            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            if ((costumeData[arrayidx] & idxtobit[bitidx]) == 0)
            {
                costumeData[arrayidx] |= idxtobit[bitidx];
                getCostumeNum++;
                return true;
            }

            return false;
        }

        public void SetTabLevel(BookTabType tab, int level)
        {
            switch (tab)
            {
                case BookTabType.Equipment:
                    itemTabLevel = level;
                    break;
                case BookTabType.Card:
                    cardTabLevel = level;
                    break;
                case BookTabType.Monster:
                    monsterTabLevel = level;
                    break;
                case BookTabType.Costume:
                    costumeTabLevel = level;
                    break;
            }
        }

        public int GetTabLevel(BookTabType tab)
        {
            switch (tab)
            {
                case BookTabType.Equipment:
                    return itemTabLevel;
                case BookTabType.Card:
                    return cardTabLevel;
                case BookTabType.Monster:
                    return monsterTabLevel;
                case BookTabType.Costume:
                    return costumeTabLevel;
            }

            return 0;
        }
    }
}
