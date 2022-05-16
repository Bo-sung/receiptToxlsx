using MsgPack;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public class CupetPositionDataManager : Singleton<CupetPositionDataManager>, IDataManger, IEqualityComparer<CupetType>
    {
        private readonly BetterList<CupetPositionData> dataList;
        private readonly Dictionary<CupetType, CupetAutoStatusData[]> dataDic;

        public ResourceType DataType => ResourceType.CupetPositionDataDB;

        public CupetPositionDataManager()
        {
            dataList = new BetterList<CupetPositionData>();
            dataDic = new Dictionary<CupetType, CupetAutoStatusData[]>(this);
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
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    dataList.Add(new CupetPositionData(mpo.AsList()));
                }
            }
        }

        public CupetAutoStatusData Get(CupetType cupetType, int level)
        {
            return dataDic.ContainsKey(cupetType) ? dataDic[cupetType][level - 1] : null;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            int maxRank = BasisType.CUPET_MAX_RANK.GetInt();
            int cupetMaxLevel = BasisType.MAX_CUPET_LEVEL.GetInt(maxRank);
            int statPoint = BasisCupetInfo.StatPointbyLevel.GetInt();

            List<(StatusType statusType, int rank, int guide)> tuples = new List<(StatusType statusType, int rank, int guide)>();

            foreach (var item in dataList)
            {
                byte str = 0;
                byte agi = 0;
                byte vit = 0;
                byte @int = 0;
                byte dex = 0;
                byte luk = 0;

                CupetType cupetType = item.cupet_type_id.ToEnum<CupetType>();
                dataDic.Add(cupetType, new CupetAutoStatusData[cupetMaxLevel + 1]); // 0레벨부터 포함시키기 위해
                dataDic[cupetType][0] = new CupetAutoStatusData(str, agi, vit, @int, dex, luk);

                int total = 0;

                int guide_str = item.guide_str;
                int guide_agi = item.guide_agi;
                int guide_vit = item.guide_vit;
                int guide_int = item.guide_int;
                int guide_dex = item.guide_dex;
                int guide_luk = item.guide_luk;

                // 가이드 스탯 비율 총합
                int totalGuide = guide_str + guide_agi + guide_vit + guide_int + guide_dex + guide_luk;

                for (int i = 1; i <= cupetMaxLevel; i++)
                {
                    for (int j = 0; j < statPoint; j++)
                    {
                        // 다음 스탯에 필요한 필요한 비율 우선순위 
                        total++;
                        int rank1 = (int)((guide_str * (total) / (float)totalGuide - str) * 100);
                        int rank2 = (int)((guide_agi * (total) / (float)totalGuide - agi) * 100);
                        int rank3 = (int)((guide_vit * (total) / (float)totalGuide - vit) * 100);
                        int rank4 = (int)((guide_int * (total) / (float)totalGuide - @int) * 100);
                        int rank5 = (int)((guide_dex * (total) / (float)totalGuide - dex) * 100);
                        int rank6 = (int)((guide_luk * (total) / (float)totalGuide - luk) * 100);

                        tuples.Add((StatusType.Str, rank1, guide_str));
                        tuples.Add((StatusType.Agi, rank2, guide_agi));
                        tuples.Add((StatusType.Vit, rank3, guide_vit));
                        tuples.Add((StatusType.Int, rank4, guide_int));
                        tuples.Add((StatusType.Dex, rank5, guide_dex));
                        tuples.Add((StatusType.Luk, rank6, guide_luk));

                        var result = from pair in tuples
                                     orderby pair.rank descending, pair.guide descending
                                     select pair;

                        StatusType type = result.First().statusType;

                        switch (type)
                        {
                            case StatusType.Str:
                                str++;
                                break;

                            case StatusType.Agi:
                                agi++;
                                break;

                            case StatusType.Vit:
                                vit++;
                                break;

                            case StatusType.Int:
                                @int++;
                                break;

                            case StatusType.Dex:
                                dex++;
                                break;

                            case StatusType.Luk:
                                luk++;
                                break;
                        }

                        tuples.Clear();
                    }

                    dataDic[cupetType][i] = new CupetAutoStatusData(str, agi, vit, @int, dex, luk);
                }
            }
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

#endif

        }

        bool IEqualityComparer<CupetType>.Equals(CupetType x, CupetType y)
        {
            return x == y;
        }

        int IEqualityComparer<CupetType>.GetHashCode(CupetType obj)
        {
            return obj.GetHashCode();
        }
    }
}