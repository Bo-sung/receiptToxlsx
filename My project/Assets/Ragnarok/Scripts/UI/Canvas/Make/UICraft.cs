using AnimationOrTween;
using System;
using UnityEngine;

namespace Ragnarok
{
    public class UICraft : MonoBehaviour, MakePresenter.IView
    {
        public static int curMainTab = 0;

        [SerializeField] UITabHelper tab;
        [SerializeField] UITabHelper subTab;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIMakeItemDetile makeItemDetile;
        [SerializeField] UILabelHelper labelNoSelect;
        [SerializeField] UILabelHelper LabelNoti;
        [SerializeField] UISlider progress;
        [SerializeField] UILabelHelper labelMaking;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnSkip;
        [SerializeField] Animator animator;

        [SerializeField] MakeSubTabConfig[] subTabInfos;

        private ActiveAnimation makeAnimation;
        private MakePresenter presenter;
        private MakeInfo[] arrayInfo;

        private int curSubTab = 0;
        private int? focusSubTab;
        private int? focusItemId;
        private int? focusMaterialId;

        public Action onTabChange;

        public void OnInit()
        {
            presenter = new MakePresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            // 메인탭
            EventDelegate.Add(tab[0].OnChange, OnClickedMainTab1);
            EventDelegate.Add(tab[1].OnChange, OnClickedMainTab2);
            EventDelegate.Add(tab[2].OnChange, OnClickedMainTab3);
            EventDelegate.Add(tab[3].OnChange, OnClickedMainTab4);
            EventDelegate.Add(tab[4].OnChange, OnClickedMainTab5);

            // 서브탭
            EventDelegate.Add(subTab[0].OnChange, OnClickedSubTab1);
            EventDelegate.Add(subTab[1].OnChange, OnClickedSubTab2);
            EventDelegate.Add(subTab[2].OnChange, OnClickedSubTab3);
            EventDelegate.Add(subTab[3].OnChange, OnClickedSubTab4);
            EventDelegate.Add(subTab[4].OnChange, OnClickedSubTab5);
            EventDelegate.Add(subTab[5].OnChange, OnClickedSubTab6);

            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnSkip.OnClick, OnClickBtnSkip);
        }

        public void OnClose()
        {
            presenter.RemoveEvent();

            // 메인탭
            EventDelegate.Remove(tab[0].OnChange, OnClickedMainTab1);
            EventDelegate.Remove(tab[1].OnChange, OnClickedMainTab2);
            EventDelegate.Remove(tab[2].OnChange, OnClickedMainTab3);
            EventDelegate.Remove(tab[3].OnChange, OnClickedMainTab4);
            EventDelegate.Remove(tab[4].OnChange, OnClickedMainTab5);

            // 서브탭
            EventDelegate.Remove(subTab[0].OnChange, OnClickedSubTab1);
            EventDelegate.Remove(subTab[1].OnChange, OnClickedSubTab2);
            EventDelegate.Remove(subTab[2].OnChange, OnClickedSubTab3);
            EventDelegate.Remove(subTab[3].OnChange, OnClickedSubTab4);
            EventDelegate.Remove(subTab[4].OnChange, OnClickedSubTab5);
            EventDelegate.Remove(subTab[5].OnChange, OnClickedSubTab6);

            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnSkip.OnClick, OnClickBtnSkip);
        }

        public void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_Make(); // 신규 컨텐츠 플래그 제거   

            // UI 바로가기 (특정 탭 또는 특정 아이템)
            if (data is UIMake.Input input)
            {
                FocusItemAsync(input.itemId, input.materialItemId);
                return;
            }

            if (tab[curMainTab].Value && subTab[curSubTab].Value)
            {
                Refresh();
            }
            else
            {
                ResetScrollProgress();
                SetMainTab(curMainTab, curSubTab);
            }
        }

        public void OnHide()
        {
            presenter.ResetMakeInfo();
        }

        public void OnLocalize()
        {
            tab[0].LocalKey = LocalizeKey._28019; // 아이템
            tab[1].LocalKey = LocalizeKey._28017; // 무기
            tab[2].LocalKey = LocalizeKey._28018; // 방어구
            tab[3].LocalKey = LocalizeKey._28026; // 특수
            tab[4].LocalKey = LocalizeKey._28066; // 교환
            LabelNoti.LocalKey = LocalizeKey._28023; // 제작 안내문
            labelMaking.LocalKey = LocalizeKey._28024; // 제작중...
            btnCancel.LocalKey = LocalizeKey._28025; // 취소
            btnSkip.LocalKey = LocalizeKey._28403; // 스킵
            labelNoSelect.LocalKey = LocalizeKey._28030; // 제작할 아이템을 선택해주세요.
        }

        public bool OnBack()
        {
            if (animator.gameObject.activeSelf)
            {
                OnClickedBtnCancel();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 특정 아이템 포커스
        /// </summary>
        void FocusItemAsync(int itemId, int materialId)
        {
            if (itemId == default)
                return;

            (int mainTabIndex, int subTabIndex) = presenter.GetMakeTab(itemId, materialId);
            if (mainTabIndex == -1)
                return;

            focusSubTab = subTabIndex;
            focusItemId = itemId;
            focusMaterialId = materialId;
            SetMainTab(mainTabIndex, subTabIndex);
        }

        void FocusItemSelect(int itemId, int materialId)
        {
            MakeInfo makeInfo = presenter.GetMakeInfo(itemId, materialId);
            presenter.SetSelectMakeInfo(makeInfo);
            CenterProgress(makeInfo);
        }

        private void CenterProgress(MakeInfo makeInfo)
        {
            if (arrayInfo.Length <= 15)
            {
                wrapper.SetProgress(0);
                return;
            }

            float size = 1f / (arrayInfo.Length / 5);
            int index = 0;
            for (int i = 0; i < arrayInfo.Length; i++)
            {
                if (arrayInfo[i].EnableType != EnableType.Enable)
                    continue;

                if (arrayInfo[i].ID == makeInfo.ID)
                {
                    index = i / 5;
                    break;
                }
            }
            wrapper.SetProgress(size * index);
        }

        /// <summary>
        /// 전체 갱신
        /// </summary>
        public void Refresh()
        {
            arrayInfo = presenter.GetMakeInfos(curMainTab, curSubTab);
            System.Array.Sort(arrayInfo, SortByCustom);
            wrapper.Resize(arrayInfo.Length);

            for (int i = 0; i < tab.Count; i++)
            {
                tab[i].SetNotice(presenter.IsMake(i));
            }
            for (int i = 0; i < 6; i++)
            {
                subTab[i].SetNotice(presenter.IsMake(curMainTab, i));
            }

            SetMakeDetailView();
        }

        /// <summary>
        /// 하단 아이템 상세 정보 갱신
        /// </summary>
        public void SetMakeDetailView()
        {
            makeItemDetile.SetData(presenter, presenter.Info);
            labelNoSelect.SetActive(presenter.Info == null);
        }

        void SetMainTab(int mainTabIndex, int subTabIndex = 0)
        {
            onTabChange?.Invoke();

            curMainTab = mainTabIndex;
            curSubTab = focusSubTab.HasValue ? focusSubTab.Value : subTabIndex;
            UIToggle.current = null;
            tab[curMainTab].Value = true;

            SetSubTabView(curMainTab);

            if (subTab[curSubTab].Value)
            {
                presenter.ResetMakeInfo();
                Refresh();
                return;
            }

            // 토글 2중으로 사용시 현재토글이 Null이 아니면 이벤트 호출이 안된다.
            UIToggle.current = null;
            subTab[curSubTab].Value = true;
        }

        void SetSubTab(int subTabIndex)
        {
            if (!subTab[subTabIndex].Value)
                return;

            curSubTab = subTabIndex;
            for (int i = 0; i < subTab.Count; i++)
            {
                subTab[i].SetLabelOutline(i == subTabIndex);
            }

            if (focusItemId.HasValue)
            {
                FocusItemSelect(focusItemId.Value, focusMaterialId.Value);
                focusItemId = null;
                focusSubTab = null;
                focusMaterialId = null;
            }
            else
            {
                presenter.SetMakeCount(1, isEvent: false);
                presenter.ResetMakeInfo();
                ResetScrollProgress();
                Refresh();
            }
        }

        void SetSubTabView(int mainTabIndex)
        {
            MakeSubTabConfig subTabConfig = subTabInfos[mainTabIndex];
            int length = subTabConfig == null ? 0 : subTabConfig.GetCount();

            if (mainTabIndex == 4)
            {
                if (!BasisOpenContetsType.Shadow.IsOpend())
                    length--;
            }

            subTab.SetValue(length);

            for (int i = 0; i < length; i++)
            {
                subTab[i].LocalKey = subTabConfig.GetSubTabNameId(i);
            }
        }

        private void ResetScrollProgress()
        {
            wrapper.SetProgress(0);
        }

        #region 메인 탭

        void OnClickedMainTab1()
        {
            if (!tab[0].Value)
                return;

            ResetScrollProgress();
            SetMainTab(0);
        }

        void OnClickedMainTab2()
        {
            if (!tab[1].Value)
                return;

            ResetScrollProgress();
            SetMainTab(1);
        }

        void OnClickedMainTab3()
        {
            if (!tab[2].Value)
                return;

            ResetScrollProgress();
            SetMainTab(2);
        }

        void OnClickedMainTab4()
        {
            if (!tab[3].Value)
                return;

            ResetScrollProgress();
            SetMainTab(3);
        }

        void OnClickedMainTab5()
        {
            if (!tab[4].Value)
                return;

            ResetScrollProgress();
            SetMainTab(4);
        }

        #endregion

        #region 서브 탭

        void OnClickedSubTab1()
        {
            SetSubTab(0);
        }

        void OnClickedSubTab2()
        {
            SetSubTab(1);
        }

        void OnClickedSubTab3()
        {
            SetSubTab(2);
        }

        void OnClickedSubTab4()
        {
            SetSubTab(3);
        }

        void OnClickedSubTab5()
        {
            SetSubTab(4);
        }

        void OnClickedSubTab6()
        {
            SetSubTab(5);
        }

        #endregion

        public void SetActiveMakeAnimation(bool isActive)
        {
            animator.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// 제작 연출
        /// </summary>
        public void PlayMakeAnimation()
        {
            makeAnimation = ActiveAnimation.Play(animator, "UI_ForgeAnvil", Direction.Forward, EnableCondition.EnableThenPlay, DisableCondition.DoNotDisable);
            EventDelegate.Add(makeAnimation.onFinished, OnFinishedMakeAnimation, oneShot: true);
        }

        /// <summary>
        /// 제작 연출 완료 이벤트
        /// </summary>
        void OnFinishedMakeAnimation()
        {
            presenter.RequestMakeItem();
        }

        /// <summary>
        /// 제작 취소
        /// </summary>
        void OnClickedBtnCancel()
        {
            EventDelegate.Remove(makeAnimation.onFinished, OnFinishedMakeAnimation);
            makeAnimation.Finish();
            SetActiveMakeAnimation(false);
        }

        void OnClickBtnSkip()
        {
            EventDelegate.Remove(makeAnimation.onFinished, OnFinishedMakeAnimation);
            makeAnimation.Finish();
            SetActiveMakeAnimation(false);
            OnFinishedMakeAnimation();
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIMakeItemSlot ui = go.GetComponent<UIMakeItemSlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        private int SortByCustom(MakeInfo x, MakeInfo y)
        {
            //int result0 = y.IsMake.CompareTo(x.IsMake);
            //int result1 = result0 == 0 ? x.Sort.CompareTo(y.Sort) : result0;
            //return result1;
            return x.Sort.CompareTo(y.Sort);
        }

        void Update()
        {
            // 제작 연출중 프로그레스바 진행도 표시
            if (makeAnimation != null && makeAnimation.isPlaying)
            {
                progress.value = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
        }
    }
}
