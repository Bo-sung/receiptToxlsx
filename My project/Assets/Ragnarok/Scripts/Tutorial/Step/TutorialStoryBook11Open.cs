using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialStoryBook11Open : TutorialStep
    {
        public TutorialStoryBook11Open() : base(TutorialType.StoryBook11Open)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.StoryBook11Open))
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.StoryBook11Open, false))
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
            UI.Show<UIStoryBook>().SetChapter(11);

            // Step2: UITutoFirstPopup 가 보이지 않을 때까지 기다린다
            yield return WaitUntilHideCanvas<UIStoryBook>();

            ShowTutorial();

            // 희망의 영웅.. 이전에 그렇게 신호가 왔었지?
            yield return Show(LocalizeKey._26101.ToText(), UIWidget.Pivot.Bottom);

            // 분명 널 가르키는 말일거야. 홀로군한테 숨기는게 있는건 아니겠지?!
            yield return Show(LocalizeKey._26102.ToText(), UIWidget.Pivot.Bottom);

            // 숨기는게 있다면 홀로군한테도 알려줘. 뭐? 모르겠다구?
            yield return Show(LocalizeKey._26103.ToText(), UIWidget.Pivot.Bottom);

            // ... 너가 모른다니, 신기하네. 그럼, 이 홀로군이 열심히 찾아보지! 쿡쿡쿡.
            yield return Show(LocalizeKey._26104.ToText(), UIWidget.Pivot.Bottom);

            HideTutorial();

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}