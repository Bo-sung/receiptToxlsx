using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialShareControl2 : TutorialStep
    {
        public TutorialShareControl2() : base(TutorialType.ShareControl2)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!Tutorial.HasAlreadyFinished(TutorialType.ShareControl1))
                return false;

            if (Tutorial.HasAlreadyFinished(TutorialType.ShareControl2))
                return false;
            
            if (!Entity.player.Quest.IsOpenContent(ContentType.ShareControl, false))
                return false;
            
            if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.Stage)
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

            ShowTutorial();

            yield return WaitUntilShowCanvas<UIQuickExpandMenu>();

            UIQuickExpandMenu quickExpandMenu = UI.GetUI<UIQuickExpandMenu>();

            quickExpandMenu.ExpandShare();

            TutorialSharingCharacterEquip.IOpenCharacterShareImpl openCharacterShareImpl = quickExpandMenu;

            yield return Show(LocalizeKey._26063.ToText(), UIWidget.Pivot.Bottom);

            yield return Show(LocalizeKey._26064.ToText(), UIWidget.Pivot.Top, openCharacterShareImpl.GetWidgetExpand());

            yield return Show(LocalizeKey._26065.ToText(), UIWidget.Pivot.Top, openCharacterShareImpl.GetWidgetExpand());
            
            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}