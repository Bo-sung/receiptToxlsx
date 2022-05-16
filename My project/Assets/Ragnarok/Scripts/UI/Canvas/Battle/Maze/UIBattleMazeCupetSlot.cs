using UnityEngine;

namespace Ragnarok.View.BattleMaze
{
    public class UIBattleMazeCupetSlot : UIBattleCupetSlot, IAutoInspectorFinder
    {
        [SerializeField] GameObject goSelect;

        public event System.Action<CupetEntity> OnSelect;

        void OnClick()
        {
            OnSelect?.Invoke(cupetEntity);
        }

        public void SetSelect(bool isSelect)
        {
            NGUITools.SetActive(goSelect, isSelect);
        }
    }
}