using System;
using System.Threading.Tasks;
using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIKafraDelivery"/>
    /// </summary>
    public class KafraDeliveryPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly QuestModel questModel;

        // <!-- Repositories --!>

        private BetterList<KafraListElement> kafraListElements;

        // <!-- Event --!>
        public event Action OnUpdateRewardCount;

        public event Action OnUpdateKafra
        {
            add { questModel.OnUpdateKafra += value; }
            remove { questModel.OnUpdateKafra -= value; }
        }

        public KafraDeliveryPresenter()
        {
            questModel = Entity.player.Quest;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
            if (kafraListElements != null)
            {
                for (int i = 0; i < kafraListElements.size; i++)
                {
                    kafraListElements[i].OnUpdateSelectedCount -= OnUpdateSelectedCount;
                }
                kafraListElements.Clear();
            }
        }

        /// <summary>
        /// 카프라 운송 상태 타입
        /// </summary>
        public KafraCompleteType GetKafraCompleteType()
        {
            return questModel.KafraCompleteType;
        }

        /// <summary>
        /// 진행중인 카프라 타입
        /// </summary>
        public KafraType GetKafraType()
        {
            return questModel.CurKafraType;
        }

        public UIKafraListElement.IInput[] GetArrayData(KafraType type)
        {
            if (kafraListElements != null)
            {
                for (int i = 0; i < kafraListElements.size; i++)
                {
                    kafraListElements[i].OnUpdateSelectedCount -= OnUpdateSelectedCount;
                }
                kafraListElements.Clear();
            }
            else
            {
                kafraListElements = new BetterList<KafraListElement>();
            }

            KafExchangeData[] arrayData = KafExchangeDataManager.Instance.GetArrayData(type);
            foreach (var item in arrayData)
            {
                KafraListElement temp = new KafraListElement(item);
                temp.OnUpdateSelectedCount += OnUpdateSelectedCount;

                kafraListElements.Add(temp);
            }
            return kafraListElements.ToArray();
        }

        public int GetRewardCount()
        {
            int totalCount = 0;
            if (kafraListElements != null)
            {
                foreach (var item in kafraListElements)
                {
                    totalCount += item.GetRewardCount();
                }
            }
            return totalCount;
        }

        /// <summary>
        /// 선택 카운트 변경 시 호출
        /// </summary>
        private void OnUpdateSelectedCount()
        {
            OnUpdateRewardCount?.Invoke();
        }

        public async Task AsyncRequestKafraDelivery()
        {
            BetterList<(int id, int count)> list = new BetterList<(int id, int count)>();
            foreach (var item in kafraListElements)
            {
                if (item.Count == 0)
                    continue;

                list.Add((item.Id, item.Count));
            }

            if (list.size == 0)
                return;

            await questModel.RequestKafraDelivery(list.ToArray());
        }

        public UIKafraInProgressElement.IInput[] GetArrayData()
        {
            BetterList<KafraInProgressElement> kafraInProgressElements = new BetterList<KafraInProgressElement>();

            KafExchangeData[] arrayData = KafExchangeDataManager.Instance.GetArrayData(questModel.CurKafraType);
            foreach (KafExchangeData data in arrayData)
            {
                KafraDeliveryPacket packet = GetPacket(data.id);
                if (packet != null)
                {
                    KafraInProgressElement temp = new KafraInProgressElement(data, packet.Count);
                    kafraInProgressElements.Add(temp);
                }
                else
                {
                    KafraInProgressElement temp = new KafraInProgressElement(data, 0);
                    kafraInProgressElements.Add(temp);
                }
            }

            KafraDeliveryPacket GetPacket(int id)
            {
                foreach (KafraDeliveryPacket item in questModel.KafraDeliveryList)
                {
                    if (id == item.Id)
                        return item;
                }
                return null;
            }

            return kafraInProgressElements.ToArray();
        }

        public class KafraListElement : UIKafraListElement.IInput
        {
            private readonly KafExchangeData data;

            public int Id => data.id;
            RewardData UIKafraListElement.IInput.Result => data.result;
            RewardData UIKafraListElement.IInput.Material => data.material;

            private int count = 0;
            public int Count
            {
                get { return count; }
                set
                {
                    if (count == value)
                        return;

                    count = value;
                    OnUpdateSelectedCount?.Invoke();
                }
            }

            public int GetRewardCount()
            {
                return data.result.Count * Count;
            }

            public KafraType GetKafraType()
            {
                return data.type.ToEnum<KafraType>();
            }

            public event Action OnUpdateSelectedCount;

            public KafraListElement(KafExchangeData data)
            {
                this.data = data;
            }
        }

        public class KafraInProgressElement : UIKafraInProgressElement.IInput
        {
            private readonly KafExchangeData data;

            RewardData UIKafraInProgressElement.IInput.Result => data.result;
            RewardData UIKafraInProgressElement.IInput.Material => data.material;

            private int count = 0;
            public int Count
            {
                get { return count; }
            }

            public int GetRewardCount()
            {
                return data.result.Count * Count;
            }

            public KafraType GetKafraType()
            {
                return data.type.ToEnum<KafraType>();
            }

            public event Action OnUpdateSelectedCount;

            public KafraInProgressElement(KafExchangeData data, int count)
            {
                this.data = data;
                this.count = count;
            }
        }
    }
}