using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleBuffItemInfo : List<BattleOption>
    {
        public interface ISettings
        {
            /// <summary>
            /// 아이템 데이터
            /// </summary>
            ItemData Data { get; }

            /// <summary>
            /// 남은 재사용 대기시간
            /// </summary>
            float RemainDuration { get; }
        }

        public readonly List<BuffItemInfo> buffItemList;
        public readonly List<EventBuffInfo> eventBuffList;
        public readonly List<BlessBuffItemInfo> blessBuffItemList;

        public BattleBuffItemInfo()
        {
            buffItemList = new List<BuffItemInfo>();
            eventBuffList = new List<EventBuffInfo>();
            blessBuffItemList = new List<BlessBuffItemInfo>();
        }

        /// <summary>
        /// 정보 리셋
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            buffItemList.Clear();
            eventBuffList.Clear();
            blessBuffItemList.Clear();
        }

        public void Initialize(ISettings[] arrSettings, EventBuffInfo[] arrInfos, IEnumerable<BlessBuffItemInfo> blssBuffItems)
        {
            Clear();

            if (arrSettings != null)
            {
                foreach (var item in arrSettings)
                {
                    Add(item);
                }
            }

            if (arrInfos != null)
            {
                foreach (var item in arrInfos)
                {
                    Add(item);
                }
            }

            if (blssBuffItems != null)
            {
                foreach (var item in blssBuffItems)
                {
                    if (item == null || item.IsInvalidData)
                        continue;

                    blessBuffItemList.Add(item);
                    AddRange(item); // 전투옵션 세팅
                }
            }
        }

        /// <summary>
        /// 유효성 체크
        /// </summary>
        public bool CheckDurationEffect()
        {
            if (buffItemList.Count == 0 && eventBuffList.Count == 0)
                return false;

            bool isDirty = false;

            for (int i = buffItemList.Count - 1; i >= 0; i--)
            {
                if (buffItemList[i].IsValid())
                    continue;

                isDirty = true;
                Remove(buffItemList[i]);
            }

            for (int i = eventBuffList.Count - 1; i >= 0; i--)
            {
                if (eventBuffList[i].remainTime > 0)
                    continue;

                isDirty = true;
                Remove(eventBuffList[i]);
            }

            return isDirty;
        }

        /// <summary>
        /// 특정 버프 아이템 지속시간 유효성
        /// </summary>
        public bool IsValid(int itemId)
        {
            foreach (var item in buffItemList.OrEmptyIfNull())
            {
                if (!item.IsValid())
                    continue;

                if (item.Itemid == itemId)
                    return true;
            }
            return false;
        }

        private void Add(ISettings settings)
        {
            if (settings == null)
                return;

            BuffItemInfo info = Create(settings.Data, settings.RemainDuration);

            if (info == null)
                return;

            buffItemList.Add(info); // 버프아이템 리스트에 추가
            AddRange(info); // 전투옵션 세팅
        }

        private void Remove(BuffItemInfo info)
        {
            buffItemList.Remove(info);
        }

        private void Add(EventBuffInfo info)
        {
            if (info == null)
                return;

            for (int i = 0; i < eventBuffList.Count; i++)
            {
                if (eventBuffList[i].remainTime < info.remainTime)
                {
                    // 먼저 추가된 이벤트보다 남은시간이 많을때는 앞에 추가해 줌.
                    eventBuffList.Insert(i, info);
                    return;
                }
            }

            eventBuffList.Add(info); // 버프아이템 리스트에 추가
        }

        private void Remove(EventBuffInfo info)
        {
            eventBuffList.Remove(info);
        }

        private BuffItemInfo Create(ItemData data, float remainDuration)
        {
            if (data == null)
                return null;

            BuffItemInfo info = new BuffItemInfo();
            info.Initialize(remainDuration);
            info.SetData(data);
            return info;
        }
    }
}