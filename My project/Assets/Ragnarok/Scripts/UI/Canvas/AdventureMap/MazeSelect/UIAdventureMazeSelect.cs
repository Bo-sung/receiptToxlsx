using Ragnarok.View;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIAdventureMazeSelect : UICanvas, TutorialMazeEnter.IImpl
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] AdventureMazeSelectView adventureMazeSelectView;
        [SerializeField] UIButtonHelper btnMap;

        AdventureMazeSelectPresenter presenter;

        private int curAdventureGroup;

        protected override void OnInit()
        {
            presenter = new AdventureMazeSelectPresenter();

            adventureMazeSelectView.OnSelect += SelectElement;
            presenter.OnUpdateZeny += titleView.ShowZeny;
            presenter.OnUpdateCatCoin += titleView.ShowCatCoin;

            EventDelegate.Add(btnMap.OnClick, OnClickedBtnMap);

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            adventureMazeSelectView.OnSelect -= SelectElement;
            presenter.OnUpdateZeny -= titleView.ShowZeny;
            presenter.OnUpdateCatCoin -= titleView.ShowCatCoin;

            EventDelegate.Remove(btnMap.OnClick, OnClickedBtnMap);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_Maze(); // 신규 컨텐츠 플래그 제거

            int adventureGroup = presenter.GetAdventureGroup();
            SelectAdventureGroup(adventureGroup);
        }

        protected override void OnHide()
        {
            curAdventureGroup = 0;
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._48201.ToText()); // 미궁섬
            btnMap.LocalKey = LocalizeKey._48282; // 지도
        }

        void OnClickedBtnMap()
        {
            AsyncShowSelectWorldPopup().WrapUIErrors();
        }

        public void SelectId(int multiMazeWaitingRoomId)
        {
            int group = presenter.GetAdventureGroupByMultiMazeWaitingRoom(multiMazeWaitingRoomId);
            SelectAdventureGroup(group);

            // 선택한 id로 이동 처리
            int index = presenter.FindIndex(group, multiMazeWaitingRoomId);
            adventureMazeSelectView.MoveTo(index);
        }

        public void ShowTimePatrol()
        {
            SelectId(MultiMazeWaitingRoomData.TIME_PATROL);
            SelectElement(MultiMazeWaitingRoomData.TIME_PATROL);
        }

        private void SelectAdventureGroup(int adventureGroup)
        {
            if (curAdventureGroup == adventureGroup)
                return;

            curAdventureGroup = adventureGroup;
            adventureMazeSelectView.SetData(presenter.GetData(curAdventureGroup));
        }

        private void SelectElement(int id)
        {
            isSelectFirstMazeEnter = true;
            presenter.Select(id);
        }

        /// <summary>
        /// 월드 선택 팝업
        /// </summary>
        private async Task AsyncShowSelectWorldPopup()
        {
            int groupId = await UI.Show<UIAdventureGroupSelect>().AsyncShow();

            // group 선택하지 않음
            if (groupId <= 0)
                return;

            // group이 바뀌지 않음
            if (groupId == curAdventureGroup)
                return;

            adventureMazeSelectView.InitChapterProgress(); // Chapter Progress 초기화
            SelectAdventureGroup(groupId);
        }

        #region Tutorial
        bool isSelectFirstMazeEnter;

        void TutorialMazeEnter.IImpl.SetTutorialMode(bool isTutorialMode)
        {
            adventureMazeSelectView.SetScrollViewEnable(!isTutorialMode);
        }

        UIWidget TutorialMazeEnter.IImpl.GetFirstMazeWidget()
        {
            // id가 1인 Element 반환 (첫번째 튜토리얼에 해당하는 미궁섬 Element)
            return adventureMazeSelectView.GetElementWidget(1);
        }

        bool TutorialMazeEnter.IImpl.IsSelectFirstMazeEnter()
        {
            if (isSelectFirstMazeEnter)
            {
                isSelectFirstMazeEnter = false;
                return true;
            }

            return false;
        }

        UIWidget TutorialMazeEnter.IImpl.GetTimePatrolWidget()
        {
            return adventureMazeSelectView.GetElementWidget(MultiMazeWaitingRoomData.TIME_PATROL);
        }

        UIWidget TutorialMazeEnter.IImpl.GetGate1Widget()
        {
            return adventureMazeSelectView.GetElementWidget(MultiMazeWaitingRoomData.GATE_1);
        }
        #endregion
    }
}