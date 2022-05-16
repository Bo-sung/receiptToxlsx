using MsgPack;

namespace Ragnarok
{
    public sealed class ElementDataManager : Singleton<ElementDataManager>, IDataManger, IElementDamage
    {
        private readonly BetterList<ElementData> dataList;

        public ResourceType DataType => ResourceType.ElementDataDB;

        public ElementDataManager()
        {
            dataList = new BetterList<ElementData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ElementData data = new ElementData(mpo.AsList());
                    dataList.Add(data);
                }
            }
        }

        /// <summary>
        /// 속성댐 배율
        /// </summary>
        public int Get(ElementType attackerType, int attackerLevel, ElementType defenderType, int defenderLevel)
        {
            int attackerTypeValue = (int)attackerType;
            int defenderTypeValue = (int)defenderType;

            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].attacker_type == attackerTypeValue && dataList[i].attacker_level == attackerLevel && dataList[i].defender_type == defenderTypeValue && dataList[i].defender_level == defenderLevel)
                    return dataList[i].damage_value;
            }

            return 0;
        }

        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            ItemDataManager itemDataRepo = ItemDataManager.Instance;

            // 속성석 체크
            var verifyDataList = new System.Collections.Generic.List<(int attackerType, int attackerLevel, int defenderType, int defenderLevel)>();
            foreach (ElementType attackerType in System.Enum.GetValues(typeof(ElementType)))
            {
                if (attackerType == ElementType.None)
                    continue;

                int attackerMaxLevel = -1;
                while (true)
                {
                    ItemData elementStone = itemDataRepo.GetElementStone(attackerType, attackerMaxLevel + 1);
                    if (elementStone == null)
                        break;

                    ++attackerMaxLevel;
                }

                foreach (ElementType defenderType in System.Enum.GetValues(typeof(ElementType)))
                {
                    if (defenderType == ElementType.None)
                        continue;

                    int defenceMaxLevel = -1;
                    while (true)
                    {
                        ItemData elementStone = itemDataRepo.GetElementStone(attackerType, defenceMaxLevel + 1);
                        if (elementStone == null)
                            break;

                        ++defenceMaxLevel;
                    }

                    for (int attackerLevel = 0; attackerLevel <= attackerMaxLevel; attackerLevel++)
                    {
                        for (int defenderLevel = 0; defenderLevel <= defenceMaxLevel; defenderLevel++)
                        {
                            verifyDataList.Add(((int)attackerType, attackerLevel, (int)defenderType, defenderLevel));
                        }
                    }
                }
            }

            foreach (var item in dataList)
            {
                int index = verifyDataList.FindIndex(a => a.attackerType == item.attacker_type && a.attackerLevel == item.attacker_level && a.defenderType == item.defender_type && a.defenderLevel == item.defender_level);
                if (index == -1)
                    continue;

                verifyDataList.RemoveAt(index);
            }

            if (verifyDataList.Count > 0)
            {
                var sb = StringBuilderPool.Get();
                for (int i = 0; i < verifyDataList.Count; i++)
                {
                    sb.AppendLine();
                    sb.Append(verifyDataList[i].attackerType)
                        .Append("_")
                        .Append(verifyDataList[i].attackerLevel)
                        .Append("_")
                        .Append(verifyDataList[i].defenderType)
                        .Append("_")
                        .Append(verifyDataList[i].defenderLevel);
                }

                throw new System.Exception($"68.속성 테이블 오류: 없는 속성 데이터 = {sb.Release()}");
            }
#endif
        }
    }
}