using UnityEngine;

namespace Ragnarok.View.BattleStage
{
    public class UIBattleStageAgentSlot : UIBattleStageCharacterSlot, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] GameObject effectHealing;

        protected override void AddEvent()
        {
            base.AddEvent();

            if (unitEntity == null)
                return;

            unitEntity.OnRecoveryHp += OnRecoveryHp;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            if (unitEntity == null)
                return;

            unitEntity.OnRecoveryHp -= OnRecoveryHp;
        }

        protected override void Refresh()
        {
            base.Refresh();

            // Icon Job 세팅
            if (iconJob == null)
                return;

            if (characterEntity == null)
                return;

            iconJob.SetJobIcon(characterEntity.Character.Job.GetJobIcon());
        }

        protected override string GetThumbnailName()
        {
            return characterEntity.GetThumbnailName();
        }

        void OnRecoveryHp(int value, int count)
        {
            // 힐링 이펙트 재생
            if (effectHealing == null)
                return;

            effectHealing.SetActive(false);
            effectHealing.SetActive(true);
        }
    }
}