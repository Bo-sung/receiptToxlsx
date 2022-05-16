using System.Globalization;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="BattlePlayerStatusPresenter"/>
    /// 플레이어의 상태를 보여주는 UI. 여러 모드가 있다.
    /// </summary>
    public sealed class UIBattlePlayerStatus : UICanvas, TutorialSkillLearn.IOpenSkillImpl
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        public enum Position
        {
            TopLeft = 0,    // 왼쪽 위
            BottomLeft,     // 왼쪽 아래
            TopLeft_Big,    // [임시] 왼쪽 위 1.25스케일
        }

        // 포지션
        [SerializeField] UIWidget container;
        [SerializeField] GameObject container_TopLeft;
        [SerializeField] GameObject container_BottomLeft;
        [SerializeField] GameObject container_TopLeft_Big;

        // 썸네일
        [SerializeField] UIButtonWithIcon btnThumbnail;
        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] GameObject fxOutlineThumbnail;

        // 정보
        [SerializeField] UILabelHelper labelAttackPower; // 전투력: xxxx
        [SerializeField] UISprite iconHp; // 체력 Fill
        [SerializeField] UILabelHelper labelHp; // 체력 라벨 n%
        [SerializeField] UISprite iconMp; // 마나 Fill
        [SerializeField] UILabelHelper labelMp; // 마나 라벨 n%
        [SerializeField] SkillSlotPreview[] skillSlotPreviews; // 스킬 슬롯 x4

        // 사이드메뉴 - 조각 (큐브조각, 제니, 경험치, 이속포션)
        [SerializeField] UIGrid grid;
        [SerializeField] UIExtraBattlePlayerStatus[] extras;

        [SerializeField] UIButton btnSkill;
        [SerializeField] UIButton btnPower;
        [SerializeField] GameObject goModify;

        private BattlePlayerStatusPresenter presenter;
        private NumberFormatInfo percentageFormat;

        protected override void OnInit()
        {
            presenter = new BattlePlayerStatusPresenter();
            presenter.AddEvent();

            presenter.OnChangeHP += OnChangeHP;
            presenter.OnChangeMP += OnChangeMP;
            presenter.OnUpdateSkill += OnUpdateSkill;
            presenter.OnChangeAutoSkill += OnChangeAutoSkill;
            presenter.OnChangedJob += OnChangedJob;
            presenter.OnChangedGender += Refresh;
            presenter.OnUpdateStatPoint += OnUpdateStatPoint;
            presenter.OnChangeAP += OnChangeAP;
            presenter.OnUpdateProfile += RefreshThumbnail;
            presenter.OnUpdateBattleMode += RefreshBattleMode;

            if (percentageFormat is null)
                percentageFormat = new NumberFormatInfo { PercentPositivePattern = 1, PercentNegativePattern = 1 };

            EventDelegate.Add(btnThumbnail.OnClick, OnClickedBtnThumbnail);
            EventDelegate.Add(btnSkill.onClick, OnClickedBtnSkill);
            EventDelegate.Add(btnPower.onClick, OnClickedBtnPower);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnChangeHP -= OnChangeHP;
            presenter.OnChangeMP -= OnChangeMP;
            presenter.OnUpdateSkill -= OnUpdateSkill;
            presenter.OnChangeAutoSkill -= OnChangeAutoSkill;
            presenter.OnChangedJob -= OnChangedJob;
            presenter.OnChangedGender -= Refresh;
            presenter.OnUpdateStatPoint -= OnUpdateStatPoint;
            presenter.OnChangeAP -= OnChangeAP;
            presenter.OnUpdateProfile -= RefreshThumbnail;
            presenter.OnUpdateBattleMode -= RefreshBattleMode;

            EventDelegate.Remove(btnThumbnail.OnClick, OnClickedBtnThumbnail);
            EventDelegate.Remove(btnSkill.onClick, OnClickedBtnSkill);
            EventDelegate.Remove(btnPower.onClick, OnClickedBtnPower);
        }

        protected override void OnShow(IUIData data = null)
        {
            SetExtraMode(null);
            RefreshBattleMode();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            RefreshAP();
        }

        /// <summary>
        /// 플레이어 설정
        /// </summary>
        public void SetPlayer(CharacterEntity player)
        {
            presenter.SetEntity(player);
            Refresh();
        }

        /// <summary>
        /// 포지션 설정
        /// </summary>
        public void SetPosition(Position position)
        {
            switch (position)
            {
                case Position.TopLeft:
                    container.SetAnchor(container_TopLeft, 0, 0, 0, 0);
                    break;

                case Position.BottomLeft:
                    container.SetAnchor(container_BottomLeft, 0, 0, 0, 0);
                    break;

                case Position.TopLeft_Big:
                    container.SetAnchor(container_TopLeft_Big, 0, 0, 0, 0);
                    break;
            }
        }

        /// <summary>
        /// 사이드메뉴 모드 설정
        /// </summary>
        public void SetExtraMode(params UIExtraBattlePlayerStatus.ExtraMode[] modes)
        {
            int length = modes == null ? 0 : modes.Length;
            for (int i = 0; i < extras.Length; i++)
            {
                extras[i].SetMode(i < length ? modes[i] : UIExtraBattlePlayerStatus.ExtraMode.None);
            }
            grid.Reposition();
        }

        /// <summary>
        /// Extra 개수
        /// </summary>
        public void SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode mode, int current, int max)
        {
            UIExtraBattlePlayerStatus ui = Find(mode);
            if (ui == null)
                return;

            ui.SetCount(current, max);
        }

        /// <summary>
        /// Extra 개수
        /// </summary>
        public void SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode mode, int count)
        {
            UIExtraBattlePlayerStatus ui = Find(mode);
            if (ui == null)
                return;

            ui.SetCount(count);
        }

        private void Refresh()
        {
            if (!presenter.HasEntity())
                return;

            // 썸네일
            RefreshThumbnail(); // 썸네일
            RefreshThumbnailNotice(); // 노티스

            // 전투력 업데이트
            RefreshAP();

            // 스킬 업데이트
            OnUpdateSkill();

            // HP, MP 업데이트
            OnChangeHP(presenter.GetCurHP(), presenter.GetMaxHP());
            OnChangeMP(presenter.GetCurMP(), presenter.GetMaxMP());
        }

        /// <summary>
        /// HP 업데이트
        /// </summary>
        private void OnChangeHP(int current, int max)
        {
            float progress = (float)current / max;

            // 모드 - 정보의 체력
            iconHp.fillAmount = progress;
            labelHp.Text = progress.ToString("P0", percentageFormat);
        }

        /// <summary>
        /// MP 업데이트
        /// </summary>
        private void OnChangeMP(int current, int max)
        {
            float progress = (float)current / max;

            // 모드 - 정보의 체력
            iconMp.fillAmount = progress;
            labelMp.Text = progress.ToString("P0", percentageFormat);
        }

        /// <summary>
        /// 스킬 슬롯 업데이트
        /// </summary>
        private void OnUpdateSkill()
        {
            var skillSlostInfos = presenter.GetSkillSlotInfos();

            int maxSkillSlotCount = BasisType.MAX_CHAR_SKILL_SLOT.GetInt(); // 총 스킬 슬롯 개수 (4)
            for (int i = 0; i < maxSkillSlotCount; ++i)
            {
                skillSlotPreviews[i].Initialize(skillSlostInfos[i].skillInfo, skillSlostInfos[i].isLockedSlot);
            }
        }

        private void OnChangeAutoSkill()
        {
            for (int i = 0; i < skillSlotPreviews.Length; ++i)
            {
                skillSlotPreviews[i].Refresh();
            }
        }

        /// <summary>
        /// 전투력 업데이트 
        /// </summary>
        private void OnChangeAP(int AP)
        {
            labelAttackPower.Text = LocalizeKey._48100.ToText() // 전투력 : {VALUE}
                .Replace(ReplaceKey.VALUE, AP);
        }

        /// <summary>
        /// 전직 이벤트
        /// </summary>
        private void OnChangedJob(bool isInit)
        {
            Refresh();

            if (!isInit)
            {
                fxOutlineThumbnail.SetActive(presenter.IsCostumeOpen());
            }
        }

        /// <summary>
        /// 전투력 업데이트
        /// </summary>
        private void RefreshAP()
        {
            // 전투력 업데이트
            int curAP = presenter.GetAP();
            OnChangeAP(curAP);
        }

        /// <summary>
        /// 스탯포인트 업데이트 -> 썸네일 Notice 설정
        /// </summary>
        private void OnUpdateStatPoint()
        {
            RefreshThumbnailNotice();
        }

        /// <summary>
        /// 썸네일 노티스 업데이트
        /// </summary>
        private void RefreshThumbnailNotice()
        {
            bool isNotice = presenter.IsStatusNotice();
            btnThumbnail.SetNotice(isNotice);
        }

        /// <summary>
        /// 썸네일 업데이트
        /// </summary>
        private void RefreshThumbnail()
        {
            thumbnail.Set(presenter.GetThumbnailIconName()); // 썸네일
        }

        /// <summary>
        /// 스킬 버튼
        /// </summary>
        private void OnClickedBtnSkill()
        {
            if (presenter.IsDungeon())
                return;

            if (!Entity.player.Quest.IsOpenContent(ContentType.Skill, isShowPopup: true))
                return;

            UI.Show<UISkill>();
        }

        /// <summary>
        /// 전투력 버튼
        /// </summary>
        private void OnClickedBtnPower()
        {
            // 랭킹 전투력 탭으로 이동
            UI.Show<UIRank>().SetTab(tabIndex: 2);
        }

        /// <summary>
        /// 영웅(썸네일) 버튼
        /// </summary>
        private void OnClickedBtnThumbnail()
        {
            if (presenter.IsDungeon())
                return;

            fxOutlineThumbnail.SetActive(false);
            UI.Show<UIProfileSelect>();
        }

        public UIWidget GetWidget(UIExtraBattlePlayerStatus.ExtraMode extraMode)
        {
            UIExtraBattlePlayerStatus ui = Find(extraMode);
            if (ui == null)
                return null;

            return ui.GetWidget();
        }

        /// <summary>
        /// MP 획득 연출 위치
        /// </summary>
        public UIWidget GetMPTarget()
        {
            return iconMp;
        }

        /// <summary>
        /// Mode 에 해당하는 UI 반환
        /// </summary>
        private UIExtraBattlePlayerStatus Find(UIExtraBattlePlayerStatus.ExtraMode mode)
        {
            for (int i = 0; i < extras.Length; i++)
            {
                if (extras[i].Mode == mode)
                    return extras[i];
            }

            Debug.Log("Mode 해당되는 UI가 음슴: mode = " + mode);

            return null;
        }

        private void RefreshBattleMode()
        {
            NGUITools.SetActive(goModify, !presenter.IsDungeon());
        }

        public override bool Find()
        {
            base.Find();

            extras = GetComponentsInChildren<UIExtraBattlePlayerStatus>();
            return true;
        }

        #region Tutorial
        UIWidget TutorialSkillLearn.IOpenSkillImpl.GetBtnQuickSkill()
        {
            return btnSkill.GetComponent<UIWidget>();
        }
        #endregion
    }
}