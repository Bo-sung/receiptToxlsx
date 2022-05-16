using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class CharacterShareListPopupView : UIView, TutorialSharingCharacterEquip.ISingleEquipSharingCharacterImpl
    {
        private const int PLAYER = 0;
        private const int GUILD = 1;
        private const int FRIEND = 2;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UITabHelper tab;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UIButtonHelper btnClose, btnSelect;
        [SerializeField] UIButton btnTempLock; // 길드 캐릭터 탭 잠금

        private UISimpleCharacterShareBar.IInput[] arrData;
        private UISimpleCharacterShareBar.IInput[] arrCloneData;
        private UICharacterShareSelectBar selected;
        private UISimpleCharacterShareBar.IInput curSelected;

        public event System.Action OnShowNextPage;
        public event UICharacterShare.SelectShareCharacterEvent OnSelect;
        public event UICharacterShare.SelectCloneCharacterEvent OnSelectClone;
        public event System.Action OnSelectTabPlayer;
        public event System.Action OnSelectTabGuild;

        private UICharacterShareSelectBar firstBar;

        /// <summary>
        /// 내 캐릭터 클론 슬롯
        /// </summary>
        private bool isClone = false;

        /// <summary>
        /// 길드 가입 여부
        /// </summary>
        private bool haveGuild;

        /// <summary>
        /// 길드 기여도 점수
        /// </summary>
        private int guildDonationPoint;

        protected override void Awake()
        {
            base.Awake();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SetClickCallback(OnItemClick);
            wrapper.ScrollView.onDragFinished = OnDragFinished;
            wrapper.SpawnNewList(prefab, 0, 0);

            tab.OnSelect += OnSelectTab;
            EventDelegate.Add(btnClose.OnClick, OnClickedBtnClose);
            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
            EventDelegate.Add(btnTempLock.onClick, OnClickedBtnTempLock);

            ResetData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            tab.OnSelect -= OnSelectTab;
            EventDelegate.Remove(btnClose.OnClick, OnClickedBtnClose);
            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
            EventDelegate.Remove(btnTempLock.onClick, OnClickedBtnTempLock);
        }

        public override void Hide()
        {
            base.Hide();

            tab.ResetToggle();
            ClearSelectedBar();
            wrapper.SetProgress(0);
        }

        public override void Show()
        {
            base.Show();

            tab.Value = PLAYER;
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._10210; // 셰어 리스트
            tab[PLAYER].LocalKey = LocalizeKey._10211; // 모험가 캐릭터
            tab[GUILD].LocalKey = LocalizeKey._10212; // 길드 캐릭터
            tab[FRIEND].LocalKey = LocalizeKey._10241; // 친구 캐릭터
            labelNoData.LocalKey = LocalizeKey._10213; // 현재 지역에서 이용 가능한 쉐어 캐릭터가 없습니다.
            btnClose.LocalKey = LocalizeKey._10208; // 닫기
            btnSelect.LocalKey = LocalizeKey._10214; // 캐릭터 고용
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UICharacterShareSelectBar ui = go.GetComponent<UICharacterShareSelectBar>();

            if (index == 0)
                firstBar = ui;

            ui.SetData(arrData[index]);

            if (arrData[index] == curSelected)
                ui.Select();
            else
                ui.Unselect();
        }

        void OnCloneItemRefresh(GameObject go, int index)
        {
            UICharacterShareSelectBar ui = go.GetComponent<UICharacterShareSelectBar>();

            if (index == 0)
                firstBar = ui;

            ui.SetData(arrCloneData[index]);

            if (arrCloneData[index] == curSelected)
                ui.Select();
            else
                ui.Unselect();
        }

        void OnItemClick(GameObject go, int index)
        {
            ClearSelectedBar();

            selected = go.GetComponent<UICharacterShareSelectBar>();
            selected.Select();
            curSelected = selected.data;
            btnSelect.IsEnabled = true;
        }

        void OnDragFinished()
        {
            if (tab.GetSelectIndex() != PLAYER)
                return;

            // UIScrollView 408줄 참고
            Bounds b = wrapper.ScrollView.bounds;
            Vector3 constraint = wrapper.Panel.CalculateConstrainOffset(b.min, b.max);
            constraint.x = 0f;

            // 최하단에서 드래그 끝났을 때
            if (constraint.sqrMagnitude > 0.1f && constraint.y < 0f)
                OnShowNextPage?.Invoke();
        }

        void OnSelectTab(int index)
        {
            if (isClone)
                return;

            switch (index)
            {
                case PLAYER:
                    OnSelectTabPlayer?.Invoke();
                    break;

                case GUILD:
                    OnSelectTabGuild?.Invoke();
                    break;

                case FRIEND:
                    SetDataSize(0);
                    break;
            }
        }

        void OnClickedBtnClose()
        {
            Hide();
        }

        void OnClickedBtnSelect()
        {
            if (selected == null)
                return;

            UISimpleCharacterShareBar.IInput data = selected.data;

            if (isClone)
            {
                OnSelectClone?.Invoke(data.Cid, data.Uid, data.CloneCharacterType);
            }
            else
            {
                OnSelect?.Invoke(data.Cid, data.Uid, data.SharingCharacterType);
            }

            Hide();
        }

        void OnClickedBtnTempLock()
        {
            UI.ShowToastPopup(LocalizeKey._90045.ToText()); // 업데이트 예정입니다.
        }

        public void ResetData()
        {
            SetData(null);
            ClearSelectedBar();
            wrapper.SetProgress(0);
        }

        public void SetIsClone(bool isClone)
        {
            this.isClone = isClone;
        }

        public void SetGuildData(bool haveGuild, int guildDonationPoint)
        {
            Debug.Log($"길드 가입 여부={haveGuild}, 길드 기여도={guildDonationPoint}");
            this.haveGuild = haveGuild;
            this.guildDonationPoint = guildDonationPoint;
        }

        public void SetData(UISimpleCharacterShareBar.IInput[] inputs)
        {
            tab.SetActive(true);
            labelMainTitle.LocalKey = LocalizeKey._10210; // 셰어 리스트

            int index = tab.GetSelectIndex();

            switch (index)
            {
                default:
                case PLAYER:

                    labelNoData.LocalKey = LocalizeKey._10213; // 현재 지역에서 이용 가능한 쉐어 캐릭터가 없습니다.

                    break;

                case GUILD:

                    if (!haveGuild)
                    {
                        labelNoData.LocalKey = LocalizeKey._90068; // 가입되어 있는 길드가 없습니다.
                    }
                    else if (guildDonationPoint < BasisType.GUILD_SHARE_NEED_DONATION.GetInt())
                    {
                        // 길드 기여도 {COUNT} 필요합니다.\n 현재 기여도 {VALUE}
                        labelNoData.Text = LocalizeKey._10280.ToText()
                            .Replace(ReplaceKey.COUNT, BasisType.GUILD_SHARE_NEED_DONATION.GetInt())
                            .Replace(ReplaceKey.VALUE, guildDonationPoint);
                    }
                    else
                    {
                        labelNoData.LocalKey = LocalizeKey._10213; // 현재 지역에서 이용 가능한 쉐어 캐릭터가 없습니다.
                    }

                    break;
            }

            wrapper.SetRefreshCallback(OnItemRefresh);

            arrData = inputs;
            SetDataSize(arrData == null ? 0 : arrData.Length);
        }

        public void SetCloneData(UISimpleCharacterShareBar.IInput[] inputs)
        {
            tab.SetActive(false);
            labelMainTitle.LocalKey = LocalizeKey._10242; // 내 캐릭터
            labelNoData.LocalKey = LocalizeKey._10266; // 클론을 할 수 있는 내 캐릭터가 없습니다.

            wrapper.SetRefreshCallback(OnCloneItemRefresh);

            arrCloneData = inputs;
            SetDataSize(arrCloneData == null ? 0 : arrCloneData.Length);
        }

        private void SetDataSize(int size)
        {
            wrapper.Resize(size);
            labelNoData.SetActive(size == 0);
        }

        private void ClearSelectedBar()
        {
            btnSelect.IsEnabled = false;

            if (selected == null)
                return;

            selected.Unselect();
            selected = null;
            curSelected = null;
        }

        #region Tutorial
        void TutorialSharingCharacterEquip.ISingleEquipSharingCharacterImpl.SetTutorialMode(bool isTutorialMode)
        {
            wrapper.ScrollView.enabled = !isTutorialMode;
        }

        bool TutorialSharingCharacterEquip.ISingleEquipSharingCharacterImpl.IsShown()
        {
            return IsShow && arrData != null && arrData.Length != 0;
        }

        UIWidget TutorialSharingCharacterEquip.ISingleEquipSharingCharacterImpl.GetFirstBar()
        {
            if (firstBar == null)
                return null;

            return firstBar.GetComponent<UIWidget>();
        }

        bool TutorialSharingCharacterEquip.ISingleEquipSharingCharacterImpl.IsSelectedFirstBar()
        {
            return selected != null;
        }

        UIWidget TutorialSharingCharacterEquip.ISingleEquipSharingCharacterImpl.GetBtnSelect()
        {
            return btnSelect.GetComponent<UIWidget>();
        }
        #endregion
    }
}