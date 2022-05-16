using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialShareClone : TutorialStep
    {
        public TutorialShareClone() : base(TutorialType.ShareClone)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.ShareClone))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.ShareHope, false))
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
            UI.Show<UIStoryBook>().SetChapter(10);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();

            // 이 홀로군이 칭찬할 정도로 훌륭했어! 아직 더 모아야겠지만, [62AEE4][C]이미르의 조각[/c][-]의 추가 단서를 찾을 수 있겠지.
            yield return Show(LocalizeKey._26097.ToText(), UIWidget.Pivot.Bottom);

            // 자, 그 전에 쉐어 바이스의 동조 기능을 이용해보자구. [62AEE4][C]이미르의 조각[/c][-]을 찾았을 때를 위해서 만들어진 기능이야.
            yield return Show(LocalizeKey._26098.ToText(), UIWidget.Pivot.Bottom);

            // [EE3F48][C](파지직!)[/c][-] !?
            yield return Show(LocalizeKey._26099.ToText(), UIWidget.Pivot.Bottom);

            // 이건.. 신기하네. 새로운 쉐어 슬롯이 생겼어. 기존 것과 달라 보이는데, 계정 안의 모험가를 불러올 수 있는 슬롯인 것 같아.
            yield return Show(LocalizeKey._26100.ToText(), UIWidget.Pivot.Bottom);

            UIContentsUnlock contentsUnlock = UI.Show<UIContentsUnlock>();
            contentsUnlock.Set(ContentType.ShareHope);

            HideTutorial();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            ShowTutorial();
            HideBlind();

            yield return WaitUntilHideCanvas<UIContentsLauncher>();

            ShowBlind();
            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}