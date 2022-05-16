using UnityEngine;
using Ragnarok.View;

namespace Ragnarok
{
    public class MailSubView : UISubCanvas<MailPresenter>, IAutoInspectorFinder
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIButtonHelper btnAllGet;
        [SerializeField] UILabelHelper labelNoMail;
        [SerializeField] UILabelHelper labNotice;
        [SerializeField] MailType MailType;
        [SerializeField] UIMailInviteSlot friendInvite;
        [SerializeField] UIWidget listView;
        [SerializeField] int smallHeight, fullHeight;

        MailInfo[] arrayInfo;
        bool hasNextPage;

        protected override void OnInit()
        {
            // 친구초대 슬롯
            SetInviteSlot();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.ScrollView.onDragFinished = OnDragFinished;
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnAllGet.OnClick, OnClickedBtnAllGet);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnAllGet.OnClick, OnClickedBtnAllGet);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnHide() { }

        protected override void OnLocalize()
        {
            btnAllGet.LocalKey = LocalizeKey._12001; // 모두 받기
            labelNoMail.LocalKey = LocalizeKey._12002; // 현재 우편함이 비어있습니다
            if (labNotice != null)
                labNotice.Text = presenter.GetLabNotice(MailType);
        }

        public void SetInviteSlot()
        {
            // 계정 탭 Normal
            if (MailType == MailType.Account)
            {
                if (Cheat.IsFacebook)
                {
                    listView.height = smallHeight;
                    friendInvite?.SetActive(true);
                    friendInvite?.SetData(OnInvite);
                }
                else
                {
                    listView.height = fullHeight;
                    friendInvite?.SetActive(false);
                }
                btnAllGet.SetActive(true);
            }
            else if (MailType == MailType.OnBuff)
            {
                listView.height = fullHeight;
                friendInvite?.SetActive(false);
                btnAllGet.SetActive(false);
            }
        }

        public void ResetPosition()
        {
            wrapper.SetProgress(0);
        }

        void Refresh()
        {
            arrayInfo = presenter.GetMailInfos(MailType);
            hasNextPage = presenter.HasNextPage(MailType);
            wrapper.Resize(arrayInfo.Length);

            bool isValid;
            if (MailType == MailType.Trade)
            {
                isValid = false;
                for (int i = 0; i < arrayInfo.Length; ++i)
                {
                    if (!arrayInfo[i].isGetItem)
                    {
                        isValid = true;
                        break;
                    }
                }
            }
            else if (MailType == MailType.Account)
            {
                isValid = false;
                for (int i = 0; i < arrayInfo.Length; ++i)
                {
                    if (arrayInfo[i].MailGroup == MailGroup.Normal)
                    {
                        isValid = true;
                        break;
                    }
                }
            }
            else
                isValid = arrayInfo.Length > 0;

            btnAllGet.IsEnabled = isValid;
            labelNoMail.SetActive(arrayInfo.Length == 0);
            if (!isValid)
                presenter.RemoveAlarm(MailType);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIMailInfo ui = go.GetComponent<UIMailInfo>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        private void OnDragFinished()
        {
            if (!hasNextPage)
                return;

            // UIScrollView 408줄 참고
            Bounds b = wrapper.ScrollView.bounds;
            Vector3 constraint = wrapper.Panel.CalculateConstrainOffset(b.min, b.max);
            constraint.x = 0f;

            // 최하단에서 드래그 끝났을 때
            if (constraint.sqrMagnitude > 0.1f && constraint.y < 0f)
                presenter.RequestNextPage(MailType);
        }

        /// <summary>
        /// 모두 받기 버튼 클릭
        /// </summary>
        void OnClickedBtnAllGet()
        {
            presenter.RequestReceiveAllMail(MailType).WrapNetworkErrors();
        }

        void OnInvite()
        {
            if (presenter.HasPermissionUserFriend())
            {
                // 초대 UI 표시
                UI.Show<UIFriendInvite>();
            }
            else
            {
                presenter.LoginFacebookWithPermission();
            }
        }

        public void SetMailType(MailType mailType)
        {
            MailType = mailType;
        }
    }
}
