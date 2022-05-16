using Ragnarok.View.CharacterShare;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterShare"/>
    /// </summary>
    public sealed class CharacterSharePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly SharingModel sharingModel;
        private readonly ShopModel shopModel;
        private readonly InventoryModel inventoryModel;
        private readonly CharacterModel characterModel;
        private readonly CharacterListModel characterListModel;
        private readonly UserModel userModel;
        private readonly DungeonModel dungeonModel;
        private readonly QuestModel questModel;
        private readonly GuildModel guildModel;

        // <!-- Repositories --!>
        private readonly StageDataManager stageDataRepo;
        private readonly ScenarioMazeDataManager mazeRepo;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Compositions --!>
        private readonly System.TimeSpan freeDailyShareTimeSpan; // 무료 충전 시간
        private readonly System.TimeSpan maxShareTimeSpan; // 최대 충전 시간

        private bool isSendShareCharacterList;

        public event System.Action OnUpdateSharingState
        {
            add { sharingModel.OnUpdateSharingState += value; }
            remove { sharingModel.OnUpdateSharingState -= value; }
        }

        public event System.Action OnUpdateRemainTimeForShare
        {
            add { sharingModel.OnUpdateRemainTimeForShare += value; }
            remove { sharingModel.OnUpdateRemainTimeForShare -= value; }
        }

        public event System.Action OnUpdateSharingCharacters
        {
            add { sharingModel.OnUpdateSharingCharacters += value; }
            remove { sharingModel.OnUpdateSharingCharacters -= value; }
        }

        public event System.Action OnUpdateShareFreeTicket
        {
            add { sharingModel.OnUpdateShareFreeTicket += value; }
            remove { sharingModel.OnUpdateShareFreeTicket -= value; }
        }

        public event System.Action OnUpdateShareTicketCount
        {
            add { sharingModel.OnUpdateShareTicketCount += value; }
            remove { sharingModel.OnUpdateShareTicketCount -= value; }
        }

        public event System.Action OnUpdateShareCharacterList
        {
            add { sharingModel.OnUpdateShareCharacterList += value; }
            remove { sharingModel.OnUpdateShareCharacterList -= value; }
        }

        public event System.Action OnUpdateJob;

        public event System.Action OnUpdateGender
        {
            add { characterModel.OnChangedGender += value; }
            remove { characterModel.OnChangedGender -= value; }
        }

        public event InventoryModel.InvenWeightEvent OnUpdateInvenWeight
        {
            add { inventoryModel.OnUpdateInvenWeight += value; }
            remove { inventoryModel.OnUpdateInvenWeight -= value; }
        }

        public event System.Action OnUpdateShareExitAutoSetting
        {
            add { sharingModel.OnUpdateShareExitAutoSetting += value; }
            remove { sharingModel.OnUpdateShareExitAutoSetting -= value; }
        }

        public event System.Action OnUpdateFilterCount
        {
            // 셰어 필터 UI가 닫힐 때, 필터 갯수를 갱신해 줌.
            add { sharingModel.OnHideShareFilterUI += value; }
            remove { sharingModel.OnHideShareFilterUI -= value; }
        }

        public event System.Action OnLevelUpSharevice
        {
            add { sharingModel.OnUpdateShareviceExperience += value; }
            remove { sharingModel.OnUpdateShareviceExperience -= value; }
        }

        public event System.Action OnShareForceLevelUp
        {
            add { characterModel.OnShareForceLevelUp += value; }
            remove { characterModel.OnShareForceLevelUp -= value; }
        }

        public event System.Action OnUpdateShareForceStatus
        {
            add { characterModel.OnUpdateShareForceStatus += value; }
            remove { characterModel.OnUpdateShareForceStatus -= value; }
        }

        public event System.Action OnUpdateGuildInfo;

        public CharacterSharePresenter()
        {
            sharingModel = Entity.player.Sharing;
            shopModel = Entity.player.ShopModel;
            inventoryModel = Entity.player.Inventory;
            characterModel = Entity.player.Character;
            characterListModel = Entity.player.CharacterList;
            userModel = Entity.player.User;
            dungeonModel = Entity.player.Dungeon;
            questModel = Entity.player.Quest;
            guildModel = Entity.player.Guild;
            mazeRepo = ScenarioMazeDataManager.Instance;
            stageDataRepo = StageDataManager.Instance;
            battleManager = BattleManager.Instance;

            freeDailyShareTimeSpan = System.TimeSpan.FromMilliseconds(BasisType.FREE_USE_CHAR_SHARE_TIME.GetInt());
            maxShareTimeSpan = System.TimeSpan.FromMilliseconds(BasisType.MAX_USE_CHAR_SHARE_TIME.GetInt());

            ResetData();
        }

        public override void AddEvent()
        {
            characterModel.OnChangedJob += InvokeChangedJob;
            guildModel.OnUpdateGuildInfo += InvokeGuildInfo;
        }

        public override void RemoveEvent()
        {
            characterModel.OnChangedJob -= InvokeChangedJob;
            guildModel.OnUpdateGuildInfo -= InvokeGuildInfo;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void ResetData()
        {
            sharingModel.ResetCharacterList();
        }

        /// <summary>
        /// 내 캐릭터 셰어상태 반환
        /// </summary>
        public SharingModel.SharingState GetSharingState()
        {
            return sharingModel.GetSharingState();
        }

        /// <summary>
        /// 현재 사용중인 셰어링 캐릭터 반환
        /// </summary>
        public UISimpleCharacterShareBar.IInput[] GetSharingCharacters()
        {
            return sharingModel.GetSharingCharacters();
        }

        /// <summary>
        /// 현재 사용중인 클론 캐릭터 반환
        /// </summary>
        public UISimpleCharacterShareBar.IInput[] GetCloneCharacters()
        {
            return sharingModel.GetCloneCharacters();
        }

        /// <summary>
        /// 공유 가능 남은시간
        /// </summary>
        public float GetRemainTimeForShare()
        {
            return sharingModel.GetRemainTimeForShare();
        }

        /// <summary>
        /// 공유 가능 시간 종료
        /// </summary>
        public bool IsFinishedSharingTime()
        {
            return sharingModel.IsFinishedSharingTime();
        }

        /// <summary>
        /// 무료 티켓 존재 여부
        /// </summary>
        public bool HasShareFreeTicket()
        {
            return GetShareTicketCount(ShareTicketType.DailyFree) > 0;
        }

        /// <summary>
        /// 무료 충전 시간
        /// </summary>
        public int GetFreeDailyShareTimeTotalHours()
        {
            return (int)freeDailyShareTimeSpan.TotalHours;
        }

        /// <summary>
        /// 최대 충전 시간
        /// </summary>
        public int GetMaxShareTimeTotalHours()
        {
            return (int)maxShareTimeSpan.TotalHours;
        }

        /// <summary>
        /// 티켓 초기화
        /// </summary>
        public ShopInfo GetShopInfo(ShareTicketType ticketType)
        {
            return shopModel.GetInfo(ticketType);
        }

        public BuffItemInfo[] GetShareviceBuffInfo()
        {
            var buffItems = Entity.player.battleBuffItemInfo.buffItemList;
            List<BuffItemInfo> buffList = new List<BuffItemInfo>();

            for (int i = 0; i < buffItems.Count; i++)
            {
                if (buffItems[i].HasBattleOptionType(BattleOptionType.ShareBoost))
                {
                    buffList.Add(buffItems[i]);
                }
            }

            return buffList.ToArray();
        }

        /// <summary>
        /// 티켓 수
        /// </summary>
        public int GetShareTicketCount(ShareTicketType ticketType)
        {
            return sharingModel.GetShareTicketCount(ticketType);
        }

        /// <summary>
        /// 셰어 캐릭터 목록 반환
        /// </summary>
        public UISimpleCharacterShareBar.IInput[] GetShareCharacterList()
        {
            return sharingModel.GetShareCharacterList();
        }

        public UISimpleCharacterShareBar.IInput[] GetCloneCharacterList()
        {
            return sharingModel.GetCloneCharacterList(characterListModel.GetCharacters(), userModel.UID, characterModel.Cid);
        }

        /// <summary>
        /// 플레이어 일러스트
        /// </summary>
        public string GetJobIllustTextureName()
        {
            return characterModel.Job.GetJobSDName(characterModel.Gender);
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

        public int GetShareviceLevel()
        {
            return sharingModel.GetShareviceLevel();
        }

        public int GetShareviceMaxBP(int addLevel)
        {
            return sharingModel.GetShareviceMaxBP(addLevel);
        }

        /// <summary>
        /// 종료 시 셰어 자동 등록 설정
        /// </summary>
        public bool IsShareExitAutoSetting()
        {
            return sharingModel.IsShareExitAutoSetting();
        }

        /// <summary>
        /// 오토 셰어링 가능 여부
        /// </summary>
        public bool IsCheckAutoSetting(bool isShowMessage)
        {
            return characterModel.IsCheckJobGrade(Constants.OpenCondition.NEED_SHARE_REGISTER_JOB_GRADE, isShowMessage);
        }

        public int GetSharingFilterCount()
        {
            return sharingModel.GetShareJobFilterAry().Count(f => f > 0);
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Sharing()
        {
            questModel.RemoveNewOpenContent(ContentType.Sharing); // 신규 컨텐츠 플래그 제거 (셰어)
        }

        /// <summary>
        /// 셰어 캐릭터 목록 요청
        /// </summary>
        public void RequestShareCharacterList()
        {
            int lastEnterStageId = Mathf.Max(1, dungeonModel.LastEnterStageId);
            StageData stageData = stageDataRepo.Get(lastEnterStageId);
            int chapter = stageData.chapter; // 챕터
            sharingModel.RequestShareCharacterList(stageData.id, chapter).WrapNetworkErrors();
        }

        /// <summary>
        /// 길드 정보 요청
        /// </summary>
        public void RequestGuildInfo()
        {
            if (!guildModel.HaveGuild)
            {
                InvokeGuildInfo();
                return;
            }

            guildModel.RequestMyGuildInfo(isForce: true).WrapNetworkErrors();
        }

        private void InvokeGuildInfo()
        {
            OnUpdateGuildInfo?.Invoke();
        }

        /// <summary>
        /// 길드 쉐어 목록 요청
        /// </summary>
        public void RequestGuildShareCharacterList()
        {
            // 길드 정보 받아온 후에 길드 쉐어목록 요청
            sharingModel.RequestGuildShareCharacterList().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드 보유 여부
        /// </summary>
        public bool HaveGuild()
        {
            return guildModel.HaveGuild;
        }

        /// <summary>
        /// 길드 기여도 점수
        /// </summary>
        public int GetGuildDonationPoint()
        {
            return guildModel.GuildDonationPoint;
        }

        /// <summary>
        /// 본인의 캐릭터 셰어 캐릭터에 등록
        /// </summary>
        public void RequestShareRegisterStart()
        {
            if (!IsCheckAutoSetting(isShowMessage: true))
                return;

            sharingModel.RequestShareCharacterSetting(isShare: true).WrapNetworkErrors();
        }

        /// <summary>
        /// 종료 시 셰어 자동 등록 설정
        /// </summary>
        public void RequestShareExitAutoSetting()
        {
            sharingModel.RequestShareExitAutoSetting().WrapNetworkErrors();
        }

        /// <summary>
        /// 셰어 캐릭터 사용
        /// </summary>
        public void RequestShareCharacterUse(int cid, int uid, SharingModel.SharingCharacterType sharingCharacterType)
        {
            sharingModel.RequestShareCharacterUseSetting(SharingModel.CharacterShareFlag.Use, cid, uid, sharingCharacterType).WrapNetworkErrors();
        }

        /// <summary>
        /// 셰어 캐릭터 사용취소
        /// </summary>
        public void RequestShareCharacterUseCancel(int cid, int uid, SharingModel.SharingCharacterType sharingCharacterType)
        {
            sharingModel.RequestShareCharacterUseSetting(SharingModel.CharacterShareFlag.Cancel, cid, uid, sharingCharacterType).WrapNetworkErrors();
        }

        /// <summary>
        /// 클론 캐릭터 사용
        /// </summary>
        public void RequestCloneCharacterUse(int cid, int uid, SharingModel.CloneCharacterType cloneType = SharingModel.CloneCharacterType.MyCharacter)
        {
            Entity.player.Sharing.RequestShareCloneCharacter(cloneType, uid, cid).WrapNetworkErrors();
        }

        /// <summary>
        /// 클론 캐릭터 사용취소
        /// </summary>
        public void RequestCloneCharacterUseCancel(int cid, int uid, SharingModel.CloneCharacterType cloneType = SharingModel.CloneCharacterType.MyCharacter)
        {
            Entity.player.Sharing.RequestReleaseCloneCharacter(cloneType).WrapNetworkErrors();
        }

        /// <summary>
        /// 셰어 캐릭터 자동장착
        /// </summary>
        public void RequestAutoCharacterShare(bool isOnlyDummy)
        {
            sharingModel.RequestShareCharacterAutoUse(isOnlyDummy).WrapNetworkErrors();
        }

        /// <summary>
        /// 일일 무료 충전 사용
        /// </summary>
        public void RequestShareCharacterFreeDailyTimeTicketUse()
        {
            sharingModel.RequestShareCharacterTimeTicketUse(ShareTicketType.DailyFree).WrapNetworkErrors();
        }

        /// <summary>
        /// 셰어 캐릭터 충전 티켓 사용
        /// </summary>
        public void RequestShareCharacterTimeTicketUse(ShareTicketType ticketType)
        {
            sharingModel.RequestShareCharacterTimeTicketUse(ticketType).WrapNetworkErrors();
        }

        /// <summary>
        /// 셰어 캐릭터 충전 티켓 구매
        /// </summary>
        public void RequestShopPurchase(ShareTicketType ticketType)
        {
            ShopInfo info = GetShopInfo(ticketType);
            if (info == null)
                return;

            shopModel.RequestNormalShopPurchase(info.ID).WrapNetworkErrors();
        }

        /// <summary>
        /// 인벤 확장
        /// </summary>
        public void RequestInvenExpand()
        {
            inventoryModel.RequestInvenExpand().WrapNetworkErrors();
        }

        /// <summary>
        /// 전직 이벤트 호출
        /// </summary>
        private void InvokeChangedJob(bool isInit)
        {
            OnUpdateJob?.Invoke();
        }

        public int GetJobGrade()
        {
            return characterModel.JobGrade();
        }

        public int GetOpenedCloneSlotCount()
        {
            return sharingModel.GetCloneSlotCount(characterModel.JobGrade());
        }

        public int GetOpenedShareSlotCount()
        {
            return sharingModel.GetShareSlotCount(characterModel.JobGrade(), questModel.IsOpenContent(ContentType.ShareHope, false));
        }

        public ShareviceState GetShareviceState()
        {
            return sharingModel.GetShareviceState();
        }

        /// <summary>
        /// 셰어바이스 경험치 아이템 체크
        /// </summary>
        public bool HasViceExperienceItem()
        {
            var viceItems = inventoryModel.itemList.FindAll(a => a is PartsItemInfo && a.ItemType == ItemType.ShareVice);
            return viceItems.ToArray().Length > 0;
        }

        /// <summary>
        /// 셰어바이스 레벨업에 필요한 미로 클리어 조건
        /// </summary>
        public int GetLevelUpShareviceId()
        {
            return mazeRepo.GetEventContentUnlock(ContentType.ShareLevelUp).name_id;
        }

        /// <summary>
        /// 셰어바이스 필터 가능 여부
        /// </summary>
        public bool IsCheckShareFilter()
        {
            return characterModel.IsCheckJobGrade(Constants.OpenCondition.NEED_SHARE_FILTER_JOB_GRADE, false);
        }

        /// <summary>
        /// 쉐어포스 스탯 빨콩 여부
        /// </summary>
        public bool HasNoticeShareForceStat()
        {
            return characterModel.CanShareForceStatusUpgrade();
        }

        /// <summary>
        /// 쉐어 동료 사용 가능 여부
        /// </summary>
        public bool IsCheckUseShare()
        {
            return battleManager.Mode != BattleMode.TimePatrol; // 타임패트롤 내부에서는 쉐어동료 사용 불가능
        }
    }
}