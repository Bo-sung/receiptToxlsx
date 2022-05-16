using UnityEngine;

namespace Ragnarok
{
    public class HudMazeMonsterName : HudUnitName, IAutoInspectorFinder
    {
        [SerializeField] UIButtonHelper btnInfo;
        [SerializeField] UIButtonHelper btnBattle;
        [SerializeField] GameObject goBattle;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnInfo.OnClick, OnClickedBtnInfo);
            EventDelegate.Add(btnBattle.OnClick, OnClickedBtnBattle);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnInfo.OnClick, OnClickedBtnInfo);
            EventDelegate.Remove(btnBattle.OnClick, OnClickedBtnBattle);
        }

        void OnClickedBtnInfo()
        {
            OnUnitInfo?.Invoke();
        }

        /// <summary>
        /// 해당 몬스터와 배틀시작
        /// </summary>
        void OnClickedBtnBattle()
        {
            OnBattleClick?.Invoke();
        }

        public override void ShowBattleHUD()
        {            
            goBattle.SetActive(true);
        }

        public override void HideBattleHUD()
        {            
            goBattle.SetActive(false);
        }       
    }
}