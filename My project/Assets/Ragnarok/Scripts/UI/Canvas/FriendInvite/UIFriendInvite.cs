using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIFriendInvite : UICanvas, IInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnInvite;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelMyCount;

        [SerializeField] UILabelValue[] descAry;
        [SerializeField] UIFriendInviteRewardSlot[] rewardSlots;

        FriendInvitePresenter presenter;

        protected override void OnInit()
        {
            presenter = new FriendInvitePresenter();

            presenter.AddEvent();
            presenter.OnRefresh += RefreshInfo;

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnInvite.OnClick, OnClickedBtnInvite);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            presenter.OnRefresh -= RefreshInfo;

            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(btnInvite.OnClick, OnClickedBtnInvite);
        }

        protected override void OnShow(IUIData data = null)
        {
            // 보상 셋팅
            RewardGroupData[] rewards = presenter.GetRewardDatas();
            for (int i = 0; i < rewardSlots.Length; i++)
            {
                rewardSlots[i].SetData(rewards[i]);
            }

            // 완료 보상 갱신
            RefreshInfo();

            // 친구리스트 요청
            if (presenter.HasPermissionUserFriend())
            {
                presenter.RequestFacebookFriends();
            }
            else
            {
                // 친구초대 권한이 없으면 UI 닫아 줌.
                Close();
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._6602; // 친구 초대

            for (int i = 0; i < descAry.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        descAry[i].ValueKey = LocalizeKey._6603; // 페이스북으로 친구를 초대합니다.
                        break;

                    case 1:
                        descAry[i].ValueKey = LocalizeKey._6604; // 친구가 초대를 수락합니다.
                        break;

                    case 2:
                        descAry[i].ValueKey = LocalizeKey._6605; // 초대받은 친구가 페이스북 연동 시 우편함으로 보상을 지급받습니다.
                        break;

                    default:
                        descAry[i].Value = "";
                        break;
                }
            }
        }

        private void RefreshInfo()
        {
            // 완료인원 갱신
            int compCnt = presenter.GetCompleteCount(); // 완료인원..
            labelMyCount.Text = LocalizeKey._6606.ToText() // 완료 인원 : {COUNT}명
                .Replace(ReplaceKey.COUNT, compCnt);

            // 획득한 보상 있으면, 완료 아이콘 표시
            for (int i = 0; i < rewardSlots.Length; i++)
            {
                rewardSlots[i].SetComplete(compCnt);
            }
        }

        private void OnClickedBtnExit()
        {
            Close();
        }

        private void OnClickedBtnInvite()
        {
            presenter.ShowFacebookInvite();
        }

        private void Close()
        {
            UI.Close<UIFriendInvite>();
        }

        bool IInspectorFinder.Find()
        {
            descAry = GetComponentsInChildren<UILabelValue>();
            rewardSlots = GetComponentsInChildren<UIFriendInviteRewardSlot>();
            return true;
        }
    }
}