using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// 우편함 정보
    /// </summary>
    public sealed class MailModel : CharacterEntityModel
    {
        private readonly Dictionary<MailType, List<MailInfo>> mailInfos;
        private readonly Dictionary<MailType, int?> nextPage;
        private const int PAGE_COUNT = 10; // 페이지당 우편 목록수
        private readonly ShopDataManager shopDataRepo;

        public MailModel()
        {
            mailInfos = new Dictionary<MailType, List<MailInfo>>();
            nextPage = new Dictionary<MailType, int?>();
            shopDataRepo = ShopDataManager.Instance;
            foreach (MailType item in Enum.GetValues(typeof(MailType)))
            {
                mailInfos.Add(item, new List<MailInfo>());
                nextPage.Add(item, null);
            }
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        /// <summary>
        /// 다음 페이지 메일 있는지 여부
        /// </summary>
        /// <param name="mailType"></param>
        /// <returns></returns>
        public bool HasNextPage(MailType mailType)
        {
            return nextPage[mailType].HasValue;
        }

        /// <summary>
        /// 메일 목록 반환
        /// </summary>
        /// <param name="mailType"></param>
        /// <returns></returns>
        public MailInfo[] GetMailInfos(MailType mailType)
        {
            return mailInfos[mailType].ToArray();
        }

        /// <summary>
        /// 전체 제거
        /// </summary>
        public void ClearAll()
        {
            foreach (MailType type in Enum.GetValues(typeof(MailType)))
            {
                mailInfos[type].Clear();
                nextPage[type] = null;
            }
        }

        /// <summary>
        /// 메일 리스트 삭제
        /// </summary>
        public void Clear(MailType type, bool exceptAd = false)
        {
            if (exceptAd)
            {
                mailInfos[type].RemoveAll(x => x.MailGroup == MailGroup.Normal);
            }
            else
            {
                mailInfos[type].Clear();
            }

            nextPage[type] = null;
        }

        public async Task RequestNextMailList(MailType mailType)
        {
            // 다음페이지가 없는 경우
            if (!nextPage[mailType].HasValue)
                return;

            await RequestMailList(nextPage[mailType].Value, mailType);
        }

        public async Task RequestMailList(int page, MailType mailType)
        {
            if (mailType == MailType.Trade)
            {
                await RequestMailList_Trade(page, mailType);
                return;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", page);
            sfs.PutByte("2", mailType.ToByteValue());

            var response = await Protocol.MAIL_LIST.SendAsync(sfs);
            if (response.isSuccess)
            {
                if (response.ContainsKey("1"))
                {
                    // 메일 리스트
                    MailPacket[] mailArray = response.GetPacketArray<MailPacket>("1");
                    foreach (var item in mailArray)
                    {
                        mailInfos[mailType].Add(new MailInfo(item));
                    }
                }

                // 다음 페이지
                if (response.ContainsKey("2"))
                {
                    nextPage[mailType] = response.GetInt("2");
                }
                else
                {
                    nextPage[mailType] = null;
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 거래소탭 전용 메일 조회 요청 프로토콜
        /// </summary>
        /// <param name="page">0-based</param>
        private async Task RequestMailList_Trade(int page, MailType mailType)
        {
            var param = Protocol.NewInstance();
            param.PutInt("1", page);

            var response = await Protocol.REQUEST_TRADEDONE_LIST.SendAsync(param);
            if (response.isSuccess)
            {
                long resCount = response.GetInt("1");
                for (long i = 0; i < resCount; ++i)
                {
                    var data = response.GetSFSObject((2 + i).ToString());

                    long resSeq_no = data.GetLong("1");
                    long resMail_uid = data.GetLong("2");
                    int resCid = data.GetInt("3");
                    int resItemID = data.GetInt("4");
                    int resTier_per = data.GetInt("5");
                    byte resItem_level = data.GetByte("6");
                    int resItem_count = data.GetInt("7");
                    long resCard_id1 = data.GetLong("8");
                    long resCard_id2 = data.GetLong("9");
                    long resCard_id3 = data.GetLong("10");
                    long resCard_id4 = data.GetLong("11");
                    long resInsertTime = data.GetLong("12");
                    byte resSellType = data.GetByte("13");
                    int resSellValue = data.GetInt("14");
                    byte resIsGetItem = data.GetByte("15");
                    int resItemTranscend = data.GetInt("16");
                    int resElementChange = data.GetInt("17");
                    int resElementLevel = data.GetInt("18");

                    MailInfo newMail = new MailInfo(resSeq_no, resMail_uid, resCid, resItemID, resTier_per, resItem_level, resItem_count,
                        resCard_id1, resCard_id2, resCard_id3, resCard_id4, resInsertTime, resSellType, resSellValue, resIsGetItem, resItemTranscend, resElementChange, resElementLevel);

                    mailInfos[mailType].Add(newMail);
                }

                nextPage[mailType] = null;
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 메일 받기
        /// </summary>
        public async Task RequestReceiveMail(MailInfo info, MailType mailType)
        {
            // 가방 무게 체크
            if (info.IsWeight && !UI.CheckInvenWeight())
                return;

            Response response;
            if (mailType == MailType.Trade)
            {
                response = await RequestGetTradeReward(info.seq_no);
            }
            else if (mailType == MailType.Shop)
            {
                if (GameServerConfig.IsKoreaLanguage())
                {
                    // 우편에서 아이템 수령 시 청약철회 및 환불이 불가능합니다.\n수령에 동의하십니까?
                    if (!await UI.SelectPopup(LocalizeKey._90308.ToText()))
                        return;
                }

                var sfs = Protocol.NewInstance();
                sfs.PutInt("1", info.id);
                response = await Protocol.REQUEST_SHOP_MAIL_GET.SendAsync(sfs);
            }
            else if (mailType == MailType.OnBuff)
            {
                response = await RequestGetOnBuffMail(info.id);
            }
            else
            {
                var sfs = Protocol.NewInstance();
                sfs.PutInt("1", info.id);
                sfs.PutByte("2", mailType.ToByteValue());

                response = await Protocol.MAIL_GET.SendAsync(sfs);
            }

            if (response.isSuccess)
            {
                // 특수 버프 아이템 보상 표시 처리
                if (mailType == MailType.Shop)
                {
                    CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null; // cud. 캐릭터 업데이트 데이터
                    Notify(charUpdateData);

                    List<RewardPacket> rewards = new List<RewardPacket>();

                    if (charUpdateData.rewards != null && charUpdateData.rewards.Length > 0)
                        rewards.AddRange(charUpdateData.rewards);

                    int shopId = info.GetShopId();

                    // 나무 보상 증가 남은시간
                    if (response.ContainsKey("1"))
                    {
                        NotifyUpdateTreeRemainTime(response.GetLong("1"));
                    }

                    NotifyReceiveShopMail(shopId);

                    ShopData data = shopDataRepo.Get(shopId);

                    if (data != null)
                    {
                        PackageType packageType = data.package_type.ToEnum<PackageType>();
                        switch (packageType)
                        {
                            // 셰어 보상 패키지 상품 우편에서 받을때 셰어 정산 20% 추가 보상 추가 표시
                            case PackageType.SharePackage:
                                {
                                    RewardPacket reward = new RewardPacket(RewardType.ShareReward.ToByteValue(), 1, 0, 0);
                                    rewards.Add(reward);
                                    break;
                                }
                            // 첫 결제 보상 받을때 가방 무게 증가 +20 증가 추가로 표시
                            case PackageType.FirstPaymentReward:
                                {
                                    RewardPacket reward = new RewardPacket(RewardType.InvenWeight.ToByteValue(), 1, 0, 0);
                                    rewards.Add(reward);
                                    NotifyInvenWeight();
                                    break;
                                }
                            case PackageType.TreePackage:
                                {
                                    RewardPacket reward = new RewardPacket(RewardType.TreeReward.ToByteValue(), 1, 0, 0);
                                    rewards.Add(reward);
                                    break;
                                }
                            case PackageType.BattlePassPackage:
                                {
                                    RewardPacket reward = new RewardPacket(RewardType.BattlePass.ToByteValue(), 1, 0, 0);
                                    rewards.Add(reward);
                                    break;
                                }
                            case PackageType.OnBuffPassPackage:
                                {
                                    RewardPacket reward = new RewardPacket(RewardType.OnBuffPass.ToByteValue(), 1, 0, 0);
                                    rewards.Add(reward);
                                    break;
                                }
                        }
                    }

                    UI.RewardInfo(rewards.ToArray()); // 획득 보상 보여줌 (UI)
                }
                else
                {
                    // cud. 캐릭터 업데이트 데이터 (우편 획득 처리)
                    if (response.ContainsKey("cud"))
                    {
                        CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                        Notify(charUpdateData);
                        UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                    }
                }

                if (mailType == MailType.Trade)
                {
                    info.isGetItem = true;
                    UI.ShowToastPopup(LocalizeKey._4414.ToText()); // 수령 완료
                }
                else
                {
                    mailInfos[mailType].Remove(info);
                }

                if (mailInfos[mailType].Count < PAGE_COUNT && nextPage[mailType].HasValue)
                    await RequestNextMailList(mailType);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 메일 모두 받기
        /// </summary>
        public async Task RequestReceiveAllMail(MailType mailType)
        {
            // 우편이 없을 경우
            if (mailInfos[mailType].Count == 0)
                return;

            int weight = 0;
            foreach (var item in mailInfos[mailType])
            {
                weight += item.GetTotalWeight();
            }

            // TODO 인벤 무게 체크
            // TODO 다음페이지가 있을경우 무게 체크에 포함 안될 수 있음
            // TODO 전체 받기의 경우 서버에서 체크해줘야 함
            if (weight > 0 && !UI.CheckInvenWeight())
                return;

            Response response;
            if (mailType == MailType.Trade)
            {
                response = await RequestGetTradeRewardAll();
            }
            else
            {
                var sfs = Protocol.NewInstance();
                sfs.PutByte("1", mailType.ToByteValue());
                response = await Protocol.MAIL_GET_ALL.SendAsync(sfs);
            }

            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터 (우편 획득 처리)
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                }

                if (mailType == MailType.Trade)
                {
                    foreach (var e in mailInfos[mailType])
                        e.isGetItem = true;

                    UI.ShowToastPopup(LocalizeKey._4414.ToText()); // 수령 완료
                }
                else
                {
                    // 모두 받기 시 광고메일은 예외
                    Clear(mailType, true);
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 메일 삭제
        /// </summary>
        public async Task RequestDeleteMail(MailInfo info, MailType mailType)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.id);
            sfs.PutByte("2", (byte)mailType);
            Response response = await Protocol.REQUEST_MAIL_DELETE.SendAsync(sfs);

            if (response.isSuccess)
            {
                mailInfos[mailType].Remove(info);

                if (mailInfos[mailType].Count < PAGE_COUNT && nextPage[mailType].HasValue)
                    await RequestNextMailList(mailType);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 거래소 거래내역 데이터로부터 아이템/골드 수령(단일)
        /// </summary>
        /// <param name="seq_no">거래기록 번호(seq_no)</param>
        private async Task<Response> RequestGetTradeReward(long seq_no)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", seq_no);
            return await Protocol.REQUEST_TRADE_GET_SELLITEM.SendAsync(sfs);
        }

        /// <summary>
        /// 거래소 거래내역 데이터로부터 아이템/골드 수령(모두)
        /// </summary>
        public async Task<Response> RequestGetTradeRewardAll()
        {
            return await Protocol.REQUEST_TRADE_GET_SELLITEMALL.SendAsync();
        }

        /// <summary>
        /// 온버프 메일 수령
        /// </summary>
        private async Task<Response> RequestGetOnBuffMail(int id)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id);
            return await Protocol.REQUEST_COIN_MAIL_GET.SendAsync(sfs);
        }
    }
}