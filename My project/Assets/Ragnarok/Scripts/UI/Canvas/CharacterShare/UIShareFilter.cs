using Ragnarok.View.CharacterShare;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIShareFilter : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnReset;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelSelectDescription;
        [SerializeField] UILabelHelper labelDescription; // 이거 기본표시
        [SerializeField] UILabelHelper labelNotice;

        [SerializeField] UIGridHelper selectGrid;
        [SerializeField] UIGrid defaultGrid;
        [SerializeField] ShareFilterIcon[] defaultIcons;
        [SerializeField] ShareFilterSelectIcon[] selectIcons;

        ShareFilterPresenter presenter;
        GameObject goDefaultGrid;

        JobFilter[] curShareJobFilterAry;
        int selectSlotIdx = -1;

        protected override void OnInit()
        {
            goDefaultGrid = defaultGrid.gameObject;
            curShareJobFilterAry = new JobFilter[Constants.Size.SHARE_SLOT_SIZE];
            presenter = new ShareFilterPresenter();

            InitIconSlots();
            AddEvent();
        }

        protected override void OnClose()
        {
            RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            // 필터목록 초기화(SelectedSlot) 후에 갱신(UpdateSelectIconSlots) 해줘야 함.
            SelectedSlot(false);

            UpdateJobFilterRopoToUI(); // 서버 값으로 현재값 갱신
            UpdateSelectIconSlots(); // 필터중인 슬롯 갱신

            selectGrid.SetValue(presenter.GetOpenedShareSlotCount());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._10248; // 장착 필터
            labelSelectDescription.LocalKey = LocalizeKey._10250; // 매칭을 원하는 직업군을 고르세요.
            labelDescription.LocalKey = LocalizeKey._10251; // 자동 장착 필터를 사용해 보세요.
            labelNotice.LocalKey = LocalizeKey._10252; // 선택한 직업 계열에서 높은 차수를 우선적으로 장착합니다.

            btnConfirm.LocalKey = LocalizeKey._10253; // 확인
            btnReset.LocalKey = LocalizeKey._10249; // Full\nOff
        }

        private void AddEvent()
        {
            presenter.OnHideShareFilterUI += HideUI;

            EventDelegate.Add(btnExit.OnClick, HideUI);
            EventDelegate.Add(btnConfirm.OnClick, OnClickBtnConfirm);
            EventDelegate.Add(btnReset.OnClick, OnClickBtnRemove);

            for (int i = 0; i < defaultIcons.Length; i++)
            {
                defaultIcons[i].OnClickJobFilter += OnClickJobFilter;
            }

            for (int i = 0; i < selectIcons.Length; i++)
            {
                selectIcons[i].OnClickBtnAddSlot += OnClickAddSlot;
                selectIcons[i].OnClickBtnRemoveSlot += OnClickRemoveSlot;
            }
        }

        private void RemoveEvent()
        {
            presenter.OnHideShareFilterUI -= HideUI;

            EventDelegate.Remove(btnExit.OnClick, HideUI);
            EventDelegate.Remove(btnConfirm.OnClick, HideUI);
            EventDelegate.Remove(btnReset.OnClick, OnClickBtnRemove);

            for (int i = 0; i < defaultIcons.Length; i++)
            {
                defaultIcons[i].OnClickJobFilter -= OnClickJobFilter;
            }

            for (int i = 0; i < selectIcons.Length; i++)
            {
                selectIcons[i].OnClickBtnAddSlot -= OnClickAddSlot;
                selectIcons[i].OnClickBtnRemoveSlot -= OnClickRemoveSlot;
            }
        }

        private void InitIconSlots()
        {
            for (int i = 0; i < defaultIcons.Length; i++)
            {
                var job = GetJob((JobFilter)(1 << i));
                defaultIcons[i].SetData(job);
            }
        }

        private void UpdateSelectIconSlots()
        {
            for (int i = 0; i < selectIcons.Length; i++)
            {
                var job = GetJob(curShareJobFilterAry[i]);
                selectIcons[i].UpdateData(i, job);
            }
        }

        private void UpdateJobFilterRopoToUI()
        {
            curShareJobFilterAry = (JobFilter[])presenter.GetShareJobFilterAry().Clone();
        }

        private void SelectedSlot(bool activeFilter, int selectIdx = -1)
        {
            goDefaultGrid.SetActive(activeFilter);
            labelDescription.SetActive(!activeFilter);
            selectSlotIdx = selectIdx;

            // 여기에 선택 아이콘 ON/OFF처리
            if (activeFilter)
            {
                for (int i = 0; i < selectIcons.Length; i++)
                {
                    selectIcons[i].ActiveSelectIcon(i == selectSlotIdx);
                }
            }
            else
            {
                if (selectSlotIdx < 0) // 전체슬롯 초기화
                {
                    for (int i = 0; i < selectIcons.Length; i++)
                    {
                        curShareJobFilterAry[i] = 0;
                        selectIcons[i].UpdateData(i, default);
                    }
                }
                else // 해당 슬롯만 초기화
                {
                    curShareJobFilterAry[selectSlotIdx] = 0;
                    selectIcons[selectSlotIdx].UpdateData(selectSlotIdx, default);
                }

                for (int i = 0; i < selectIcons.Length; i++)
                {
                    selectIcons[i].ActiveSelectIcon(false);
                }
            }
        }

        private void HideUI()
        {
            UI.Close<UIShareFilter>();
        }

        private void OnClickJobFilter(Job job)
        {
            if (selectSlotIdx < 0) throw new System.InvalidOperationException("Selected Share Job Filter Slot Index Error");

            curShareJobFilterAry[selectSlotIdx] = job.ToJobFilter();
            selectIcons[selectSlotIdx].UpdateData(selectSlotIdx, job);
        }

        private void OnClickAddSlot(int idx)
        {
            // 필터 추가
            SelectedSlot(true, idx);
        }

        private void OnClickRemoveSlot(int idx)
        {
            // 필터 제거
            SelectedSlot(false, idx);
        }

        private void OnClickBtnRemove()
        {
            SelectedSlot(false);
        }

        private void OnClickBtnConfirm()
        {
            presenter.UpdateShareFilter(curShareJobFilterAry);
        }

        private Job GetJob(JobFilter filter)
        {
            return presenter.GetJob(filter);
        }

        public override bool Find()
        {
            base.Find();

            if (defaultGrid) defaultIcons = defaultGrid.GetComponentsInChildren<ShareFilterIcon>();
            if (selectGrid) selectIcons = selectGrid.GetComponentsInChildren<ShareFilterSelectIcon>();
            return true;
        }
    }
}