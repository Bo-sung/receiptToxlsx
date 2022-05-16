using UnityEngine;

namespace Ragnarok.View
{
    public sealed class MyGuildBattleDefenseView : UIView
    {
        [SerializeField] UISingleGuildBattleElement myGuild;
        [SerializeField] UITurretReadyInfo leftTurret, rightTurret;
        [SerializeField] UILabelHelper labelNotice;

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._33741; // 수비진형으로 사용한 큐펫은 공격진형에 사용할 수 없습니다.

            leftTurret.SetName(LocalizeKey._33805.ToText()); // (좌) 포링포탑
            rightTurret.SetName(LocalizeKey._33806.ToText()); // (우) 포링포탑
        }

        public void SetData(UISingleGuildBattleElement.IInput input, CupetModel[] leftCupets, CupetModel[] rightCupets)
        {
            myGuild.SetData(input);
            leftTurret.SetCupet(leftCupets);
            rightTurret.SetCupet(rightCupets);
        }
    }
}