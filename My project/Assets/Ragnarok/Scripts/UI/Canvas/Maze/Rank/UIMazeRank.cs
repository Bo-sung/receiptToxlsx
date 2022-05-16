using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMazeRank : UICanvas, IMazeRankCanvas
    {
        public class Input : IUIData
        {
            public int mazeMapId;
            public Input(int mazeMapId)
            {
                this.mazeMapId = mazeMapId;
            }
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SimplePopupView popupView;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIMazeRankSlot[] mazeRanks;
        [SerializeField] UIMazeRankSlot myRank;
        [SerializeField] UILabelHelper labelMapName;
        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelTime;

        MazeRankPresenter presenter;

        protected override void OnInit()
        {
            presenter = new MazeRankPresenter(this);
            popupView.OnExit += OnExit;

            presenter.AddEvent();

            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnExit);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnExit);
            popupView.OnExit -= OnExit;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is Input input)
            {
                presenter.Set(input.mazeMapId);
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._19600; // 랭킹
            labelRank.LocalKey = LocalizeKey._19601; // 등수
            labelName.LocalKey = LocalizeKey._19602; // 유저
            labelTime.LocalKey = LocalizeKey._19603; // 시간
            btnConfirm.LocalKey = LocalizeKey._19604; // 확인
        }

        public void Refresh()
        {
            labelMapName.Text = presenter.GetMazeMapName();

            var info = presenter.GetRankInfos();
            for (int i = 0; i < mazeRanks.Length; i++)
            {
                if (i < info.Length)
                {
                    mazeRanks[i].SetActive(true);
                    mazeRanks[i].Set(i + 1, info[i].CharName, info[i].Score);
                }
                else
                {
                    mazeRanks[i].SetActive(false);
                }
            }

            var myRankInfo = presenter.GetMyRankInfo();
            myRank.Set((int)myRankInfo.Rank, myRankInfo.CharName, myRankInfo.Score);
        }

        void OnExit()
        {
            OnClickedBtnExit();
        }

        void OnClickedBtnExit()
        {
            UI.Close<UIMazeRank>();
        }
    }
}