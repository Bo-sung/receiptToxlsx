using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterShareWaiting"/>
    /// </summary>
    public sealed class CharacterShareWaitingPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly SharingModel sharingModel;
        private readonly InventoryModel inventoryModel;
        private readonly ShopModel shopModel;
        private readonly BattleBuffItemInfo battleBuffItemInfo;

        // <!-- Compositions --!>
        private readonly System.TimeSpan maxExposeTime; // 최대 노출 시간

        private readonly BattleManager battleManager;

        public event System.Action OnUpdateSharingState
        {
            add { sharingModel.OnUpdateSharingState += value; }
            remove { sharingModel.OnUpdateSharingState -= value; }
        }

        public event System.Action OnUpdateSharingRewards
        {
            add { sharingModel.OnUpdateSharingRewards += value; }
            remove { sharingModel.OnUpdateSharingRewards -= value; }
        }

        public event System.Action OnUpdateEmployer
        {
            add { sharingModel.OnUpdateEmployer += value; }
            remove { sharingModel.OnUpdateEmployer -= value; }
        }

        public CharacterShareWaitingPresenter()
        {
            sharingModel = Entity.player.Sharing;
            inventoryModel = Entity.player.Inventory;
            shopModel = Entity.player.ShopModel;
            battleBuffItemInfo = Entity.player.battleBuffItemInfo;
            battleManager = BattleManager.Instance;

            maxExposeTime = System.TimeSpan.FromMilliseconds(BasisType.CHAR_SHARE_TIME.GetInt());
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 내 캐릭터 셰어상태 반환
        /// </summary>
        public SharingModel.SharingState GetSharingState()
        {
            return sharingModel.GetSharingState();
        }

        /// <summary>
        /// 내 캐릭터 셰어보상 반환
        /// </summary>
        public SharingRewardPacket GetSharingRewardData()
        {
            return sharingModel.GetSharingRewardPacket();
        }

        /// <summary>
        /// 고용주 정보 반환
        /// </summary>
        public SharingEmployerPacket GetSharingEmployerData()
        {
            return sharingModel.GetEmployerInfo();
        }

        /// <summary>
        /// 본인의 캐릭터 셰어 캐릭터 철회
        /// </summary>
        public void RequestShareRegisterEnd()
        {
            sharingModel.RequestShareCharacterSetting(isShare: false).WrapNetworkErrors();
        }

        /// <summary>
        /// 보상 정보 확인
        /// </summary>
        public void RequestShareCharacterRewardInfo()
        {
            sharingModel.RequestShareCharacterRewardInfo().WrapNetworkErrors();
        }

        /// <summary>
        /// 최대 셰어링 노출 시간
        /// </summary>
        public long GetMaxExposeTimeTicks()
        {
            return maxExposeTime.Ticks;
        }

        /// <summary>
        /// 현재 무게
        /// </summary>
        public int GetCurrentInvenWeight()
        {
            return inventoryModel.CurrentInvenWeight;
        }

        /// <summary>
        /// 최대 무게
        /// </summary>
        public int GetMaxInvenWeight()
        {
            return inventoryModel.MaxInvenWeight;
        }

        /// <summary>
        /// 현재 전투 모드
        /// </summary>
        public BattleMode GetCurrentBattleMode()
        {
            return battleManager.Mode;
        }

        /// <summary>
        /// 영구 셰어 버프 적용여부
        /// </summary>
        /// <returns></returns>
        public bool IsShareBuff()
        {
            return shopModel.IsShareBuff;
        }

        /// <summary>
        /// 영구 셰어 버프 증가량
        /// </summary>
        /// <returns></returns>
        public string GetShareBuffValueText()
        {
            int value = BasisType.SHARE_UP_POINT_RATE.GetInt();
            return MathUtils.ToPermyriadText(value);
        }

        /// <summary>
        /// 적용중인 버프중 카프라 버프 정보 반환
        /// </summary>
        /// <returns></returns>
        public BuffItemInfo GetKafraBuffItem()
        {
            var buffItems = battleBuffItemInfo.buffItemList;

            for (int i = 0; i < buffItems.Count; i++)
            {
                if (buffItems[i].HasBattleOptionType(BattleOptionType.ShareReward))
                {
                    return buffItems[i];
                }
            }

            return null;            
        }

        /// <summary>
        /// 카프라 버프 증가량
        /// </summary>
        /// <returns></returns>
        public string GetKafraBuffValueText()
        {
            int value = BasisType.KAFRA_POINT_RATE.GetInt();
            return MathUtils.ToPermyriadText(value);
        }
    }
}