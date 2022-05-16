namespace Ragnarok
{
    public interface IGamePotImpl : ILoginImpl
    {
        void ShowAgreeDialog();
        void ShowNotice(bool showTodayButton);
        void ShowCSWebView();
        void ShowTerms();
        void ShowPrivacy();
        void TogglePush();
        void ToggleNightPush();
        bool IsPush();
        bool IsNightPush();

        bool IsLinkedLogin(NCommon.LinkingType linkingType);
        void ToggleLinked(NCommon.LinkingType linkingType);
        void SendLogCharacter(GamePotSendLogCharacter sendLogCharacter);
        NPurchaseItem[] GetPurchaseItems();
        void GetPurchaseDetailListAsync();
        void Purchase(string productId, string uniqueId);
        void Coupon(string couponNumber, string userData);
        string GetMemberId();
        bool IsGuestLogin();
        void DeleteMember();

        event System.Action<NAgreeResultInfo> OnAgreeDialogSuccess;
        event System.Action OnUpdateLinked;
        event System.Action OnUpdatePush;
        event System.Action OnUpdateNightPush;
        event System.Action OnPurchaseSuccess;
        event System.Action<NPurchaseItem[]> OnPurchaseItemListSuccess;
        event System.Action OnDeleteMeberSuccess;
    }
}