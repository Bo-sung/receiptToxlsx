using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIConsumableInfo"/>
    /// </summary>
    public sealed class ConsumableInfoPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void CloseUI();
        }

        private readonly IView view;
        private readonly InventoryModel inventoryModel;
        private readonly CharacterModel characterModel;
        private readonly DuelModel duelModel;
        private readonly ItemDataManager itemDataRepo;

        private readonly int jobChangeTicketItemId;
        private readonly int transcendenceDisasembleItemId;

        public ItemInfo info { get; private set; }

        public ConsumableInfoPresenter(IView view)
        {
            this.view = view;
            inventoryModel = Entity.player.Inventory;
            characterModel = Entity.player.Character;
            duelModel = Entity.player.Duel;

            itemDataRepo = ItemDataManager.Instance;

            jobChangeTicketItemId = BasisItem.JobChangeTicket.GetID();
            transcendenceDisasembleItemId = BasisItem.TranscendenceDisasemble.GetID();
        }

        public override void AddEvent()
        {

        }

        public override void RemoveEvent()
        {
            RemoveInfoEvent();
        }

        public void SelectInfo(ItemInfo info)
        {
            RemoveInfoEvent();

            this.info = info;
            inventoryModel.ShowBoxItemRewardLog(info);

            AddInfoEvent();
            view.Refresh();
        }

        private void AddInfoEvent()
        {
            if (info != null)
                info.OnUpdateEvent += view.Refresh;
        }

        private void RemoveInfoEvent()
        {
            if (info != null)
            {
                info.OnUpdateEvent -= view.Refresh;
                info = null;
            }
        }

        /// <summary>
        /// 소모품 사용
        /// </summary>
        public async Task RequestUseConsumableItem(int use_count = 1)
        {
            // 노비스는 박스아이템중 직업 참조 아이템을 열수 없다.
            if (characterModel.Job == Job.Novice && info.ItemType == ItemType.Box && info.BoxType == BoxType.JobRef)
            {
                UI.ConfirmPopup(LocalizeKey._90225.ToText()); // 전직 후 사용할 수 있습니다.
                view.CloseUI();
                return;
            }

            if (info.ItemId == jobChangeTicketItemId)
            {
                if (characterModel.Job == Job.Novice)
                {
                    UI.ConfirmPopup(LocalizeKey._22007.ToText()); // 1차 전직 후 사용할 수 있습니다.
                    return;
                }

                UI.Show<UIJobReplace>();
                //view.CloseUI();
                return;
            }

            if (characterModel.JobGrade() < 2 && info.ItemType == ItemType.Box && info.BoxType == BoxType.JobGradeRef)
            {
                // 2차 전직 이하 일때 오픈할수 없는 상자타입
                int needGrade = 2;

                string description = LocalizeKey._90170.ToText() // {VALUE}차 전직 후 이용 가능합니다.
                        .Replace(ReplaceKey.VALUE, needGrade)
                        .Replace(ReplaceKey.LEVEL, BasisType.JOB_MAX_LEVEL.GetInt(needGrade));

                UI.ConfirmPopup(description);
                view.CloseUI();
                return;
            }

            // [확성기] 확성기 아이템인 경우, 메시지 입력 받기
            if (info.ConsumableItemType == ConsumableItemType.LoudSpeaker)
            {
                UI.Show<UILoudSpeakerPopup>().Set(info.ItemNo);
                view.CloseUI();
                return;
            }

            // [성별변경]
            if (info.ConsumableItemType == ConsumableItemType.TranssexualPotion)
            {
                string title = LocalizeKey._90236.ToText(); // 성별 변경
                string message = characterModel.Gender == Gender.Male ? LocalizeKey._90237.ToText() : LocalizeKey._90238.ToText();
                if (!await UI.SelectPopup(title, message))
                {
                    view.CloseUI();
                    return;
                }
            }

            // [듀얼 티켓 충전]
            if (info.ConsumableItemType == ConsumableItemType.DuelTicket)
            {
                if (duelModel.DuelPoint + BasisType.DUEL_POINT_CHARGE.GetInt() > BasisType.DUEL_POINT_TOTAL_MAX.GetInt())
                {
                    UI.ShowToastPopup(LocalizeKey._47821.ToText()); // 듀얼 포인트가 최대치입니다.
                    return;
                }

                string title = LocalizeKey._47822.ToText(); // 듀얼 포인트 충전
                string message = LocalizeKey._47823.ToText(); // 듀얼 포인트를 충전하시겠습니까?
                if (!await UI.SelectPopup(title, message))
                {
                    view.CloseUI();
                    return;
                }
            }

            // 초월 분해
            if (info.ItemId == transcendenceDisasembleItemId)
            {
                UI.Show<UITranscendenceDisassemble>();
                view.CloseUI();
                return;
            }
            var response = await inventoryModel.RequestUseConsumableItem(info, equipmentId: 0, useBoxCount: use_count);
            if (info.ItemType == ItemType.Box)
            {
                if (response != null && response.isSuccess && response.ContainsKey("cud"))
                {
                    RewardPacket[] rewards = response.GetPacket<CharUpdateData>("cud").rewards;

                    if (rewards == null)
                    {
                        Debug.LogError("CUD 99번이 없다.");
                        return;
                    }

                    if (rewards.Length != 0)
                    {
                        // 카드 보상 찾기
                        List<CardItemInfo.ICardInfoSimple> cardInfoList = new List<CardItemInfo.ICardInfoSimple>();
                        foreach (var reward in rewards)
                        {
                            CardItemInfo param = GetCardItemFromRewardPacket(reward);
                            cardInfoList.Add(param);
                        }

                        bool isShowCardRewardUI = cardInfoList.All(e => e != null);
                        if (isShowCardRewardUI) // 보상이 전부 카드인 경우 카드연출
                        {
                            UICardReward uiCardReward = UI.Show<UICardReward>();
                            uiCardReward.Show(cardInfoList.ToArray(), true, true);
                        }
                        else // 박스 연출
                        {
                            UI.Show<UIBoxOpen>(new UIBoxOpen.Input(rewards, info.IconName));
                        }
                    }
                }
            }
            view.CloseUI();
        }
        private CardItemInfo GetCardItemFromRewardPacket(RewardPacket packet)
        {
            RewardType type = packet.rewardType.ToEnum<RewardType>();
            if (type == RewardType.Item)
            {
                ItemData itemData = itemDataRepo.Get(packet.rewardValue);
                if (itemData.ItemGroupType == ItemGroupType.Card)
                {
                    CardItemInfo cardItemInfo = new CardItemInfo();
                    cardItemInfo.SetData(itemData);

                    return cardItemInfo;
                }
            }

            return null;
        }

        public int GetItemCount()
        {
            return info == null ? 0 : inventoryModel.GetItemCount(info.ItemId);
        }
    }
}