using Ragnarok.View;
using Ragnarok.View.BattleDuel;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleDuel : UICanvas
    {
        public enum UnitType
        {
            Player,
            Enemy,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide | UIType.Back;

        [SerializeField] BattleDuelView battleDuelView;
        [SerializeField] BattleDuelCountView battleDuelCountView;
        [SerializeField] UIButtonHelper btnGiveUp;

        // 셀렉트 팝업
        [SerializeField] GameObject goPopupBase;
        [SerializeField] UILabelHelper labPopupTitle;
        [SerializeField] UILabelHelper labPopupDesc;
        [SerializeField] UIButtonHelper btnPopupConfirm;
        [SerializeField] UIButtonHelper btnPopupCancel;
        [SerializeField] UIButtonHelper btnPopupExit;

        BattleDuelPresenter presenter;

        public bool IsToggleOn { get; private set; }

        public event System.Action OnGiveUp;

        protected override void OnInit()
        {
            presenter = new BattleDuelPresenter();

            battleDuelCountView.OnSelect += OnSelect;

            EventDelegate.Add(btnGiveUp.OnClick, OnClickedBtnGiveUp);

            EventDelegate.Add(btnPopupConfirm.OnClick, OnClickedBtnPopupConfirm);
            EventDelegate.Add(btnPopupCancel.OnClick, OnClickedBtnPopupCancel);
            EventDelegate.Add(btnPopupExit.OnClick, OnClickedBtnPopupCancel);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            battleDuelCountView.OnSelect -= OnSelect;

            EventDelegate.Remove(btnGiveUp.OnClick, OnClickedBtnGiveUp);

            EventDelegate.Remove(btnPopupConfirm.OnClick, OnClickedBtnPopupConfirm);
            EventDelegate.Remove(btnPopupCancel.OnClick, OnClickedBtnPopupCancel);
            EventDelegate.Remove(btnPopupExit.OnClick, OnClickedBtnPopupCancel);

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            battleDuelCountView.Show(); // 토글 시작 정보 포함
            ShowSelectPopup(false);
        }

        protected override void OnHide()
        {
            battleDuelCountView.Hide(); // 토글 시작 정보 포함
        }

        protected override void OnLocalize()
        {
            btnGiveUp.LocalKey = LocalizeKey._47836; // 나가기

            btnPopupConfirm.LocalKey = LocalizeKey._1;
            btnPopupCancel.LocalKey = LocalizeKey._2;
            labPopupTitle.LocalKey = LocalizeKey._47836; // 나가기
            labPopupDesc.LocalKey = LocalizeKey._47837; // 전투를 포기하고 나가시겠습니까?
        }

        public void ResetData()
        {
            battleDuelView.ResetData();
        }

        public void SetData(UnitType type, int point, CharacterEntity entity, bool isSingle = false)
        {
            string tierIconName = isSingle ? PvETierDataManager.SINGLE_GRADE_ICON_NAME : presenter.GetTierIconName(point);
            if (type == UnitType.Player)
            {
                battleDuelView.SetPlayer(tierIconName, entity);
            }
            else
            {
                battleDuelView.SetEnemy(tierIconName, entity);
            }
        }

        public void SetAgents(UnitType type, CharacterEntity[] agents)
        {
            if (type == UnitType.Player)
            {
                battleDuelView.SetPlayerAgents(agents);
            }
            else
            {
                battleDuelView.SetEnemyAgents(agents);
            }
        }

        public void SetActiveCount(bool isActive)
        {
            // Virtual 함수 실행시키기 위해 SetActive 함수 대신 Show/Hide 로 호출
            if (isActive)
            {
                battleDuelCountView.Show();
            }
            else
            {
                battleDuelCountView.Hide();
            }
        }

        public void SetCount(int cur, int max)
        {
            battleDuelCountView.SetCount(cur, max);
        }

        void OnSelect(bool isOn)
        {
            IsToggleOn = isOn;
        }

        /// <summary>
        /// 나가기 팝업
        /// </summary>
        public void ShowSelectPopup(bool isActive)
        {
            goPopupBase.SetActive(isActive);
        }

        #region 버튼 이벤트

        /// <summary>나가기 버튼 클릭 이벤트</summary>
        void OnClickedBtnGiveUp()
        {
            ShowSelectPopup(true);
        }

        /// <summary>확인 버튼 클릭 이벤트</summary>
        void OnClickedBtnPopupConfirm()
        {
            ShowSelectPopup(false);
            OnGiveUp?.Invoke();
        }

        /// <summary>취소 버튼 클릭 이벤트</summary>
        void OnClickedBtnPopupCancel()
        {
            ShowSelectPopup(false);
        }

        #endregion

        protected override void OnBack()
        {
            if (goPopupBase.activeSelf)
            {
                OnClickedBtnPopupCancel();
            }
            else
            {
                ShowSelectPopup(true);
            }
        }
    }
}