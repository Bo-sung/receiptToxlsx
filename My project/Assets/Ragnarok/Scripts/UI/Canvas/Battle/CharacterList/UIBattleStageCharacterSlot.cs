using UnityEngine;

namespace Ragnarok.View.BattleStage
{
    public class UIBattleStageCharacterSlot : UIBattleCharacterSlot, IAutoInspectorFinder
    {
        [SerializeField] GameObject goSelect;

        public event System.Action<CharacterEntity> OnSelect;

        void OnClick()
        {
            OnSelect?.Invoke(characterEntity);
        }

        public void SetSelect(bool isSelect)
        {
            NGUITools.SetActive(goSelect, isSelect);
        }

        public void RefreshSlot()
        {
            Refresh();
        }
    }
}