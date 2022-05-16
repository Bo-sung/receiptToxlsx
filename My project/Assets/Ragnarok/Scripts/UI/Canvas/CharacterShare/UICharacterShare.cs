using MEC;
using Ragnarok.View.CharacterShare;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICharacterShare : UICanvas, TutorialSharingCharacterEquip.ICharacterShareImpl
    {
        private const int TAB_USE_CHARACTER_SHARING = 0;
        private const int TAB_REGISTER_CHARACTER_SHARE = 1;

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton btnMainExit;
        [SerializeField] UIButtonHelper btnToggle;
        [SerializeField] UITabHelper mainTab;
        [SerializeField] CharacterShareView characterShareView;
        [SerializeField] CharacterShareRegisterView characterShareRegisterView;
        [SerializeField] CharacterShareListPopupView characterShareListPopupView;
        [SerializeField] CharacterShareChargePopupView characterShareChargePopupView;
        [SerializeField] UISharevice shareviceLevelUpView;

        public delegate void SelectShareCharacterEvent(int cid, int uid, SharingModel.SharingCharacterType sharingCharacterType);
        public delegate void SelectCloneCharacterEvent(int cid, int uid, SharingModel.CloneCharacterType cloneCharacterType);

        CharacterSharePresenter presenter;

        bool TutorialSharingCharacterEquip.ICharacterShareImpl.IsShareviceLevelUpViewShowing() => shareviceLevelUpView.gameObject.activeSelf;
        bool TutorialSharingCharacterEquip.ICharacterShareImpl.IsShareviceLevelUpViewHiding() => !shareviceLevelUpView.gameObject.activeSelf;
        UISharevice TutorialSharingCharacterEquip.ICharacterShareImpl.GetSharevice() => shareviceLevelUpView;

        protected override void OnInit()
        {
            presenter = new CharacterSharePresenter();

            EventDelegate.Add(btnMainExit.onClick, HideUI);
            EventDelegate.Add(btnToggle.OnClick, SwitchSharevice);

            mainTab.OnSelect += OnSelectMainTab;
            characterShareView.OnFinishShareviceBuff += UpdateShareviceBuffInfo;
            characterShareView.OnSelectFreeCharge += presenter.RequestShareCharacterFreeDailyTimeTicketUse;
            characterShareView.OnSelectCharge += characterShareChargePopupView.Show;
            characterShareView.OnSelectAutoSetting += OnSelectAutoSetting;
            characterShareView.OnSelectAddSlot += ShowCharacterShareListPopupView;
            characterShareView.OnSelectCloneAddSlot += ShowCharacterCloneListPopupView;
            characterShareView.OnSelectDelete += presenter.RequestShareCharacterUseCancel;
            characterShareView.OnSelectCloneDelete += presenter.RequestCloneCharacterUseCancel;
            characterShareView.OnClickLevelUP += OnClickLevelUP;
            characterShareRegisterView.OnSelectBtnInven += ShowInvenUI;
            characterShareRegisterView.OnSelectBtnWeightPlus += presenter.RequestInvenExpand;
            characterShareRegisterView.OnSelectRegister += presenter.RequestShareRegisterStart;
            characterShareRegisterView.OnSelectAutoShare += presenter.RequestShareExitAutoSetting;
            characterShareListPopupView.OnShowNextPage += presenter.RequestShareCharacterList;
            characterShareListPopupView.OnSelect += presenter.RequestShareCharacterUse;
            characterShareListPopupView.OnSelectClone += presenter.RequestCloneCharacterUse;
            characterShareListPopupView.OnSelectTabPlayer += OnSelectTabPlayer;
            characterShareListPopupView.OnSelectTabGuild += OnSelectTabGuild;
            characterShareChargePopupView.OnSelectFreeCharge += presenter.RequestShareCharacterFreeDailyTimeTicketUse;
            characterShareChargePopupView.OnSelectUse += presenter.RequestShareCharacterTimeTicketUse;
            characterShareChargePopupView.OnSelectBuy += presenter.RequestShopPurchase;

            presenter.OnUpdateSharingState += UpdateSharingState;
            presenter.OnUpdateRemainTimeForShare += UpdateRemainTimeForSharing;
            presenter.OnUpdateSharingCharacters += UpdateSharingCharacters;
            presenter.OnUpdateShareFreeTicket += UpdateShareFreeTicket;
            presenter.OnUpdateShareTicketCount += UpdateShareTicketCount;
            presenter.OnUpdateShareCharacterList += UpdateShareCharacterList;
            presenter.OnUpdateJob += UpdateJobIllust;
            presenter.OnUpdateJob += UpdateEnableAutoSetting;
            presenter.OnUpdateGender += UpdateJobIllust;
            presenter.OnUpdateInvenWeight += UpdateInvenWeight;
            presenter.OnUpdateShareExitAutoSetting += UpdateShareExitAutoSetting;
            presenter.OnUpdateFilterCount += UpdateFilterCount;
            presenter.OnLevelUpSharevice += UpdateShareviceBuffInfo;
            presenter.OnShareForceLevelUp += UpdateNotice;
            presenter.OnUpdateShareForceStatus += UpdateNotice;
            presenter.OnUpdateGuildInfo += UpdateGuildInfo;

            presenter.AddEvent();
            shareviceLevelUpView.OnInit();
            shareviceLevelUpView.Hide();

            Initialize();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnMainExit.onClick, HideUI);
            EventDelegate.Remove(btnToggle.OnClick, SwitchSharevice);

            mainTab.OnSelect -= OnSelectMainTab;
            characterShareView.OnFinishShareviceBuff -= UpdateShareviceBuffInfo;
            characterShareView.OnSelectFreeCharge -= presenter.RequestShareCharacterFreeDailyTimeTicketUse;
            characterShareView.OnSelectCharge -= characterShareChargePopupView.Show;
            characterShareView.OnSelectAutoSetting -= OnSelectAutoSetting;
            characterShareView.OnSelectAddSlot -= ShowCharacterShareListPopupView;
            characterShareView.OnSelectCloneAddSlot -= ShowCharacterCloneListPopupView;
            characterShareView.OnSelectDelete -= presenter.RequestShareCharacterUseCancel;
            characterShareView.OnSelectCloneDelete -= presenter.RequestCloneCharacterUseCancel;
            characterShareView.OnClickLevelUP -= OnClickLevelUP;
            characterShareRegisterView.OnSelectBtnInven -= ShowInvenUI;
            characterShareRegisterView.OnSelectBtnWeightPlus -= presenter.RequestInvenExpand;
            characterShareRegisterView.OnSelectRegister -= presenter.RequestShareRegisterStart;
            characterShareRegisterView.OnSelectAutoShare -= presenter.RequestShareExitAutoSetting;
            characterShareListPopupView.OnShowNextPage -= presenter.RequestShareCharacterList;
            characterShareListPopupView.OnSelect -= presenter.RequestShareCharacterUse;
            characterShareListPopupView.OnSelectClone -= presenter.RequestCloneCharacterUse;
            characterShareListPopupView.OnSelectTabPlayer -= OnSelectTabPlayer;
            characterShareListPopupView.OnSelectTabGuild -= OnSelectTabGuild;
            characterShareChargePopupView.OnSelectFreeCharge -= presenter.RequestShareCharacterFreeDailyTimeTicketUse;
            characterShareChargePopupView.OnSelectUse -= presenter.RequestShareCharacterTimeTicketUse;
            characterShareChargePopupView.OnSelectBuy -= presenter.RequestShopPurchase;

            presenter.OnUpdateSharingState -= UpdateSharingState;
            presenter.OnUpdateRemainTimeForShare -= UpdateRemainTimeForSharing;
            presenter.OnUpdateSharingCharacters -= UpdateSharingCharacters;
            presenter.OnUpdateShareFreeTicket -= UpdateShareFreeTicket;
            presenter.OnUpdateShareTicketCount -= UpdateShareTicketCount;
            presenter.OnUpdateShareCharacterList -= UpdateShareCharacterList;
            presenter.OnUpdateJob -= UpdateJobIllust;
            presenter.OnUpdateJob -= UpdateEnableAutoSetting;
            presenter.OnUpdateGender -= UpdateJobIllust;
            presenter.OnUpdateInvenWeight -= UpdateInvenWeight;
            presenter.OnUpdateShareExitAutoSetting -= UpdateShareExitAutoSetting;
            presenter.OnUpdateFilterCount -= UpdateFilterCount;
            presenter.OnLevelUpSharevice -= UpdateShareviceBuffInfo;
            presenter.OnShareForceLevelUp -= UpdateNotice;
            presenter.OnUpdateShareForceStatus -= UpdateNotice;
            presenter.OnUpdateGuildInfo -= UpdateGuildInfo;

            presenter.RemoveEvent();
            shareviceLevelUpView.OnClose();
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_Sharing(); // 신규 컨텐츠 플래그 제거

            mainTab.Value = TAB_USE_CHARACTER_SHARING;
            characterShareListPopupView.Hide();
            characterShareChargePopupView.Hide();

            UpdateNotice();
            UpdateRemainTimeForSharing();
            UpdateSharingCharacters();
            UpdateShareFreeTicket();
            UpdateShareTicketCount();
            UpdateJobIllust();
            UpdateInvenWeight();
            UpdateShareExitAutoSetting();
            UpdateEnableAutoSetting();
            // 현재 필터중인 갯수 갱신
            UpdateFilterCount();
            // 셰어바이스 레벨버프 갱신
            UpdateShareviceBuffInfo();
        }

        protected override void OnHide()
        {
            presenter.ResetData();

            characterShareView.ResetData();
            characterShareListPopupView.ResetData();
        }

        protected override void OnLocalize()
        {
            btnToggle.LocalKey = LocalizeKey._10268; // 2세대
            mainTab[TAB_USE_CHARACTER_SHARING].LocalKey = LocalizeKey._10229; // 셰어 이용
            mainTab[TAB_REGISTER_CHARACTER_SHARE].LocalKey = LocalizeKey._10230; // 셰어 등록
            shareviceLevelUpView.OnLocalize();
        }

        public void SetRegisterMode()
        {
            mainTab.Value = TAB_REGISTER_CHARACTER_SHARE;
        }

        private void HideUI()
        {
            UI.Close<UICharacterShare>();
        }

        void SwitchSharevice()
        {
            UI.Show<UICharacterShare2nd>();
            HideUI();
        }

        private void Initialize()
        {
            characterShareChargePopupView.Initialize(presenter.GetFreeDailyShareTimeTotalHours(), presenter.GetMaxShareTimeTotalHours());

            foreach (ShareTicketType item in System.Enum.GetValues(typeof(ShareTicketType)))
            {
                if (item == ShareTicketType.None || item == ShareTicketType.DailyFree)
                    continue;

                CharacterShareChargePopupView.IInitializer shopInfo = presenter.GetShopInfo(item);
                if (shopInfo == null)
                    continue;

                characterShareChargePopupView.Initialize(item, shopInfo);
            }
        }

        private void OnSelectMainTab(int index)
        {
            characterShareView.SetActive(index == TAB_USE_CHARACTER_SHARING);
            characterShareRegisterView.SetActive(index == TAB_REGISTER_CHARACTER_SHARE);
        }

        private void OnSelectAutoSetting()
        {
            if (!presenter.IsCheckUseShare())
            {
                UI.ShowToastPopup(LocalizeKey._90315.ToText()); // 타임 패트롤 내부에서는 동료 쉐어를 이용할 수 없습니다.
                return;
            }

            if (presenter.IsFinishedSharingTime())
            {
                UI.ShowToastPopup(LocalizeKey._10227.ToText()); // 캐릭터 셰어 시간을 충전 후 이용하시기 바랍니다.
                return;
            }

            bool isOnlyDummy = !Tutorial.HasAlreadyFinished(TutorialType.SharingCharacterEquip); // 셰어캐릭터 장착 튜토리얼을 하지 않았을 때
            presenter.RequestAutoCharacterShare(isOnlyDummy);
        }

        private void OnSelectTabPlayer()
        {
            presenter.ResetData();
            presenter.RequestShareCharacterList();
        }

        private void OnSelectTabGuild()
        {
            presenter.ResetData();
            presenter.RequestGuildInfo();
        }

        private void ShowCharacterShareListPopupView()
        {
            if (!presenter.IsCheckUseShare())
            {
                UI.ShowToastPopup(LocalizeKey._90315.ToText()); // 타임 패트롤 내부에서는 동료 쉐어를 이용할 수 없습니다.
                return;
            }

            if (presenter.IsFinishedSharingTime())
            {
                UI.ShowToastPopup(LocalizeKey._10227.ToText()); // 캐릭터 셰어 시간을 충전 후 이용하시기 바랍니다.
                return;
            }

            characterShareListPopupView.SetIsClone(isClone: false);
            characterShareListPopupView.Show();
        }

        private void ShowCharacterCloneListPopupView()
        {
            if (!presenter.IsCheckUseShare())
            {
                UI.ShowToastPopup(LocalizeKey._90315.ToText()); // 타임 패트롤 내부에서는 동료 쉐어를 이용할 수 없습니다.
                return;
            }

            if (presenter.IsFinishedSharingTime())
            {
                UI.ShowToastPopup(LocalizeKey._10227.ToText()); // 캐릭터 셰어 시간을 충전 후 이용하시기 바랍니다.
                return;
            }

            presenter.ResetData(); // 셰어캐릭터 목록 캐싱 제거

            characterShareListPopupView.SetIsClone(isClone: true);
            characterShareListPopupView.Show();
            Timing.RunCoroutineSingleton(DelayShowCloneCharacterList().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> DelayShowCloneCharacterList()
        {
            yield return Timing.WaitForOneFrame;

            UpdateCloneCharacterList();
        }

        private void ShowInvenUI()
        {
            HideUI();
            UI.Show<UIInven>();
        }

        private void UpdateSharingState()
        {
            SharingModel.SharingState state = presenter.GetSharingState();
            if (state == SharingModel.SharingState.Sharing)
            {
                HideUI();
                UI.Show<UICharacterShareWaiting>();
            }
        }

        private void UpdateRemainTimeForSharing()
        {
            float remainTimeForShare = presenter.GetRemainTimeForShare();
            characterShareView.SetRemainTime(remainTimeForShare);
        }

        private void UpdateSharingCharacters()
        {
            UISimpleCharacterShareBar.IInput[] sharingCharacters = presenter.GetSharingCharacters();
            UISimpleCharacterShareBar.IInput[] cloneCharacters = presenter.GetCloneCharacters();
            characterShareView.SetData(sharingCharacters, cloneCharacters, presenter.GetJobGrade(), presenter.GetOpenedShareSlotCount(), presenter.GetOpenedCloneSlotCount(), presenter.GetLevelUpShareviceId(), presenter.IsCheckShareFilter());
        }

        private void UpdateShareFreeTicket()
        {
            bool hasFreeTicket = presenter.HasShareFreeTicket();
            characterShareView.SetChargeNotice(hasFreeTicket);
            characterShareChargePopupView.SetFreeChargeTicket(hasFreeTicket);
        }

        private void UpdateShareTicketCount()
        {
            foreach (ShareTicketType item in System.Enum.GetValues(typeof(ShareTicketType)))
            {
                if (item == ShareTicketType.None)
                    continue;

                characterShareChargePopupView.SetTicketCount(item, presenter.GetShareTicketCount(item));
            }
        }

        private void UpdateGuildInfo()
        {
            bool haveGuild = presenter.HaveGuild();
            int donationPoint = presenter.GetGuildDonationPoint();

            characterShareListPopupView.SetGuildData(haveGuild, donationPoint);

            // 길드가 없는 경우 || // 기여도 부족
            if (!haveGuild || donationPoint < BasisType.GUILD_SHARE_NEED_DONATION.GetInt())
            {
                UpdateShareCharacterList();
                return;
            }

            presenter.RequestGuildShareCharacterList();
        }

        private void UpdateShareCharacterList()
        {
            UISimpleCharacterShareBar.IInput[] shareCharacterList = presenter.GetShareCharacterList();
            characterShareListPopupView.SetData(shareCharacterList);
        }

        private void UpdateCloneCharacterList()
        {
            UISimpleCharacterShareBar.IInput[] cloneCharacterList = presenter.GetCloneCharacterList();
            characterShareListPopupView.SetCloneData(cloneCharacterList);
        }

        private void UpdateJobIllust()
        {
            characterShareRegisterView.SetJobIllust(presenter.GetJobIllustTextureName());
        }

        private void UpdateInvenWeight()
        {
            int cur = presenter.GetCurrentInvenWeight();
            int max = presenter.GetMaxInvenWeight();
            characterShareRegisterView.UpdateWeightValue(cur, max);
        }

        private void UpdateShareExitAutoSetting()
        {
            bool isShareExitAutoSetting = presenter.IsShareExitAutoSetting();
            characterShareRegisterView.UpdateShareExitAutoSetting(isShareExitAutoSetting);
        }

        private void UpdateEnableAutoSetting()
        {
            bool isEnableAutoSetting = presenter.IsCheckAutoSetting(isShowMessage: false);
            characterShareRegisterView.SetCanAutoSetting(isEnableAutoSetting);
        }

        private void UpdateFilterCount()
        {
            characterShareView.SetFilterCount(presenter.GetSharingFilterCount());
        }

        private void UpdateShareviceBuffInfo()
        {
            var buffInfos = presenter.GetShareviceBuffInfo();
            BuffItemInfo buffInfo = null; // 시간이 얼마남지 않은 버프
            foreach (var info in buffInfos)
            {
                if (buffInfo != null)
                {
                    if (buffInfo.RemainTime > info.RemainTime)
                    {
                        buffInfo = info;
                    }
                }
                else
                {
                    buffInfo = info;
                }
            }

            var level = presenter.GetShareviceLevel();
            int buffLevel = 0;
            foreach (var info in buffInfos)
            {
                buffLevel += info.GetBattleOptionTypeValues(BattleOptionType.ShareBoost)[0];
            }

            var resultBP = presenter.GetShareviceMaxBP(buffLevel);
            characterShareView.UpdateShareViceValue(buffInfo, level, buffLevel, resultBP, presenter.GetShareviceState(), presenter.HasViceExperienceItem());
        }

        protected override void OnBack()
        {
            if (shareviceLevelUpView.gameObject.activeSelf)
            {
                shareviceLevelUpView.Hide();
                return;
            }

            if (characterShareChargePopupView.IsShow)
            {
                characterShareChargePopupView.Hide();
                return;
            }

            if (characterShareListPopupView.IsShow)
            {
                characterShareListPopupView.Hide();
                return;
            }

            HideUI();
        }

        private void OnClickLevelUP()
        {
            shareviceLevelUpView.Show();
        }

        private void UpdateNotice()
        {
            btnToggle.SetNotice(presenter.HasNoticeShareForceStat());
        }

        #region Tutorial
        bool TutorialSharingCharacterEquip.ICharacterShareImpl.IsEmptySharingTime()
        {
            return presenter.IsFinishedSharingTime();
        }

        void TutorialSharingCharacterEquip.ICharacterShareImpl.RequestShareCharacterFreeDailyTimeTicketUse()
        {
            // 튜토리얼에서 자동 충전 사용
            if (!presenter.HasShareFreeTicket())
            {
                Debug.LogError("일일 무료 충전 티켓이 존재하지 않습니다");
                return;
            }

            presenter.RequestShareCharacterFreeDailyTimeTicketUse();
        }

        TutorialSharingCharacterEquip.ISingleEquipSharingCharacterImpl TutorialSharingCharacterEquip.ICharacterShareImpl.GetSingleEquipSharingCharacterImpl()
        {
            return characterShareListPopupView;
        }

        TutorialSharingCharacterEquip.IEquipSharingCharacterImpl TutorialSharingCharacterEquip.ICharacterShareImpl.GetEquipSharingCharacterImpl()
        {
            return characterShareView;
        }

        void TutorialSharingCharacterEquip.ICharacterShareImpl.HideCharacterShareUI()
        {
            HideUI();
        }
        #endregion
    }
}