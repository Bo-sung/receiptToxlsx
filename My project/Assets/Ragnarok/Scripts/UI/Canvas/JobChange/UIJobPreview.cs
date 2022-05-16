using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobPreview : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UILabelHelper label1st;
        [SerializeField] UILabelHelper label2st;
        [SerializeField] UIButtonHelper btnConfirm;

        [SerializeField] UIButtonHelper btnDetailInfo_1st_00;
        [SerializeField] UIButtonHelper btnDetailInfo_1st_01;
        [SerializeField] UIButtonHelper btnDetailInfo_1st_02;
        [SerializeField] UIButtonHelper btnDetailInfo_1st_03;
        [SerializeField] UIButtonHelper btnDetailInfo_2nd_00;
        [SerializeField] UIButtonHelper btnDetailInfo_2nd_01;
        [SerializeField] UIButtonHelper btnDetailInfo_2nd_02;
        [SerializeField] UIButtonHelper btnDetailInfo_2nd_03;
        [SerializeField] UIButtonHelper btnDetailInfo_2nd_04;
        [SerializeField] UIButtonHelper btnDetailInfo_2nd_05;
        [SerializeField] UIButtonHelper btnDetailInfo_2nd_06;
        [SerializeField] UIButtonHelper btnDetailInfo_2nd_07;

        protected override void OnInit()
        {
            EventDelegate.Add(btnClose.OnClick, OnBack);
            EventDelegate.Add(btnConfirm.OnClick, OnBack);

            EventDelegate.Add(btnDetailInfo_1st_00.OnClick, OnClickedBtnDetailInfo_1st_00);
            EventDelegate.Add(btnDetailInfo_1st_01.OnClick, OnClickedBtnDetailInfo_1st_01);
            EventDelegate.Add(btnDetailInfo_1st_02.OnClick, OnClickedBtnDetailInfo_1st_02);
            EventDelegate.Add(btnDetailInfo_1st_03.OnClick, OnClickedBtnDetailInfo_1st_03);
            EventDelegate.Add(btnDetailInfo_2nd_00.OnClick, OnClickedBtnDetailInfo_2nd_00);
            EventDelegate.Add(btnDetailInfo_2nd_01.OnClick, OnClickedBtnDetailInfo_2nd_01);
            EventDelegate.Add(btnDetailInfo_2nd_02.OnClick, OnClickedBtnDetailInfo_2nd_02);
            EventDelegate.Add(btnDetailInfo_2nd_03.OnClick, OnClickedBtnDetailInfo_2nd_03);
            EventDelegate.Add(btnDetailInfo_2nd_04.OnClick, OnClickedBtnDetailInfo_2nd_04);
            EventDelegate.Add(btnDetailInfo_2nd_05.OnClick, OnClickedBtnDetailInfo_2nd_05);
            EventDelegate.Add(btnDetailInfo_2nd_06.OnClick, OnClickedBtnDetailInfo_2nd_06);
            EventDelegate.Add(btnDetailInfo_2nd_07.OnClick, OnClickedBtnDetailInfo_2nd_07);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClose.OnClick, OnBack);
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);

            EventDelegate.Remove(btnDetailInfo_1st_00.OnClick, OnClickedBtnDetailInfo_1st_00);
            EventDelegate.Remove(btnDetailInfo_1st_01.OnClick, OnClickedBtnDetailInfo_1st_01);
            EventDelegate.Remove(btnDetailInfo_1st_02.OnClick, OnClickedBtnDetailInfo_1st_02);
            EventDelegate.Remove(btnDetailInfo_1st_03.OnClick, OnClickedBtnDetailInfo_1st_03);
            EventDelegate.Remove(btnDetailInfo_2nd_00.OnClick, OnClickedBtnDetailInfo_2nd_00);
            EventDelegate.Remove(btnDetailInfo_2nd_01.OnClick, OnClickedBtnDetailInfo_2nd_01);
            EventDelegate.Remove(btnDetailInfo_2nd_02.OnClick, OnClickedBtnDetailInfo_2nd_02);
            EventDelegate.Remove(btnDetailInfo_2nd_03.OnClick, OnClickedBtnDetailInfo_2nd_03);
            EventDelegate.Remove(btnDetailInfo_2nd_04.OnClick, OnClickedBtnDetailInfo_2nd_04);
            EventDelegate.Remove(btnDetailInfo_2nd_05.OnClick, OnClickedBtnDetailInfo_2nd_05);
            EventDelegate.Remove(btnDetailInfo_2nd_06.OnClick, OnClickedBtnDetailInfo_2nd_06);
            EventDelegate.Remove(btnDetailInfo_2nd_07.OnClick, OnClickedBtnDetailInfo_2nd_07);
        }

        protected override void OnHide()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnLocalize()
        {
            label1st.LocalKey = LocalizeKey._2002; // 1차 전직
            label2st.LocalKey = LocalizeKey._2003; // 2차 전직
            btnConfirm.LocalKey = LocalizeKey._2015; // 확인
        }

        void OnClickedBtnDetailInfo_1st_00()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Novice, Gender.Male, Job.Swordman);
        }

        void OnClickedBtnDetailInfo_1st_01()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Novice, Gender.Male, Job.Archer);
        }

        void OnClickedBtnDetailInfo_1st_02()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Novice, Gender.Male, Job.Magician);
        }

        void OnClickedBtnDetailInfo_1st_03()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Novice, Gender.Male, Job.Thief);
        }

        void OnClickedBtnDetailInfo_2nd_00()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Swordman, Gender.Male, 0);
        }

        void OnClickedBtnDetailInfo_2nd_01()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Swordman, Gender.Male, 1);
        }

        void OnClickedBtnDetailInfo_2nd_02()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Archer, Gender.Male, 1);
        }

        void OnClickedBtnDetailInfo_2nd_03()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Archer, Gender.Male, 0);
        }

        void OnClickedBtnDetailInfo_2nd_04()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Magician, Gender.Male, 1);
        }

        void OnClickedBtnDetailInfo_2nd_05()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Magician, Gender.Male, 0);
        }

        void OnClickedBtnDetailInfo_2nd_06()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Thief, Gender.Male, 1);
        }

        void OnClickedBtnDetailInfo_2nd_07()
        {
            UI.Show<UIJobInfoPreview>().Show(Job.Thief, Gender.Male, 0);
        }
    }
}