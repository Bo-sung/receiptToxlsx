using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICupet"/>
    /// </summary>
    public class CupetPresenter : ViewPresenter
    {
        private readonly GoodsModel goodsModel;
        private readonly CupetListModel cupetListModel;
        private readonly QuestModel questModel;
        private readonly GuildModel guildModel;

        private readonly Buffer<CupetElement> cupetElements;

        /// <summary>
        /// 길드장 여부
        /// </summary>
        public bool IsGuildMaster => guildModel.GuildPosition == GuildPosition.Master;

        /// <summary>
        /// 부길드장 여부
        /// </summary>
        public bool IsGuildPartMaster => guildModel.GuildPosition == GuildPosition.PartMaster;

        public event System.Action OnUpdateCupetList
        {
            add { cupetListModel.OnUpdateCupetList += value; }
            remove { cupetListModel.OnUpdateCupetList -= value; }
        }

        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public CupetPresenter()
        {
            goodsModel = Entity.player.Goods;
            cupetListModel = Entity.player.CupetList;
            questModel = Entity.player.Quest;
            guildModel = Entity.player.Guild;
            cupetElements = new Buffer<CupetElement>();
        }

        public override void AddEvent()
        {
            cupetListModel.OnSummon += InvokeGuildCupetList; // 큐펫 소환
            cupetListModel.OnEvolution += InvokeGuildCupetList; // 큐펫 랭크업
        }

        public override void RemoveEvent()
        {
            cupetListModel.OnSummon -= InvokeGuildCupetList; // 큐펫 소환
            cupetListModel.OnEvolution -= InvokeGuildCupetList; // 큐펫 랭크업
        }

        /// <summary>
        /// 모든 큐펫 목록을 반환 (미보유 포함)
        /// </summary>
        public CupetEntity[] GetArray()
        {
            return cupetListModel.GetArray();
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Cupet()
        {
            questModel.RemoveNewOpenContent(ContentType.Cupet); // 신규 컨텐츠 플래그 제거 (큐펫)
        }

        /// <summary>
        /// 보유 중인 큐펫 수 반환
        /// </summary>
        public int GetHaveCupetCount()
        {
            CupetEntity[] cupetList = cupetListModel.GetArray();
            int haveCount = 0;
            foreach (CupetEntity cupet in cupetList)
            {
                if (cupet.Cupet.IsInPossession)
                {
                    ++haveCount;
                }
            }
            return haveCount;
        }

        /// <summary>
        /// 모든 큐펫 수 반환
        /// </summary>
        public int GetAllCupetCount()
        {
            return cupetListModel.GetArray().Length;
        }

        /// <summary>
        /// 보유 큐펫의 랭크 총합을 반환
        /// </summary>
        public int GetHaveCupetRankSum()
        {
            return cupetListModel.GetHaveCupetRankSum();
        }

        void InvokeGuildCupetList()
        {
            RequestCupetList();
        }

        public void RequestCupetList()
        {
            cupetListModel.RequestGuildCupetInfo().WrapNetworkErrors();
        }

        public UICupetElement.IInput[] GetCupetArray(CupetViewSortType sortType)
        {
            var cupetArray = cupetListModel.GetArray();
            System.Array.Sort(cupetArray, sortType.GetSortFunc());

            foreach (CupetEntity item in cupetArray)
            {
                CupetElement temp = new CupetElement(item.Cupet, CheckNotice);
                cupetElements.Add(temp);
            }

            return cupetElements.GetBuffer(isAutoRelease: true);
        }

        public void OnSelectCupet(int cupetId)
        {
            UI.Show<UICupetInfo>(cupetListModel.Get(cupetId));
        }

        private bool CheckNotice(int cupetId)
        {
            // 일반 길드원은 큐펫 소환 불가
            if (guildModel.GuildPosition == GuildPosition.Member || guildModel.GuildPosition == GuildPosition.None)
                return false;

            CupetModel cupetModel = cupetListModel.Get(cupetId).Cupet;

            // 미보유 (소환 체크)
            if (!cupetModel.IsInPossession)
            {
                // 소환에 필요한 조각 부족
                if (cupetModel.Count < cupetModel.GetNeedSummonPieceCount())
                    return false;
            }
            else // 보유 (진화 체크)
            {
                // 최대 랭크 도달
                if (cupetModel.IsMaxRank())
                    return false;

                // 진화에 필요한 조각 부족
                if (cupetModel.Count < cupetModel.GetNeedEvolutionPieceCount())
                    return false;

                // 제니 부족
                if (!CoinType.Zeny.Check(cupetModel.GetNeedEvolutionPrice(), isShowPopup: false))
                    return false;
            }

            return true;
        }

        private class CupetElement : UICupetElement.IInput
        {
            public CupetModel CupetModel { get; }
            public bool IsNotice => checkNotice(CupetModel.CupetID);

            private readonly System.Func<int, bool> checkNotice;

            public CupetElement(CupetModel cupetModel, System.Func<int, bool> checkNotice)
            {
                CupetModel = cupetModel;
                this.checkNotice = checkNotice;
            }
        }
    }
}