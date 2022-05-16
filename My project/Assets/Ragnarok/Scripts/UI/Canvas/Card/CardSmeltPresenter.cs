//#define USE_SELECT_VIEW

using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICardSmelt"/>
    /// </summary>
    public class CardSmeltPresenter : ViewPresenter
#if USE_SELECT_VIEW
        , UICardSmeltMaterialSelectSlot.Impl
#endif
    {
        private const string KEY = nameof(CardSmeltPresenter) + "_AutoSaveRestorePointSetting";

        private const string TAG = nameof(CardSmeltPresenter);

        private readonly ItemDataManager itemDataRepo;
        private readonly InventoryModel inventoryModel;
        private readonly GoodsModel goodsModel;

#if USE_SELECT_VIEW
        private readonly List<ItemInfo> smeltMaterals; // 연속 제련시 사용할 재료 목록
#else
        private bool isContinuousShowWarning;
        private bool isContinuousSmeltConfirm;
        public bool IsContinuousShowWarning() => isContinuousShowWarning;
#endif
        private int skipWaringSmeltLevel; // 스킵할 경고 팝업 제련 레벨

        private int curAgreedRestorePointLevel; // 유저가 마지막으로 이 레벨의 카드 정보를 복원 포인트로 저장하겠다고 동의한 레벨.
        private bool isCheckRestorePoint;
        public bool IsCheckRestorePoint() => isCheckRestorePoint;

        private bool isProgressSmeltCard;
        public bool IsProgressSmeltCard() => isProgressSmeltCard; // 대기 프로그래스바 연출중 여부

        private bool isPlayingResultEffect;
        private bool IsPlayingResultEffect() => isPlayingResultEffect; // 제련 결과 연출중 여부

        public event System.Action OnUpdateView;
        public event System.Action OnUpdateGoodsZeny;
        public event System.Action OnSmeltProgress;
        public event System.Action<bool> OnSmeltResultEffect;
        public event System.Action OnSmeltResult;
#if USE_SELECT_VIEW
        public event System.Action OnSelectMaterials;
#else
        public event System.Action OnShowWarning;
#endif
        public event System.Action OnCheckRestorePoint;

        public CardSmeltPresenter()
        {
#if USE_SELECT_VIEW
            smeltMaterals = new List<ItemInfo>();
#endif
            itemDataRepo = ItemDataManager.Instance;
            inventoryModel = Entity.player.Inventory;
            goodsModel = Entity.player.Goods;
            useMaterials = new Dictionary<int, int>();
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += InvokeUpdateGoodsZeny;
        }

        public override void RemoveEvent()
        {
            Timing.KillCoroutines(TAG);

            goodsModel.OnUpdateZeny -= InvokeUpdateGoodsZeny;
        }

        public ItemInfo info { get; private set; } // 제련할 카드
        public ItemInfo smeltMaterial { get; private set; } // 카드 제련 재료

        public bool IsMaxLevel => info.IsCardMaxLevel;
        public int NeedZeny => info.NeedCardSmeltZeny;
        public long HaveZeny => goodsModel.Zeny;

        public ItemInfo StartItem { get; private set; }
        public ItemInfo EndItem { get; private set; }
        public ItemInfo BeforeItem { get; private set; }
        public ItemInfo AfterItem { get; private set; }

        private readonly Dictionary<int, int> useMaterials; // 연속 제련시 사용한 재료 목록 (ItemId, 수량)

        private int useZeny; // 연속 제련시 사용한 제니량

        /// <summary>
        /// 레벨업 할 카드정보
        /// </summary>
        /// <param name="info"></param>
        public void SetCardInfo(CardItemInfo info)
        {
            this.info = info;
            skipWaringSmeltLevel = 0;
            curAgreedRestorePointLevel = 0;
            OnUpdateView?.Invoke();
        }

        public bool GetAutoSaveRestorePointSetting()
        {
            return PlayerPrefs.HasKey(KEY) ? PlayerPrefs.GetInt(KEY) > 0 : false;
        }

        public void SetAutoSaveRestorePointSetting(bool value)
        {
            PlayerPrefs.SetInt(KEY, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 카드 제련 재료
        /// </summary>
        public ItemInfo GetCardSmeltMaterial()
        {
            if (info == null)
                return null;

            var materisls = inventoryModel.itemList
                .Where(x => x is PartsItemInfo && x.IsSmeltMaterial && x.MaxSmelt > info.CardLevel)
                .OrderBy(x => x.MaxSmelt).ToArray();            

            if (materisls.Length > 0)
            {
                smeltMaterial = materisls[0];
                return smeltMaterial;
            }

            return smeltMaterial = null;
        }

#if USE_SELECT_VIEW
        /// <summary>
        /// 사용 가능한 카드재료 아이템 목록
        /// </summary>
        private ItemInfo[] GetCardSmeltMaterials()
        {
            if (info == null)
                return null;

            var materisls = inventoryModel.itemList
                .Where(x => x is PartsItemInfo && x.IsSmeltMaterial && x.MaxSmelt > info.CardLevel)
                .OrderBy(x => x.MaxSmelt).ToArray();

            return materisls;
        }

        public UICardSmeltMaterialSelectSlot.Info[] GetMaterialsSelectSlotInfos()
        {
            var data = GetCardSmeltMaterials();
            var models = new UICardSmeltMaterialSelectSlot.Info[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                models[i] = new UICardSmeltMaterialSelectSlot.Info(data[i], this);
                if (info.CardLevel >= data[i].WaringSmeltLevel && info.CardLevel < data[i].MaxSmelt)
                {
                    smeltMaterals.Add(data[i]);
                }
            }
            return models;
        }
#endif

        /// <summary>
        /// 제련 가능 여부 (개별 제련)
        /// </summary>
        public bool CanSmelt()
        {
            if (smeltMaterial == null)
                return false;

            if (HaveZeny < NeedZeny)
                return false;

            if (IsMaxLevel)
                return false;

            return true;
        }

        /// <summary>
        /// 더 높은 레벨의 재료 사용 시
        /// 같은 재료일때 한번만 경고 팝업 보여준다
        /// </summary>
        /// <returns></returns>
        public bool IsWaringPopup()
        {
            return info.CardLevel < smeltMaterial.WaringSmeltLevel
                && smeltMaterial.WaringSmeltLevel > skipWaringSmeltLevel;
        }

        /// <summary>
        /// 카드 제련 요청
        /// </summary>
        public void RequestSmeltCard()
        {
            if (isPlayingResultEffect)
            {
                isPlayingResultEffect = false;
                Timing.KillCoroutines(TAG);
                OnUpdateView?.Invoke();
            }
            skipWaringSmeltLevel = smeltMaterial.WaringSmeltLevel;
            Timing.RunCoroutine(YieldSmeltCard(), TAG);
        }

        /// <summary>
        /// 카드 제련 (단일)
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> YieldSmeltCard()
        {
            int level = info.CardLevel;
            SetBeforeItem();

            Task<Response> task = inventoryModel.RequestSmeltCard(info, smeltMaterial);
            yield return Timing.WaitUntilTrue(task.IsComplete);

            // 제련 결과 연출 작업
            SetAterItem();
            bool isSuccess = info.CardLevel > level;
            OnSmeltResultEffect?.Invoke(isSuccess);
            isPlayingResultEffect = true;
            yield return Timing.WaitUntilFalse(IsPlayingResultEffect);
            OnUpdateView?.Invoke();
        }

        void InvokeUpdateGoodsZeny(long value)
        {
            OnUpdateGoodsZeny?.Invoke();
        }

#if USE_SELECT_VIEW
        /// <summary>
        /// 연속 제련 재료 목록 초기화
        /// </summary>
        public void ClearMaterials()
        {
            smeltMaterals.Clear();
        }

        /// <summary>
        /// 선택된 재료 있는지 여부
        /// </summary>
        /// <returns></returns>
        public bool IsSmeltMaterials()
        {
            return smeltMaterals.Count > 0;
        }

        /// <summary>
        /// 연속 제련 재료 선택 토글
        /// </summary>
        /// <param name="item"></param>
        void UICardSmeltMaterialSelectSlot.Impl.OnSelect(ItemInfo item)
        {
            if (smeltMaterals.Contains(item))
            {
                smeltMaterals.Remove(item);
            }
            else
            {
                smeltMaterals.Add(item);
            }
            OnSelectMaterials?.Invoke();
        }

        /// <summary>
        /// 연속 제련 재료 목록 선택 여부
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool UICardSmeltMaterialSelectSlot.Impl.IsSelect(ItemInfo item)
        {
            return smeltMaterals.Contains(item);
        }
#endif

        /// <summary>
        /// 사용한 제련 재료 목록 정보
        /// </summary>
        /// <returns></returns>
        public UICardSmeltResultMaterialSlot.Info[] GetUseMaterials()
        {
            List<UICardSmeltResultMaterialSlot.Info> infos = new List<UICardSmeltResultMaterialSlot.Info>();
            foreach (var item in useMaterials)
            {
                ItemData data = itemDataRepo.Get(item.Key);
                var info = new UICardSmeltResultMaterialSlot.Info(data.icon_name, item.Value);
                infos.Add(info);
            }
            return infos.ToArray();
        }

        /// <summary>
        /// 사용한 제니
        /// </summary>
        /// <returns></returns>
        public int UseZeny()
        {
            return useZeny;
        }

        /// <summary>
        /// 연속 제련 시작
        /// </summary>
        public void RequestContinuousSmeltCard()
        {
#if USE_SELECT_VIEW
            // 선택 된 재료 없음
            if (smeltMaterals.Count == 0)
                return;
#endif

            GetCardSmeltMaterial();

            if (smeltMaterial == null)
                return;

            curAgreedRestorePointLevel = 0;
            useMaterials.Clear();
            useZeny = 0;
            SetStartItem();

            isProgressSmeltCard = false; // 첫 실행은 프로그래스 연출 없음
            isCheckRestorePoint = false;
#if !USE_SELECT_VIEW
            skipWaringSmeltLevel = smeltMaterial.WaringSmeltLevel;
#endif
            Timing.RunCoroutine(YieldContinuousSmeltCard(), TAG);
        }

        private void SetStartItem()
        {
            StartItem = new CardItemInfo();
            StartItem.SetData(itemDataRepo.Get(info.ItemId));
            StartItem.SetItemInfo(info.CardLevel, 0, 0
                , info.GetCardOptionValue(0)
                , info.GetCardOptionValue(1)
                , info.GetCardOptionValue(2)
                , info.GetCardOptionValue(3)
                , info.IsLock
                , info.ItemTranscend
                , info.ItemChangedElement
                , info.ElementLevel);
        }

        private void SetEndItem()
        {
            EndItem = new CardItemInfo();
            EndItem.SetData(itemDataRepo.Get(info.ItemId));
            EndItem.SetItemInfo(info.CardLevel, 0, 0
                , info.GetCardOptionValue(0) - StartItem.GetCardOptionValue(0)
                , info.GetCardOptionValue(1) - StartItem.GetCardOptionValue(1)
                , info.GetCardOptionValue(2) - StartItem.GetCardOptionValue(2)
                , info.GetCardOptionValue(3) - StartItem.GetCardOptionValue(3)
                , info.IsLock
                , info.ItemTranscend
                , info.ItemChangedElement
                , info.ElementLevel);
        }

        private void SetBeforeItem()
        {
            BeforeItem = new CardItemInfo();
            BeforeItem.SetData(itemDataRepo.Get(info.ItemId));
            BeforeItem.SetItemInfo(info.CardLevel, 0, 0
                , info.GetCardOptionValue(0)
                , info.GetCardOptionValue(1)
                , info.GetCardOptionValue(2)
                , info.GetCardOptionValue(3)
                , info.IsLock
                , info.ItemTranscend
                , info.ItemChangedElement
                , info.ElementLevel);
        }

        private void SetAterItem()
        {
            // 제련 결과 옵션 증감량
            AfterItem = new CardItemInfo();
            AfterItem.SetData(itemDataRepo.Get(info.ItemId));
            AfterItem.SetItemInfo(info.CardLevel, 0, 0
                , info.GetCardOptionValue(0) - BeforeItem.GetCardOptionValue(0)
                , info.GetCardOptionValue(1) - BeforeItem.GetCardOptionValue(1)
                , info.GetCardOptionValue(2) - BeforeItem.GetCardOptionValue(2)
                , info.GetCardOptionValue(3) - BeforeItem.GetCardOptionValue(3)
                , info.IsLock
                , info.ItemTranscend
                , info.ItemChangedElement
                , info.ElementLevel);
        }

        /// <summary>
        /// 연속 제련 중단
        /// </summary>
        public void StopContinuousSmeltCard()
        {
            // 결과 팝업 표시
            Timing.KillCoroutines(TAG);
            OnResultView();
        }

        IEnumerator<float> YieldContinuousSmeltCard()
        {
            while (true)
            {
                SetBeforeItem();

#if USE_SELECT_VIEW
                ItemInfo materisl = smeltMaterals
                    .Where(x => x.MaxSmelt > info.CardLevel)
                    .OrderBy(x => x.MaxSmelt).FirstOrDefault<ItemInfo>();
#else
                ItemInfo materisl = GetCardSmeltMaterial();
#endif

                // 재료 부족, 제니 부족 제련 중단, 최대 레벨 도달
                if (materisl == null || HaveZeny < info.NeedCardSmeltZeny || info.IsCardMaxLevel)
                {
                    OnResultView();
                    yield break;
                }

#if !USE_SELECT_VIEW
                if (IsWaringPopup())
                {
                    isContinuousShowWarning = true;
                    OnShowWarning?.Invoke();
                    yield return Timing.WaitUntilFalse(IsContinuousShowWarning);

                    if (!isContinuousSmeltConfirm)
                    {
                        OnResultView();
                        yield break;
                    }

                    skipWaringSmeltLevel = smeltMaterial.WaringSmeltLevel;
                }
#endif

                CardItemInfo cardInfo = info as CardItemInfo;

                // progress 바 연출이 뜨기 전에만 표시.(최초에는 연속 강화 View가 뜨기 전, 밖에서 체크하므로.)
                if (isProgressSmeltCard && info.CardLevel % 10 == 0 && curAgreedRestorePointLevel < info.CardLevel && cardInfo.RestorePointLevel < info.CardLevel)
                {
                    isCheckRestorePoint = true;
                    OnCheckRestorePoint?.Invoke(); // 복원 포인트 저장 여부 체크
                    yield return Timing.WaitUntilFalse(IsCheckRestorePoint);
                }

#if USE_SELECT_VIEW
                if (materisl.ItemCount == 1)
                    smeltMaterals.Remove(materisl);
#endif

                if (isProgressSmeltCard) // 다음 연속 제련 대기 연출
                    OnSmeltProgress?.Invoke();

                yield return Timing.WaitUntilFalse(IsProgressSmeltCard);

                int materialId = materisl.ItemId;
                int zeny = info.NeedCardSmeltZeny;
                int level = info.CardLevel; // 제련 전 레벨

                Task<Response> task = inventoryModel.RequestSmeltCard(info, materisl);
                yield return Timing.WaitUntilTrue(task.IsComplete);

                // 결과 연출
                SetAterItem();
                isPlayingResultEffect = true;
                bool isSuccess = info.CardLevel > level;
                OnSmeltResultEffect?.Invoke(isSuccess);
                yield return Timing.WaitUntilFalse(IsPlayingResultEffect);

                // 소모한 재료 아이템 & 제니 정보 저장
                if (!useMaterials.ContainsKey(materialId))
                {
                    useMaterials.Add(materialId, 1);
                }
                else
                {
                    useMaterials[materialId] += 1;
                }
                useZeny += zeny;

                OnUpdateView?.Invoke();
                isProgressSmeltCard = true;
            }
        }

#if !USE_SELECT_VIEW
        public void ConfirmWarning(bool isConfirm)
        {
            isContinuousShowWarning = false;
            isContinuousSmeltConfirm = isConfirm;
        }
#endif

        /// <summary>
        /// 결과창 표시 이벤트
        /// </summary>
        private void OnResultView()
        {
            SetEndItem();
            OnSmeltResult?.Invoke();
        }

        /// <summary>
        /// 다음 제련 시작
        /// </summary>
        public void NextSmelt()
        {
            isProgressSmeltCard = false;
        }

        /// <summary>
        /// 연출 종료
        /// </summary>
        public void FinishResultEffect()
        {
            isPlayingResultEffect = false;
        }

        public void OnUserAgreeToSaveCheckPoint()
        {
            isCheckRestorePoint = false;
        }

        public void SaveRestoreCheckPoint()
        {
            curAgreedRestorePointLevel = info.CardLevel;
        }
    }
}
