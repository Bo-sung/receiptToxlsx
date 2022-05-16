using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialTrade : TutorialStep
    {
        public TutorialTrade() : base(TutorialType.Trade)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.Lobby)
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
            yield return WaitUntilShowCanvas<UIBattleMenu>();
            yield return WaitUntilShowCanvas<UIMainTop>();

            ShowTutorial();

            UIBattleMenu battleMenu = UI.GetUI<UIBattleMenu>();
            UIMainTop mainTop = UI.GetUI<UIMainTop>();

            yield return Show(LocalizeKey._26050.ToText(), UIWidget.Pivot.Center);

            yield return Show(LocalizeKey._26051.ToText(), UIWidget.Pivot.Center, mainTop.RoPointWidget);

            yield return Show(LocalizeKey._26052.ToText(), UIWidget.Pivot.Center, battleMenu.TradeButtonWidget);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}