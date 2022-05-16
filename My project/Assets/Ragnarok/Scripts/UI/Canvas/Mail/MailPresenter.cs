using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIMail"/>
    /// </summary>
    public sealed class MailPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void SetMailNew(AlarmType alarmType);
        }

        private IronSourceManager ironSourceManager;
        private FacebookManager facebookManager;
        private ConnectionManager connectionManager;

        private readonly IView view;
        private readonly MailModel mailModel;
        private readonly AlarmModel alarmModel;
        private readonly UserModel userModel;
        private MailType curMailType;
        private readonly ShopDataManager shopDataRepo;

        private MailInfo mailInfo;

        public MailPresenter(IView view)
        {
            this.view = view;

            mailModel = Entity.player.Mail;
            alarmModel = Entity.player.AlarmModel;
            userModel = Entity.player.User;
            shopDataRepo = ShopDataManager.Instance;
            ironSourceManager = IronSourceManager.Instance;
            facebookManager = FacebookManager.Instance;
            connectionManager = ConnectionManager.Instance;
        }

        public override void AddEvent()
        {
            alarmModel.OnAlarm += view.SetMailNew;
        }

        public override void RemoveEvent()
        {
            alarmModel.OnAlarm -= view.SetMailNew;
        }

        /// <summary>
        /// 메일 타입 반환
        /// </summary>
        /// <returns></returns>
        public MailType GetCurMailType()
        {
            return curMailType;
        }


        /// <summary>
        /// 메일 목록 반환
        /// </summary>
        /// <param name="mailType"></param>
        /// <returns></returns>
        public MailInfo[] GetMailInfos(MailType mailType)
        {
            return mailModel.GetMailInfos(mailType);
        }

        /// <summary>
        /// 다음 페이지 있는지 여부
        /// </summary>
        /// <param name="mailType"></param>
        /// <returns></returns>
        public bool HasNextPage(MailType mailType)
        {
            return mailModel.HasNextPage(mailType);
        }

        /// <summary>
        /// 메일 리스트 요청
        /// </summary>
        public async Task RequestMailList(MailType mailType)
        {
            SetMailType(mailType);
            mailModel.Clear(mailType);
            await mailModel.RequestMailList(0, mailType);
        }

        public void SetMailType(MailType mailType)
        {
            curMailType = mailType;
        }

        /// <summary>
        /// 메일 다음페이지 요청
        /// </summary>
        /// <param name="mailType"></param>
        public async void RequestNextPage(MailType mailType)
        {
            await mailModel.RequestNextMailList(mailType);
            if (view != null)
                view.Refresh();
        }

        /// <summary>
        /// 메일 받기
        /// </summary>
        public async Task RequestReceiveMail(MailInfo info)
        {
            if (curMailType == MailType.OnBuff)
            {
                if (!userModel.IsOnBuffAccountLink())
                {
                    if (await UI.SelectPopup(LocalizeKey._90326.ToText())) // INNO 지갑 연동 후 받기가 가능합니다.\n연동 하시겠습니까?
                    {
                        UI.ShortCut<UIOption>();
                    }
                    return;
                }

                // 온버프 로그인이 안 되어 있을 때 로그인 시도 후 메일 받기
                if(!userModel.IsOnBuffLogin)
                {
                    if (!await userModel.RequestOnBuffLogin())
                        return;
                }
            }

            await mailModel.RequestReceiveMail(info, curMailType);
            if (view != null)
                view.Refresh();
        }

        /// <summary>
        /// 메일 모두 받기
        /// </summary>
        public async Task RequestReceiveAllMail(MailType mailType)
        {
            await mailModel.RequestReceiveAllMail(mailType);
            if (view != null)
                view.Refresh();
        }

        /// <summary>
        /// 광고메일 받기
        /// </summary>
        public async Task RequestReceiveAdMail(MailInfo info)
        {
            string message = LocalizeKey._8058.ToText(); // 광고를 시청하고 상품을 받으시겠습니까?
            string confirmText = LocalizeKey._8017.ToText(); // 광고 보기
            string cancelText = LocalizeKey._1001.ToText(); // 삭 제

            bool? result = await UI.SelectClosePopup(message, confirmText, cancelText, ConfirmButtonType.Ad);

            if (!result.HasValue)
            {
                // 닫기 버튼
                return;
            }
            else
            {
                if (result.Value)
                {
                    // 가방 무게 체크
                    if (info.IsWeight && !UI.CheckInvenWeight())
                        return;

                    // 광고 보기
                    mailInfo = info;
                    ironSourceManager.ShowRewardedVideo(IronSourceManager.PlacementNameType.None, false, false, OnCompleteRewardVideo);
                }
                else
                {
                    // 삭제
                    await mailModel.RequestDeleteMail(info, curMailType);
                    if (view != null)
                        view.Refresh();
                }
            }
        }

        /// <summary>
        /// 페북메일 받기
        /// </summary>
        public async Task RequestReceiveFBMail(MailInfo info)
        {
            string message = LocalizeKey._90304.ToText(); // 웹 페이지를 방문하고 보상을 받으시겠습니까?
            string confirmText = LocalizeKey._801.ToText(); // 예
            string cancelText = LocalizeKey._1001.ToText(); // 삭 제

            bool? result = await UI.SelectClosePopup(message, confirmText, cancelText, ConfirmButtonType.Ad);

            if (!result.HasValue)
            {
                // 닫기 버튼
                return;
            }
            else
            {
                if (result.Value)
                {
                    // 가방 무게 체크
                    if (info.IsWeight && !UI.CheckInvenWeight())
                        return;

                    // 웹페이지 이동
                    if (GameServerConfig.IsKorea())
                    {
                        BasisUrl.NaverHomepage.OpenUrl(); // 네이버 라운지 이동
                    }
                    else if (connectionManager.IsTaiwan())
                    {
                        BasisUrl.TaiwanFaceBookHompage.OpenUrl(); // 페이스북 공식 페이지 이동
                    }
                    else
                    {
                        BasisUrl.FaceBookHomepage.OpenUrl(); // 페이스북 공식 페이지 이동
                    }

                    await Awaiters.Seconds(5f);

                    RequestReceiveMail(info).WrapNetworkErrors();
                }
                else
                {
                    // 삭제
                    await mailModel.RequestDeleteMail(info, curMailType);
                    if (view != null)
                        view.Refresh();
                }
            }
        }

        public bool HasPermissionUserFriend()
        {
            return facebookManager.HasPermissionUserFriend();
        }

        public void LoginFacebookWithPermission()
        {
            facebookManager.LoginWithUserFriendPermission();
        }

        private void OnCompleteRewardVideo()
        {
            // 보상 처리
            RequestReceiveMail(mailInfo).WrapNetworkErrors();
        }

        /// <summary>
        /// 우편함 알람표시 제거
        /// </summary>
        public void RemoveAlarm(MailType mailType)
        {
            switch (mailType)
            {
                case MailType.Account:
                    alarmModel.RemoveAlarm(AlarmType.MailAccount);
                    break;
                case MailType.Character:
                    alarmModel.RemoveAlarm(AlarmType.MailCharacter);
                    break;
                case MailType.Shop:
                    alarmModel.RemoveAlarm(AlarmType.MailShop);
                    break;
                case MailType.Trade:
                    alarmModel.RemoveAlarm(AlarmType.MailTrade);
                    break;
                case MailType.OnBuff:
                    alarmModel.RemoveAlarm(AlarmType.MailOnBuff);
                    break;
            }
        }

        /// <summary>
        /// 상점탭에서 메일 아이콘이름
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public string GetMailShopIconName(int shopId)
        {
            ShopData data = shopDataRepo.Get(shopId);
            if (data == null)
                return string.Empty;

            return data.mail_icon;
        }

        public string GetLabNotice(MailType mailType)
        {
            switch (mailType)
            {
                case MailType.Trade:
                    return LocalizeKey._12011.ToText(); // 거래 기록은 최대 50개 까지 저장이 되며, 수령 금액 및\n아이템은 3일, 미수령 금액 및 아이템은 30일 보관 후 삭제 됩니다.
            }
            return string.Empty;
        }

        /// <summary>
        /// GM 메시지 확인 & 삭제
        /// </summary>
        public async Task RequestReceiveMessageMail(MailInfo info)
        {
            string message = info.GetMessage();
            string confirmText = LocalizeKey._1.ToText(); // 확인
            string cancelText = LocalizeKey._1001.ToText(); // 삭 제

            bool? result = await UI.SelectClosePopup(message, confirmText, cancelText, ConfirmButtonType.Ad);

            if (!result.HasValue)
            {
                // 닫기 버튼
                return;
            }
            else
            {
                if (!result.Value)
                {
                    await mailModel.RequestDeleteMail(info, curMailType);
                    if (view != null)
                        view.Refresh();
                }
            }
        }
    }
}