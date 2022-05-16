using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class BoxDataManager : Singleton<BoxDataManager>, IDataManger, BoxDataManager.IBoxDataRepoImpl
    {
        public interface IBoxDataRepoImpl
        {
            RewardData[] ToBoxRewards(RewardData reward);
        }

        private readonly Dictionary<ObscuredInt, BoxData> dataDic;

        public ResourceType DataType => ResourceType.BoxDataDB;

        public BoxDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, BoxData>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    BoxData data = new BoxData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public BoxData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
                throw new System.ArgumentException($"박스 데이터가 존재하지 않습니다: id = {id}");

            return dataDic[id];
        }


        /// <summary>
        /// RewardType이 RefGacha(99)인 상자 리스트 반환
        /// </summary>
        public BoxData[] GetRefGachaTypeBoxes()
        {
            List<BoxData> boxList = new List<BoxData>();

            foreach (var box in dataDic.Values)
            {
                if (!ContainsRewardType(box, RewardType.RefGacha))
                    continue;

                boxList.Add(box);
            }

            return boxList.ToArray();
        }

        bool ContainsRewardType(BoxData boxData, RewardType type)
        {
            if (boxData.type_1 == type.ToIntValue() ||
                boxData.type_2 == type.ToIntValue() ||
                boxData.type_3 == type.ToIntValue() ||
                boxData.type_4 == type.ToIntValue() ||
                boxData.type_5 == type.ToIntValue() ||
                boxData.type_6 == type.ToIntValue() ||
                boxData.type_7 == type.ToIntValue() ||
                boxData.type_8 == type.ToIntValue() ||
                boxData.type_9 == type.ToIntValue() ||
                boxData.type_10 == type.ToIntValue())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 특정 아이템을 주는 상자 리스트 반환
        /// </summary>
        public BoxData[] GetBoxesDropItem(int itemId)
        {
            List<BoxData> boxList = new List<BoxData>();

            foreach (var box in dataDic.Values)
            {
                if (IsBoxDropItem(box, itemId))
                {
                    boxList.Add(box);
                }
            }

            return boxList.ToArray();
        }

        /// <summary>
        /// 상자 반환 값으로 변환
        /// </summary>
        public RewardData[] ToBoxRewards(RewardData reward)
        {
            if (reward == null)
                return null;

            if (reward.RewardType == RewardType.Item)
            {
                // 박스의 경우
                ItemData itemData = reward.ItemData;
                if (itemData != null && itemData.ItemType == ItemType.Box)
                {
                    int boxId = itemData.event_id;
                    BoxData boxData = Get(boxId);
                    if (boxData != null)
                        return boxData.rewards;
                }
            }

            return new RewardData[] { reward };
        }

        /// <summary>
        /// 해당 상자가 특정 아이템을 주는지.
        /// </summary>
        private bool IsBoxDropItem(BoxData boxData, int itemId)
        {
            if (boxData == null)
                return false;

            if (boxData.value_1 == itemId ||
                boxData.value_2 == itemId ||
                boxData.value_3 == itemId ||
                boxData.value_4 == itemId ||
                boxData.value_5 == itemId ||
                boxData.value_6 == itemId ||
                boxData.value_7 == itemId ||
                boxData.value_8 == itemId ||
                boxData.value_9 == itemId ||
                boxData.value_10 == itemId)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                foreach (var reward in item.rewards)
                {
                    if (reward.RewardType == RewardType.Item)
                    {
                        if (ItemDataManager.Instance.Get(reward.ItemId) == null)
                        {
                            throw new System.Exception($"12.상자 테이블 오류 ID={item.id}, 없는 아이템={reward.ItemId}");
                        }
                    }
                }
            }
#endif
        }
    }
}