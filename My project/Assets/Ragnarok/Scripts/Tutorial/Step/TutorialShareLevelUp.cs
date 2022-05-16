using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialShareLevelUp : TutorialStep
    {
        public TutorialShareLevelUp() : base(TutorialType.ShareLevelUp)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.ShareLevelUp))
                return false;
            
            if (!Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false))
                return false;

            var mode = BattleManager.Instance.GetCurrentEntry().mode;
            if (mode != BattleMode.MultiMazeLobby && mode != BattleMode.Stage)
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

            yield return WaitUntilHideDailyCheck();
            yield return WaitUntilHideCanvas<UIQuestReward>();
            yield return WaitUntilHideCanvas<UICharacterShareWaiting>();

            // Step1: 스토리북 오픈
            UI.Show<UIStoryBook>().SetChapter(4);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();
            
            yield return Show(LocalizeKey._26066.ToText(), UIWidget.Pivot.Bottom);

            yield return Show(LocalizeKey._26067.ToText(), UIWidget.Pivot.Bottom);
            
            UIContentsUnlock contentsUnlock = UI.Show<UIContentsUnlock>();
            contentsUnlock.Set(ContentType.ShareLevelUp);

            HideTutorial();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            ShowTutorial();
            HideBlind();

            yield return WaitUntilHideCanvas<UIContentsLauncher>();

            ShowBlind();

            TutorialSharingCharacterEquip.IOpenCharacterShareImpl openCharacterShareImpl = UI.GetUI<UIQuickExpandMenu>();

            // Step2: 다른 유저의 캐릭터를 [62AEE4][C]공유[/c][-]받아볼까요?
            yield return Show(LocalizeKey._26068.ToText(), UIWidget.Pivot.Top, openCharacterShareImpl.GetBtnSharing(), IsVisibleCanvas<UICharacterShare>);

            TutorialSharingCharacterEquip.ICharacterShareImpl characterShareImpl = UI.GetUI<UICharacterShare>();
            var characterShareView = characterShareImpl.GetEquipSharingCharacterImpl();

            yield return Show(LocalizeKey._26069.ToText(), UIWidget.Pivot.Center, characterShareView.GetWidgetViceStatus());

            yield return Show(LocalizeKey._26070.ToText(), UIWidget.Pivot.Top, characterShareView.GetBtnViceLevelUp(), characterShareImpl.IsShareviceLevelUpViewShowing);

            UISharevice shareVice = characterShareImpl.GetSharevice();

            yield return Show(LocalizeKey._26071.ToText(), UIWidget.Pivot.Top, shareVice.GetBtnFirstExperienceItem(), shareVice.IsFirstExperienceItemSelected);

            yield return Show(LocalizeKey._26072.ToText(), UIWidget.Pivot.Top, shareVice.GetBtnLevelUp(), shareVice.IsLevelUpAccomplished);

            yield return Show(LocalizeKey._26073.ToText(), UIWidget.Pivot.Top, shareVice.GetBtnClose(), characterShareImpl.IsShareviceLevelUpViewHiding);

            yield return Show(LocalizeKey._26074.ToText(), UIWidget.Pivot.Top, characterShareView.GetWidgetCharacterPanel());

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}