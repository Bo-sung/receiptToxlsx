using Sfs2X.Entities.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class BookModel : CharacterEntityModel
    {
        private readonly byte[] idxtobit = { 1, 2, 4, 8, 16, 32, 64, 128 };

        private byte[] itemData;
        private byte[] cardData;
        private byte[] monsterData;
        private byte[] costumeData;
        private byte[] specialData;
        private byte[] onBuffData;

        private int getItemNum;
        private int getCardNum;
        private int getMonsterNum;
        private int getCostumeNum;
        private int getSpecialNum;
        private int getOnBuffNum;

        private int itemTabLevel;
        private int cardTabLevel;
        private int monsterTabLevel;
        private int costumeTabLevel;
        private int specialTabLevel;
        private int onBuffTabLevel;

        private readonly BookDataManager bookRepo;
        public readonly List<BattleOption> applyingBattleOptions = new List<BattleOption>();

        public IEnumerable<BattleOption> BattleOptions => applyingBattleOptions;

        public event Action<BookTabType> OnBookStateChange;
        public event Action OnBookStateRefreshed;
        public event Action OnStatusReloadRequired;

        public BookModel()
        {
            bookRepo = BookDataManager.Instance;
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        public override void ResetData()
        {
            base.ResetData();
            applyingBattleOptions.Clear();
        }

        internal void Initialize(BookPacket packet)
        {
            var sfsObject = packet.response;

            itemData = sfsObject.GetByteArray("1").Bytes;
            cardData = sfsObject.GetByteArray("2").Bytes;
            monsterData = sfsObject.GetByteArray("3").Bytes;
            costumeData = sfsObject.GetByteArray("4").Bytes;
            specialData = sfsObject.GetByteArray("13").Bytes;

            getItemNum = 0;
            foreach (byte each in itemData)
            {
                for (int i = 0; i < idxtobit.Length; ++i)
                {
                    if ((each & idxtobit[i]) > 0) // 도감 장비 획득
                        ++getItemNum;
                }
            }

            getCardNum = 0;
            foreach (byte each in cardData)
            {
                for (int i = 0; i < idxtobit.Length; ++i)
                {
                    if ((each & idxtobit[i]) > 0) // 도감 카드 획득
                        ++getCardNum;
                }
            }

            getMonsterNum = 0;
            foreach (byte each in monsterData)
            {
                for (int i = 0; i < idxtobit.Length; ++i)
                {
                    if ((each & idxtobit[i]) > 0) // // 도감 몬스터 획득
                        ++getMonsterNum;
                }
            }

            getCostumeNum = 0;
            foreach (byte each in costumeData)
            {
                for (int i = 0; i < idxtobit.Length; ++i)
                {
                    if ((each & idxtobit[i]) > 0) // 도감 코스튬 획득
                        ++getCostumeNum;
                }
            }

            getSpecialNum = 0;
            foreach (byte each in specialData)
            {
                for (int i = 0; i < idxtobit.Length; ++i)
                {
                    if ((each & idxtobit[i]) > 0) // 도감 스페셜 획득
                        ++getSpecialNum;
                }
            }

            Debug.LogWarning($"[BookModel] [{getItemNum}, {getCardNum}, {getMonsterNum}, {getCostumeNum}, {getSpecialNum}] {packet.response.GetDump()}");

            itemTabLevel = sfsObject.GetShort("9");
            cardTabLevel = sfsObject.GetShort("10");
            monsterTabLevel = sfsObject.GetShort("11");
            costumeTabLevel = sfsObject.GetShort("12");
            specialTabLevel = sfsObject.GetShort("15");

            Buffer<BattleOption> buffer = new Buffer<BattleOption>();
            for (int i = 1; i <= itemTabLevel; ++i)
            {
                BookData bookData = bookRepo.GetBookRewardData(BookTabType.Equipment, i);
                if (bookData == null)
                    continue;

                buffer.Add(bookData.GetOption());
            }

            for (int i = 1; i <= cardTabLevel; ++i)
            {
                BookData bookData = bookRepo.GetBookRewardData(BookTabType.Card, i);
                if (bookData == null)
                    continue;

                buffer.Add(bookData.GetOption());
            }

            for (int i = 1; i <= monsterTabLevel; ++i)
            {
                BookData bookData = bookRepo.GetBookRewardData(BookTabType.Monster, i);
                if (bookData == null)
                    continue;

                buffer.Add(bookData.GetOption());
            }

            for (int i = 1; i <= costumeTabLevel; ++i)
            {
                BookData bookData = bookRepo.GetBookRewardData(BookTabType.Costume, i);
                if (bookData == null)
                    continue;

                buffer.Add(bookData.GetOption());
            }

            for (int i = 1; i <= specialTabLevel; ++i)
            {
                BookData bookData = bookRepo.GetBookRewardData(BookTabType.Special, i);
                if (bookData == null)
                    continue;

                buffer.Add(bookData.GetOption());
            }

            if (GameServerConfig.IsOnBuff())
            {
                onBuffData = sfsObject.GetByteArray("13").Bytes;
                onBuffTabLevel = sfsObject.GetShort("15");

                getOnBuffNum = 0;
                foreach (byte each in onBuffData)
                {
                    for (int i = 0; i < idxtobit.Length; ++i)
                    {
                        if ((each & idxtobit[i]) > 0) // 도감 온버프 획득
                            ++getOnBuffNum;
                    }
                }

                for (int i = 1; i <= onBuffTabLevel; ++i)
                {
                    BookData bookData = bookRepo.GetBookRewardData(BookTabType.OnBuff, i);
                    if (bookData == null)
                        continue;

                    buffer.Add(bookData.GetOption());
                }
            }

            Initialize(buffer.GetBuffer(isAutoRelease: true));

            OnBookStateRefreshed?.Invoke();
        }

        internal void Initialize(BattleOption[] bookOptions)
        {
            applyingBattleOptions.Clear();
            applyingBattleOptions.AddRange(bookOptions);
        }

        public async Task<bool> RequestLevelUp(BookTabType tabType)
        {
            SFSObject par = SFSObject.NewInstance();
            int level = GetTabLevel(tabType) + 1;
            par.PutInt("1", (int)tabType);
            par.PutInt("2", level);

            var response = await Protocol.GET_DICTIONARY_BUF.SendAsync(par);

            if (response.isSuccess)
            {
                int val = response.GetInt("1");

                if (val == -1)
                {
                    if (response.ContainsKey("cd"))
                    {
                        Initialize(response.GetPacket<BookPacket>("cd"));
                        OnStatusReloadRequired?.Invoke();
                    }

                    return false;
                }
                else
                {
                    SetTabLevel(tabType, level);

                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool IsRecorded(BookTabType tabType, int bookIndex)
        {
            byte[] data = null;

            switch (tabType)
            {
                case BookTabType.Equipment:
                    data = itemData;
                    break;

                case BookTabType.Card:
                    data = cardData;
                    break;

                case BookTabType.Monster:
                    data = monsterData;
                    break;

                case BookTabType.Costume:
                    data = costumeData;
                    break;

                case BookTabType.Special:
                    data = specialData;
                    break;

                case BookTabType.OnBuff:
                    data = onBuffData;
                    break;
            }

            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            return (data[arrayidx] & idxtobit[bitidx]) > 0;
        }

        public bool Record(BookTabType tabType, int bookIndex)
        {
            byte[] data = null;

            switch (tabType)
            {
                case BookTabType.Equipment:
                    data = itemData;
                    break;

                case BookTabType.Card:
                    data = cardData;
                    break;

                case BookTabType.Monster:
                    data = monsterData;
                    break;

                case BookTabType.Costume:
                    data = costumeData;
                    break;

                case BookTabType.Special:
                    data = specialData;
                    break;

                case BookTabType.OnBuff:
                    data = onBuffData;
                    break;
            }

            if (data == null)
                return false;

            int arrayidx = bookIndex / 8;
            int bitidx = bookIndex % 8;

            if ((data[arrayidx] & idxtobit[bitidx]) == 0)
            {
                data[arrayidx] |= idxtobit[bitidx];

                switch (tabType)
                {
                    case BookTabType.Equipment:
                        getItemNum++;
                        break;
                    case BookTabType.Card:
                        getCardNum++;
                        break;
                    case BookTabType.Monster:
                        getMonsterNum++;
                        break;
                    case BookTabType.Costume:
                        getCostumeNum++;
                        break;
                    case BookTabType.Special:
                        getSpecialNum++;
                        break;
                    case BookTabType.OnBuff:
                        getOnBuffNum++;
                        break;
                }

                OnBookStateChange?.Invoke(tabType);
                return true;
            }

            return false;
        }

        private void SetTabLevel(BookTabType tab, int level)
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

                case BookTabType.Special:
                    specialTabLevel = level;
                    break;

                case BookTabType.OnBuff:
                    onBuffTabLevel = level;
                    break;
            }

            BookData bookData = bookRepo.GetBookRewardData(tab, level);
            if (bookData != null)
                applyingBattleOptions.Add(bookData.GetOption());

            OnBookStateChange?.Invoke(tab);
            OnStatusReloadRequired?.Invoke();
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

                case BookTabType.Special:
                    return specialTabLevel;

                case BookTabType.OnBuff:
                    return onBuffTabLevel;
            }

            return 0;
        }

        public int GetTabRecordCount(BookTabType tab)
        {
            switch (tab)
            {
                case BookTabType.Equipment:
                    return getItemNum;

                case BookTabType.Card:
                    return getCardNum;

                case BookTabType.Monster:
                    return getMonsterNum;

                case BookTabType.Costume:
                    return getCostumeNum;

                case BookTabType.Special:
                    return getSpecialNum;

                case BookTabType.OnBuff:
                    return getOnBuffNum;
            }

            return 0;
        }

        public bool IsThereAvailableLevelUp()
        {
            BookData data = null;

            data = bookRepo.GetBookRewardData(BookTabType.Equipment, itemTabLevel + 1);
            if (data != null && data.score <= getItemNum)
                return true;

            data = bookRepo.GetBookRewardData(BookTabType.Card, cardTabLevel + 1);
            if (data != null && data.score <= getCardNum)
                return true;

            data = bookRepo.GetBookRewardData(BookTabType.Monster, monsterTabLevel + 1);
            if (data != null && data.score <= getMonsterNum)
                return true;

            data = bookRepo.GetBookRewardData(BookTabType.Costume, costumeTabLevel + 1);
            if (data != null && data.score <= getCostumeNum)
                return true;

            data = bookRepo.GetBookRewardData(BookTabType.Special, specialTabLevel + 1);
            if (data != null && data.score <= getSpecialNum)
                return true;

            if (GameServerConfig.IsOnBuff())
            {
                data = bookRepo.GetBookRewardData(BookTabType.OnBuff, onBuffTabLevel + 1);
                if (data != null && data.score <= getOnBuffNum)
                    return true;
            }

            return false;
        }
    }
}
