using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class TutorialSharingCharacterEquip : TutorialStep
    {
        public interface IOpenCharacterShareImpl
        {
            UIWidget GetBtnSharing();
            bool IsClickedBtnSharing();
            UIWidget GetWidgetExpand();
        }

        public interface ICharacterShareImpl
        {
            bool IsEmptySharingTime();
            void RequestShareCharacterFreeDailyTimeTicketUse();

            ISingleEquipSharingCharacterImpl GetSingleEquipSharingCharacterImpl();
            IEquipSharingCharacterImpl GetEquipSharingCharacterImpl();

            void HideCharacterShareUI();
            bool IsShareviceLevelUpViewShowing();
            bool IsShareviceLevelUpViewHiding();

            UISharevice GetSharevice();
        }

        public interface ISingleEquipSharingCharacterImpl
        {
            bool IsShown();
            void SetTutorialMode(bool isTutorialMode);

            UIWidget GetFirstBar();
            bool IsSelectedFirstBar();
            UIWidget GetBtnSelect();
        }

        public interface IEquipSharingCharacterImpl
        {
            void SetTutorialMode(bool isTutorialMode);

            UIWidget GetBtnAddShare();
            bool IsEquippedSingleSharingCharacter();
            UIWidget GetBtnAutoSetting();
            bool IsEquippedSharingCharacter();
            UIWidget GetWidgetViceStatus();
            UIWidget GetBtnViceLevelUp();
            UIWidget GetWidgetCharacterPanel();
        }

        public TutorialSharingCharacterEquip() : base(TutorialType.SharingCharacterEquip)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            // 셰어링 컨텐츠가 오픈되어 있지 않음
            if (!Entity.player.Quest.IsOpenContent(ContentType.Sharing, isShowPopup: false))
                return false;

            // UIQuickExpandMenu가 없음
            if (!IsVisibleCanvas<UIQuickExpandMenu>())
                return false;

            return true;
        }

        protected override bool HasSkip()
        {
            return false;
        }

        protected override Npc GetNpc()
        {
            return Npc.HOLORUCHI;
        }

        protected override IEnumerator<float> Run()
        {
            HideTutorial();

            // UIDailyCheck UI 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideDailyCheck();

            // 튜토리얼 화면
            ShowTutorial();

            IOpenCharacterShareImpl openCharacterShareImpl = UI.GetUI<UIQuickExpandMenu>();

            // Step1: 모험을 떠나려면 든든한 파티원이 필요하실거에요.
            yield return Show(LocalizeKey._66000.ToText(), UIWidget.Pivot.Center);

            // Step2: 다른 유저의 캐릭터를 [62AEE4][C]공유[/c][-]받아볼까요?
            yield return Show(LocalizeKey._66001.ToText(), UIWidget.Pivot.Center, openCharacterShareImpl.GetBtnSharing(), IsVisibleCanvas<UICharacterShare>);

            ICharacterShareImpl characterShareImpl = UI.GetUI<UICharacterShare>();
            ISingleEquipSharingCharacterImpl singleEquipSharingCharacterImpl = characterShareImpl.GetSingleEquipSharingCharacterImpl();
            IEquipSharingCharacterImpl equipSharingCharacterImpl = characterShareImpl.GetEquipSharingCharacterImpl();

            // 캐릭터 셰어링 시간이 없을 경우
            if (characterShareImpl.IsEmptySharingTime())
                characterShareImpl.RequestShareCharacterFreeDailyTimeTicketUse(); // 서버로 일일 무료 충전 티켓 사용 프로토콜 날리기

            yield return Timing.WaitUntilFalse(characterShareImpl.IsEmptySharingTime); // 캐릭터 셰어링 시간이 존재할 때까지 기다림

            // Step3: 캐릭터를 공유받기 위한 슬롯을 눌러주세요.
            yield return Show(LocalizeKey._66002.ToText(), UIWidget.Pivot.Top, equipSharingCharacterImpl.GetBtnAddShare(), singleEquipSharingCharacterImpl.IsShown);

            singleEquipSharingCharacterImpl.SetTutorialMode(true); // 튜토리얼 모드로 변경
            equipSharingCharacterImpl.SetTutorialMode(true); // 튜토리얼 모드로 변경

            // Step4: 이 캐릭터를 고용해볼까요?
            yield return Show(LocalizeKey._66003.ToText(), UIWidget.Pivot.Top, singleEquipSharingCharacterImpl.GetFirstBar(), singleEquipSharingCharacterImpl.IsSelectedFirstBar);

            // Step5: [62AEE4][C]캐릭터 고용[/c][-]을 눌러주세요.
            yield return Show(LocalizeKey._66004.ToText(), UIWidget.Pivot.Top, singleEquipSharingCharacterImpl.GetBtnSelect(), equipSharingCharacterImpl.IsEquippedSingleSharingCharacter);

            // Step6: 총 4명까지 고용이 가능해요!\n이번엔 간편한 [62AEE4][C]자동 장착[/c][-]을 이용해볼까요?
            yield return Show(LocalizeKey._66005.ToText(), UIWidget.Pivot.Top, equipSharingCharacterImpl.GetBtnAutoSetting(), equipSharingCharacterImpl.IsEquippedSharingCharacter);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄

            singleEquipSharingCharacterImpl.SetTutorialMode(false); // 튜토리얼 모드 해제
            equipSharingCharacterImpl.SetTutorialMode(false); // 튜토리얼 모드로 해제

            // Step7: 파티원도 다 구했으니,\n이제 본격적으로 모험을 떠나볼까요?
            yield return Show(LocalizeKey._66006.ToText(), UIWidget.Pivot.Center);

            characterShareImpl.HideCharacterShareUI();

            UI.Show<UIRewardZeny>();
        }
    }
}