using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DailyCheckPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void OnUpdateTreeView();
        }

        private readonly IView view;
        private readonly UserModel userModel;
        private readonly CharacterModel characterModel;
        private readonly QuestModel questModel;
        private readonly ShopModel shopModel;
        private readonly BingoModel bingoModel;
        private readonly EventModel eventModel;
        private readonly InventoryModel inventoryModel;

        private readonly int baseZeny;
        private readonly int tickSecond;
        private readonly float zenyCoefficient;

        /// <summary>
        /// 룰렛 뽑기 제한 레벨
        /// </summary>
        private int jobLevelLimit;

        MaterialBaseView materialBaseView;

        /// <summary>
        /// 어둠의 나무
        /// </summary>
        public IDarkTreeInfo DarkTree => inventoryModel.DarkTree;

        public event System.Action OnUpdateEventQuest
        {
            add
            {
                questModel.OnEventQuest += value;
                questModel.OnStandByReward += value;
            }
            remove
            {
                questModel.OnEventQuest -= value;
                questModel.OnStandByReward -= value;
            }
        }

        public event System.Action OnUpdateEventNotice;

        public event System.Action OnUpdateDarkTree
        {
            add => DarkTree.OnUpdate += value;
            remove => DarkTree.OnUpdate -= value;
        }

        public event System.Action OnUpdateWordCollectionItemCount
        {
            add => eventModel.OnUpdateWordCollectionItemCount += value;
            remove => eventModel.OnUpdateWordCollectionItemCount -= value;
        }

        public DailyCheckPresenter(IView view)
        {
            this.view = view;
            userModel = Entity.player.User;
            characterModel = Entity.player.Character;
            questModel = Entity.player.Quest;
            shopModel = Entity.player.ShopModel;
            bingoModel = Entity.player.Bingo;
            eventModel = Entity.player.Event;
            inventoryModel = Entity.player.Inventory;
            materialGroupId = CurMaterialGroupId;

            baseZeny = BasisType.TREE_REWARD_ZENY_BASE.GetInt(); // 400
            tickSecond = BasisType.TREE_REWARD_ACCRUE_SECOND.GetInt(); // 800
            zenyCoefficient = BasisType.TREE_REWARD_ZENY_COEFFICIENT.GetFloat(); // 1.015 (10150)
            jobLevelLimit = BasisType.SPECIAL_ROULETTE_JOB_LEVEL_LIMIT.GetInt();
        }

        public override void AddEvent()
        {
            characterModel.OnUpdateJobLevel += OnUpdateJobLevel;
            userModel.OnTreeReward += view.OnUpdateTreeView;
            DarkTree.OnUpdate += view.OnUpdateTreeView;

            OnUpdateJobLevel(characterModel.JobLevel);
        }

        public override void RemoveEvent()
        {
            characterModel.OnUpdateJobLevel -= OnUpdateJobLevel;
            userModel.OnTreeReward -= view.OnUpdateTreeView;
            DarkTree.OnUpdate -= view.OnUpdateTreeView;
        }

        void OnUpdateJobLevel(int level)
        {
            // 직업 레벨이 변경되고 그룹아이디가 변경되었을때 재료나무 보상 갱신
            if (materialBaseView == null)
                return;
            if (materialGroupId == CurMaterialGroupId)
                return;
            materialGroupId = CurMaterialGroupId;
            materialBaseView.Show();

            OnUpdateEventNotice?.Invoke();
        }

        public void SetMaterialBaseView(MaterialBaseView view)
        {
            materialBaseView = view;
        }

        public bool IsNewDaily => GetIsNewDaily();

        public void SetNewDaily(bool value)
        {
            userModel.SetNewDaily(value);
        }

        /// <summary>
        /// 출석체크 보상 목록 반환
        /// </summary>
        /// <returns></returns>
        public DailyCheckInfo[] GetDailyCheckInfos()
        {
            return userModel.GetDailyCheckInfos();
        }

        /// <summary>
        /// 출석 일수 반환
        /// </summary>
        public byte DailyCount => userModel.DailyCount;

        int materialGroupId;

        /// <summary>
        /// 현재 재료나무 보상 정보 그룹아이디
        /// </summary>
        int CurMaterialGroupId => (int)(characterModel.JobLevel / BasisType.TREE_REWARD_RESOURCE_REF.GetInt()) + 1;

        /// <summary>
        /// 접속보상 Index
        /// </summary>
        public int ConnectTimeRewardIndex => userModel.ConnectTimeRewardIndex;

        /// <summary>
        /// 금일 누적 접속 시간(초)
        /// </summary>
        public CumulativeTime DayConnectionTime => userModel.DayConnectionTime;

        /// <summary>
        /// 제니 나무 시간
        /// </summary>
        public CumulativeTime ZenyTreeTime => userModel.ZenyTreeTime;

        /// <summary>
        /// 재료 나무 시간
        /// </summary>
        public CumulativeTime MaterialTreeTime => userModel.MaterialTreeTime;

        /// <summary>
        /// 나무 최대 누적 시간
        /// </summary>
        public float TreeMaxTime => BasisType.TREE_REWARD_MAX_SECOND.GetInt() * 1000f;

        /// <summary>
        /// 제니 현재 누적시간
        /// </summary>
        public float CurZenyTreeTime => ZenyTreeTime.ToCumulativeTime() >= TreeMaxTime ? TreeMaxTime : ZenyTreeTime.ToCumulativeTime();

        /// <summary>
        /// 재료 나무 현재 누적시간
        /// </summary>
        public float CurMaterialTreeTime => MaterialTreeTime.ToCumulativeTime() >= TreeMaxTime ? TreeMaxTime : MaterialTreeTime.ToCumulativeTime();

        /// <summary>
        /// 재료나무 보상 가능 여부
        /// </summary>
        public bool IsMaterialTreeReward => (int)(CurMaterialTreeTime / (tickSecond * 1000f)) > 0;

        /// <summary>
        /// 제니나무 보상 가능 여부
        /// </summary>
        public bool IsZenyTreeReward => (int)(CurZenyTreeTime / (tickSecond * 1000f)) > 0;

        /// <summary>
        /// 제니나무 보상 정보
        /// </summary>
        public int ZenyTreeReward
        {
            get
            {
                int tickCount = (int)(CurZenyTreeTime / (tickSecond * 1000f)); // tickSecond = 800
                if (tickCount == 0)
                    return 0;

                float retCoefficient = Mathf.Pow(zenyCoefficient, characterModel.JobLevel); // zenyCoefficient = 1.015 (10150)
                retCoefficient = (float)(MathUtils.RoundToInt(retCoefficient * 100) / 100f);

                return MathUtils.ToInt(baseZeny * retCoefficient) * tickCount; // baseZeny = 400
            }
        }

        /// <summary>
        /// 냥다래 나무 보상 있는지 여부
        /// </summary>
        public bool IsCatCoinReward => userModel.IsCatCoinReward;

        /// <summary>
        /// 어둠의나무 보상 있는지 여부
        /// </summary>
        public bool IsDarkTreeReward => DarkTree.HasStandByReward();

        /// <summary>
        /// 냥다래 나무 보상 정보목록
        /// </summary>
        public CatCoinRewardInfo[] GetCatCoinRewardInfos()
        {
            return userModel.GetCatCoinRewardInfos();
        }

        /// <summary>
        /// 냥다래 나무 이벤트 보상 타입
        /// </summary>
        public RewardType GetCatCoinTreeEventRewardType()
        {
            return BasisType.CAT_COIN_TREE_EVENT_REWARD_TYPE.GetInt().ToEnum<RewardType>();
        }

        /// <summary>
        /// 냥다래 나무 이벤트 추가 보상
        /// </summary>
        public int[] GetCatCoinTreeAddReward()
        {
            int[] rewardCount = new int[5];
            for (int i = 0; i < rewardCount.Length; i++)
            {
                rewardCount[i] = BasisType.CAT_COIN_TREE_RO_POINT_COUNT.GetInt(i + 1);
            }
            return rewardCount;
        }

        /// <summary>
        /// 재료나무 보상 정보 목록
        /// </summary>
        public MaterialRewardInfo[] GetMaterialRewardInfos()
        {
            var gachaData = GachaDataManager.Instance.Gets(GroupType.Material, CurMaterialGroupId);
            List<int> itemList = new List<int>(); // 중복 막기 위함

            for (int i = 0; i < gachaData.Length; i++)
            {
                int itemId = gachaData[i].reward_value; // 아이템 타입만 있는 것으로 간주
                if (itemList.Contains(itemId))
                    continue;

                itemList.Add(itemId);
            }

            MaterialRewardInfo[] materialRewardInfos = new MaterialRewardInfo[itemList.Count];
            for (int i = 0; i < itemList.Count; i++)
            {
                materialRewardInfos[i] = new MaterialRewardInfo();
                materialRewardInfos[i].SetData(new RewardData(RewardType.Item, itemList[i], 1));
            }

            return materialRewardInfos;
        }

        /// <summary>
        /// 냥다래 나무 보상 받기
        /// </summary>
        public async void RequestGetConnectReward()
        {
            await userModel.RequestGetConnectReward();
        }

        /// <summary>
        /// 제니, 재료 나무 보상 받기
        /// </summary>
        /// <param name="isZenyTree"></param>
        public async void RequestGetTreeReward(bool isZenyTree)
        {
            await userModel.RequestGetTreeReward(isZenyTree);
        }

        private bool GetIsNewDaily()
        {
            return userModel.IsNewDaily;

            /*bool isNewDaily = userModel.IsNewDaily;

            if (isNewDaily)
            {
                DailyCheckInfo[] infos = GetDailyCheckInfos();
                if (infos != null)
                {
                    int index = DailyCount - 1;
                    if (index >= 0 || index < infos.Length)
                    {
                        UIDailyCheckBox ui = UI.Show<UIDailyCheckBox>();
                        ui.Show(infos[index].RewardData);
                    }
                }
            }

            return isNewDaily;*/
        }

        public bool IsEventQuestStandByReward()
        {
            return questModel.IsEventQuestStandByReward();
        }

        public bool IsBingoQuestStandByReward()
        {
            return bingoModel.IsBingoQuestStandByReward();
        }

        /// <summary>
        /// 스페셜 룰렛 뽑기 가능 여부
        /// </summary>
        public bool IsSpecialRouletteNotice()
        {
            if (IsJobLevelLimit())
                return false;

            if (!eventModel.IsRemainTimeRoulette())
                return false;

            if (eventModel.IsSpecialRouletteMaxUsed())
                return false;

            int count = inventoryModel.GetItemCount(eventModel.SpecialRouletteItemId);

            return count >= eventModel.GetSpecialRouletteNeedCount();
        }

        /// <summary>
        /// 나무 보상 남은 시간
        /// </summary>
        /// <returns></returns>
        public float GetTreePackRemainTime()
        {
            return shopModel.TreePackRemainTime.ToRemainTime();
        }

        /// <summary>
        /// 나무 보상 패키지 가격
        /// </summary>
        public string GetPriceTreePack()
        {
            int shopId = BasisShop.TreePack.GetID();
            ShopInfo info = shopModel.GetInfo(shopId);

            if (info == null)
                return string.Empty;

            return info.CostText;
        }

        public void ShowTreePack()
        {
            if (Tutorial.isInProgress)
            {
                UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
                return;
            }

            UI.Show<UIPackageTree>().Set(BasisShop.TreePack.GetID());
        }

        /// <summary>
        /// 스페셜 룰렛 직업 레벨 제한 여부
        /// </summary>
        private bool IsJobLevelLimit()
        {
            return characterModel.JobLevel < jobLevelLimit;
        }

        /// <summary>
        /// 어둠의 나무 시작
        /// </summary>
        public void RequestDarkTreeStart()
        {
            inventoryModel.RequestDarkTreeStart().WrapNetworkErrors();
        }

        /// <summary>
        /// 어둠의 나무 보상 받기
        /// </summary>
        public void RequestDarkTreeGetReward()
        {
            inventoryModel.RequestDarkTreeGetReward().WrapNetworkErrors();
        }

        /// <summary>
        /// [출석체크 이벤트] 받을 보상이 있는지 여부
        /// </summary>
        public bool IsAttendEventStandByReward()
        {
            EventQuestGroupInfo eventData = questModel.GetEventQuestByShortCut(ShortCutType.AttendEvent);
            if (eventData == null || eventData.RemainTime.ToRemainTime() <= 0)
                return false;

            return eventModel.IsAttendEventStandByReward();
        }

        /// <summary>
        /// [단어수집 이벤트] 받을 보상이 있는지 여부
        /// </summary>
        public bool IsWordCollectionStandByReward()
        {
            EventQuestGroupInfo eventData = questModel.GetEventQuestByShortCut(ShortCutType.WordCollectionEvent);
            if (eventData == null || eventData.RemainTime.ToRemainTime() <= 0)
                return false;

            return eventModel.IsWordCollectionStandByReward();
        }
    }
}