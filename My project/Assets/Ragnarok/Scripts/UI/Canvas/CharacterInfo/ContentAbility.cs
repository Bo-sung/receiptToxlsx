using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterInfo"/> 내 정보 보기
    /// <see cref="UIOthersCharacterInfo"/> 다른 유저 정보 보기 
    /// </summary>
    public sealed class ContentAbility : UISubCanvas, ContentAbilityPresenter.IView, IInspectorFinder
    {
        [SerializeField] UIStatusElement[] statusElements;
        [SerializeField] UIButtonHelper btnStatInfo;
        [SerializeField] UIApplyStatBase applyStatBase;

        [SerializeField] UIToggleHelper toggleX10;

        [SerializeField] UILabelValue labelStatusPoint;
        [SerializeField] UIButtonHelper btnAutoStat;
        [SerializeField] UIButtonHelper btnResetStat;
        [SerializeField] UIButtonHelper btnStatSave;
        [SerializeField] UIButtonHelper btnStatCancel;
        [SerializeField] GameObject pointAnimation;

        [SerializeField] UIButtonHelper btnAutoOption;
        [SerializeField] UIToggleHelper toggleAuto;

        [SerializeField] UIButtonHelper showDetailButton;
        [SerializeField] AbilityPopupView abilityPopupView;

        /// <summary>
        /// 수정 가능 여부 (스탯포인트 분배 등)
        /// </summary>
        private bool isEditable;

        /******************** Tutorial ********************/
        [SerializeField] UIWidget statusArea;
        bool isClickedBtnStrLevelUp;

        ContentAbilityPresenter presenter;

        System.Action OnResetPreview;

        public ContentAbilityPresenter GetPresenter()
        {
            return presenter = presenter ?? new ContentAbilityPresenter(this);
        }

        protected override void OnInit()
        {
            presenter = presenter ?? new ContentAbilityPresenter(this);
            presenter.AddEvent();

            OnResetPreview += OnClickedBtnStatCancel;

            if (isEditable)
            {
                statusElements[0].OnPlus += OnClickedBtnStr;
                statusElements[1].OnPlus += OnClickedBtnAgi;
                statusElements[2].OnPlus += OnClickedBtnVit;
                statusElements[3].OnPlus += OnClickedBtnInt;
                statusElements[4].OnPlus += OnClickedBtnDex;
                statusElements[5].OnPlus += OnClickedBtnLuk;

                statusElements[0].OnMax += OnClickedBtnMaxStr;
                statusElements[1].OnMax += OnClickedBtnMaxAgi;
                statusElements[2].OnMax += OnClickedBtnMaxVit;
                statusElements[3].OnMax += OnClickedBtnMaxInt;
                statusElements[4].OnMax += OnClickedBtnMaxDex;
                statusElements[5].OnMax += OnClickedBtnMaxLuk;

                EventDelegate.Add(btnAutoStat.OnClick, OnClickedBtnAutoStat);
                EventDelegate.Add(btnResetStat.OnClick, OnClickedBtnResetStat);
                EventDelegate.Add(btnStatSave.OnClick, OnClickedBtnStatSave);
                EventDelegate.Add(btnStatCancel.OnClick, OnClickedBtnStatCancel);

                EventDelegate.Add(btnAutoOption.OnClick, OnClickedBtnAutoOption);
                EventDelegate.Add(toggleX10.OnChange, OnChangedtoggleX10);
            }
            EventDelegate.Add(btnStatInfo.OnClick, OnClickedBtnStatInfo);
            EventDelegate.Add(showDetailButton.OnClick, ShowAbilityPopupView);
        }

        protected override void OnClose()
        {
            OnResetPreview -= OnClickedBtnStatCancel;

            presenter.RemoveEvent();

            if (isEditable)
            {
                statusElements[0].OnPlus -= OnClickedBtnStr;
                statusElements[1].OnPlus -= OnClickedBtnAgi;
                statusElements[2].OnPlus -= OnClickedBtnVit;
                statusElements[3].OnPlus -= OnClickedBtnInt;
                statusElements[4].OnPlus -= OnClickedBtnDex;
                statusElements[5].OnPlus -= OnClickedBtnLuk;

                statusElements[0].OnMax -= OnClickedBtnMaxStr;
                statusElements[1].OnMax -= OnClickedBtnMaxAgi;
                statusElements[2].OnMax -= OnClickedBtnMaxVit;
                statusElements[3].OnMax -= OnClickedBtnMaxInt;
                statusElements[4].OnMax -= OnClickedBtnMaxDex;
                statusElements[5].OnMax -= OnClickedBtnMaxLuk;

                EventDelegate.Remove(btnAutoStat.OnClick, OnClickedBtnAutoStat);
                EventDelegate.Remove(btnResetStat.OnClick, OnClickedBtnResetStat);
                EventDelegate.Remove(btnStatSave.OnClick, OnClickedBtnStatSave);
                EventDelegate.Remove(btnStatCancel.OnClick, OnClickedBtnStatCancel);

                EventDelegate.Remove(btnAutoOption.OnClick, OnClickedBtnAutoOption);
                EventDelegate.Remove(toggleX10.OnChange, OnChangedtoggleX10);
            }

            EventDelegate.Remove(btnStatInfo.OnClick, OnClickedBtnStatInfo);
            EventDelegate.Remove(showDetailButton.OnClick, ShowAbilityPopupView);
        }

        protected override void OnShow()
        {
            if (presenter.GetPlayer() is null)
                return;

            Refresh();
        }

        public void Refresh()
        {
            presenter.ShowStatus();
            presenter.ShowStatusPoint();
            presenter.SetAutoStat();
            HideAbilityPopup();
            if (isEditable)
            {
                toggleX10.Set(LocalValue.IsStatPointTenFold, notify: false);
            }
        }

        protected override void OnHide()
        {
            presenter.ReleaseClonePlayer();
        }

        protected override void OnLocalize()
        {
            statusElements[0].Get().TitleKey = LocalizeKey._4004;
            statusElements[1].Get().TitleKey = LocalizeKey._4005;
            statusElements[2].Get().TitleKey = LocalizeKey._4006;
            statusElements[3].Get().TitleKey = LocalizeKey._4007;
            statusElements[4].Get().TitleKey = LocalizeKey._4008;
            statusElements[5].Get().TitleKey = LocalizeKey._4009;

            showDetailButton.LocalKey = LocalizeKey._4052; // 더 보기

            if (isEditable)
            {
                toggleX10.LocalKey = LocalizeKey._4020;

                btnAutoStat.LocalKey = LocalizeKey._4022;
                btnResetStat.LocalKey = LocalizeKey._4023;
                btnStatSave.LocalKey = LocalizeKey._4024;
                btnStatCancel.LocalKey = LocalizeKey._4025;

                toggleAuto.LocalKey = LocalizeKey._4022;
                labelStatusPoint.TitleKey = LocalizeKey._4021;
            }

        }

        /// <summary>
        /// 편집 가능 여부 설정 (OnInit 전에 실행되어야 한다.)
        /// </summary>
        public void Initialize(bool isEditable)
        {
            this.isEditable = isEditable;
        }

        /// <summary>
        /// 플레이어 정보 입력 (OnInit 이후에 실행되어야 한다.)
        /// </summary>
        public void SetPlayer(CharacterEntity charaEntity)
        {
            presenter.SetPlayer(charaEntity);
        }

        public void HideAbilityPopup()
        {
            abilityPopupView.Hide();
        }

        public bool IsActiveAbilityPopup()
        {
            return abilityPopupView.isActiveAndEnabled;
        }

        // 자동분배 버튼
        void OnClickedBtnAutoStat()
        {
            presenter.AutoStat();
        }

        // 능력치 초기화 버튼
        void OnClickedBtnResetStat()
        {
            presenter.RequestCharStatPointInit().WrapNetworkErrors();
        }

        // 스탯 적용 버튼
        void OnClickedBtnStatSave()
        {
            presenter.RequestStatPointUpdate();
        }

        // 스탯 적용 취소 버튼
        void OnClickedBtnStatCancel()
        {
            presenter.CancelPreviewStatPoint();
        }

        void OnClickedBtnStr()
        {
            presenter.AddStr(toggleX10.Value ? (short)10 : (short)1);
            isClickedBtnStrLevelUp = true;
        }

        void OnClickedBtnAgi()
        {
            presenter.AddAgi(toggleX10.Value ? (short)10 : (short)1);
        }

        void OnClickedBtnVit()
        {
            presenter.AddVit(toggleX10.Value ? (short)10 : (short)1);
        }

        void OnClickedBtnInt()
        {
            presenter.AddInt(toggleX10.Value ? (short)10 : (short)1);
        }

        void OnClickedBtnDex()
        {
            presenter.AddDex(toggleX10.Value ? (short)10 : (short)1);
        }

        void OnClickedBtnLuk()
        {
            presenter.AddLuk(toggleX10.Value ? (short)10 : (short)1);
        }

        void OnClickedBtnStatInfo()
        {
            UI.ConfirmPopupLong(LocalizeKey._2014.ToText()); // [ STR ]\n근접 물리 공격력에 영향을 줍니다.\n\n[ AGI ]\n공격 속도에 영향을 줍니다.\n\n[ VIT ]\nHP와 회복력에 영향을 줍니다.\n\n[ INT ]\n마법 공격력에 영향을 줍니다.\n\n[ DEX ]\n원거리 물리 공격력과 명중력에 영향을 줍니다.\n\n[ LUK ]\n치명타 관련 능력치에 영향을 줍니다.
        }

        void OnClickedBtnMaxStr()
        {
            ShowOverStatus(BasicStatusType.Str);
        }

        void OnClickedBtnMaxAgi()
        {
            ShowOverStatus(BasicStatusType.Agi);
        }

        void OnClickedBtnMaxVit()
        {
            ShowOverStatus(BasicStatusType.Vit);
        }

        void OnClickedBtnMaxInt()
        {
            ShowOverStatus(BasicStatusType.Int);
        }

        void OnClickedBtnMaxDex()
        {
            ShowOverStatus(BasicStatusType.Dex);
        }

        void OnClickedBtnMaxLuk()
        {
            ShowOverStatus(BasicStatusType.Luk);
        }

        void ShowOverStatus(BasicStatusType optionType)
        {
            if (presenter.IsPreview)
            {
                UI.ShowToastPopup(LocalizeKey._90267.ToText()); // STATUS POINT를 적용 후 사용할 수 있습니다.
                return;
            }

            int status = presenter.GetPlayer().Status.MaxStatusPlusCount(optionType);
            if (status >= BasisType.OVER_STATUS_MAX.GetInt())
            {
                UI.ShowToastPopup(LocalizeKey._90266.ToText()); // STATUS POINT가 최대치에 도달하였습니다.
                return;
            }

            if (presenter.RemainStatPoint == 0)
            {
                UI.ShowToastPopup(LocalizeKey._90264.ToText()); // STATUS POINT가 부족합니다.
                return;
            }

            // 오버스탯 추가
            CharacterEntity player;
            CharacterEntity preview;

            presenter.AddMaxStat(optionType, out player, out preview);

            BattleStatusInfo previewStatus = preview.battleStatusInfo;
            BattleStatusInfo previewWithNoBuffStatus = preview.onlyBaseBattleStatusInfo;
            BattleStatusInfo noBuffStatus = player.onlyBaseBattleStatusInfo;

            bool isMeleeAttack = player.battleSkillInfo.GetBasicActiveAttackType() == AttackType.MeleeAttack;
            UI.Show<UIOverStatus>().Set(optionType, isMeleeAttack, previewStatus, noBuffStatus, previewWithNoBuffStatus, OnResetPreview);
        }

        void ContentAbilityPresenter.IView.ShowStatus(CharacterEntity player, CharacterEntity preview)
        {
            BattleStatusInfo previewStatus = preview.battleStatusInfo;
            BattleStatusInfo previewWithNoBuffStatus = preview.onlyBaseBattleStatusInfo;
            BattleStatusInfo noBuffStatus = player.onlyBaseBattleStatusInfo;
            StatusModel status = player.Status;

            statusElements[0].Get().SetStatValue(previewStatus.Str, noBuffStatus.Str, previewWithNoBuffStatus.Str - noBuffStatus.Str);
            SetMaxStat(0, status.MaxStatus(BasicStatusType.Str) <= previewWithNoBuffStatus.Str, status.MaxStatusPlusCount(BasicStatusType.Str), previewWithNoBuffStatus.Str);

            statusElements[1].Get().SetStatValue(previewStatus.Agi, noBuffStatus.Agi, previewWithNoBuffStatus.Agi - noBuffStatus.Agi);
            SetMaxStat(1, status.MaxStatus(BasicStatusType.Agi) <= previewWithNoBuffStatus.Agi, status.MaxStatusPlusCount(BasicStatusType.Agi), previewWithNoBuffStatus.Agi);

            statusElements[2].Get().SetStatValue(previewStatus.Vit, noBuffStatus.Vit, previewWithNoBuffStatus.Vit - noBuffStatus.Vit);
            SetMaxStat(2, status.MaxStatus(BasicStatusType.Vit) <= previewWithNoBuffStatus.Vit, status.MaxStatusPlusCount(BasicStatusType.Vit), previewWithNoBuffStatus.Vit);

            statusElements[3].Get().SetStatValue(previewStatus.Int, noBuffStatus.Int, previewWithNoBuffStatus.Int - noBuffStatus.Int);
            SetMaxStat(3, status.MaxStatus(BasicStatusType.Int) <= previewWithNoBuffStatus.Int, status.MaxStatusPlusCount(BasicStatusType.Int), previewWithNoBuffStatus.Int);

            statusElements[4].Get().SetStatValue(previewStatus.Dex, noBuffStatus.Dex, previewWithNoBuffStatus.Dex - noBuffStatus.Dex);
            SetMaxStat(4, status.MaxStatus(BasicStatusType.Dex) <= previewWithNoBuffStatus.Dex, status.MaxStatusPlusCount(BasicStatusType.Dex), previewWithNoBuffStatus.Dex);

            statusElements[5].Get().SetStatValue(previewStatus.Luk, noBuffStatus.Luk, previewWithNoBuffStatus.Luk - noBuffStatus.Luk);
            SetMaxStat(5, status.MaxStatus(BasicStatusType.Luk) <= previewWithNoBuffStatus.Luk, status.MaxStatusPlusCount(BasicStatusType.Luk), previewWithNoBuffStatus.Luk);

            // 디테일 스탯 정보
            bool isMeleeAttack = player.battleSkillInfo.GetBasicActiveAttackType() == AttackType.MeleeAttack;
            applyStatBase.SetStatView(isMeleeAttack, previewStatus, noBuffStatus, previewWithNoBuffStatus);

            if (isEditable)
            {
                btnResetStat.IsEnabled = presenter.HaveUseStatPoint;
                btnStatSave.IsEnabled = presenter.UseStatPoint > 0;
                btnStatCancel.IsEnabled = presenter.UseStatPoint > 0;
            }
        }



        void SetMaxStat(int index, bool isMax, int overStatusCount, int curStatus)
        {
            if (isEditable)
            {
                statusElements[index].SetIsMax(isMax);

                // 오버스탯 가능한지 여부
                bool canOverStatus = presenter.RemainStatPoint > 0 && !presenter.IsPreview && overStatusCount < BasisType.OVER_STATUS_MAX.GetInt();
                statusElements[index].SetCanOverStatus(canOverStatus);

                // 스탯이 501 이상
                bool isOverStatus = curStatus > BasisType.MAX_STAT.GetInt();
                statusElements[index].SetIsOverStatus(isOverStatus);
            }
        }

        void SetBtnPlusEnabled(bool IsEnabled)
        {
            if (isEditable)
            {
                for (int i = 0; i < statusElements.Length; i++)
                {
                    statusElements[i].SetEnableBtnPlus(IsEnabled);
                }
            }
        }

        void ContentAbilityPresenter.IView.ShowStatusPoint(int point)
        {
            if (isEditable)
            {
                labelStatusPoint.Value = point.ToString();
                btnAutoStat.IsEnabled = point > 0;
                SetBtnPlusEnabled(point > 0);
                pointAnimation.SetActive(point > 0);
            }
        }

        UIWidget ContentAbilityPresenter.IView.GetStatusArea()
        {
            return statusArea;
        }

        UIButtonHelper ContentAbilityPresenter.IView.GetBtnStrUpgrade()
        {
            return statusElements[0].GetBtnPlus();
        }

        bool ContentAbilityPresenter.IView.IsClickedBtnStrLevelUp()
        {
            if (isClickedBtnStrLevelUp)
            {
                isClickedBtnStrLevelUp = false;
                return true;
            }

            return false;
        }

        UIButtonHelper ContentAbilityPresenter.IView.GetBtnApplyStatus()
        {
            return btnStatSave;
        }

        public void SetAutoStat(bool isAutoStat)
        {
            if (isEditable)
            {
                toggleAuto.Value = isAutoStat;
            }
        }

        void OnClickedBtnAutoOption()
        {
            presenter.RequestAutoStat();
        }

        private void ShowAbilityPopupView()
        {
            abilityPopupView.Show(presenter.GetPreview());
        }

        void OnChangedtoggleX10()
        {
            if (!toggleX10.IsCurrentToggle())
                return;

            LocalValue.IsStatPointTenFold = toggleX10.Value;
        }

        bool IInspectorFinder.Find()
        {
            statusElements = GetComponentsInChildren<UIStatusElement>();
            return true;
        }
    }
}