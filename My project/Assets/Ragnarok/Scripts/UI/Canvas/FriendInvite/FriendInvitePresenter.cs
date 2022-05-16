using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class FriendInvitePresenter : ViewPresenter
    {
        // <!-- Models --!>
        FriendModel friendModel;

        // <!-- Repositories --!>
        FacebookManager facebookManager;
        RewardGroupDataManager rewardManager;

        // <!-- Event --!>
        public event System.Action OnRefresh
        {
            add { friendModel.onRefresh += value; }
            remove { friendModel.onRefresh -= value; }
        }

        public FriendInvitePresenter()
        {
            friendModel = Entity.player.Friend;

            facebookManager = FacebookManager.Instance;
            rewardManager = RewardGroupDataManager.Instance;
        }

        public override void AddEvent()
        {
            facebookManager.OnCheckAccept += CheckAccept;
            facebookManager.OnCheckInvite += CheckInvite;
        }

        public override void RemoveEvent()
        {
            facebookManager.OnCheckAccept -= CheckAccept;
            facebookManager.OnCheckInvite -= CheckInvite;
        }

        /// <summary>
        /// 친구초대 권한이 있는지 체크
        /// </summary>
        public bool HasPermissionUserFriend()
        {
            return facebookManager.HasPermissionUserFriend();
        }

        // 친구리스트 요청 fb
        public void RequestFacebookFriends()
        {
            facebookManager.RequestFacebookFriends();
        }

        // 친구초대 fb
        public void ShowFacebookInvite()
        {
            facebookManager.ShowFacebookInvite();
        }

        public int GetCompleteCount()
        {
            return friendModel.GetInviteCount();
        }

        public RewardGroupData[] GetRewardDatas()
        {
            return rewardManager.Gets((int)RewardType.InviteReward);
        }

        void CheckAccept(string[] ids)
        {
            friendModel.RequestAcceptFriend(ids).WrapNetworkErrors();
        }

        void CheckInvite(string[] ids)
        {
            friendModel.RequestInviteFriend(ids).WrapNetworkErrors();
        }
    }
}