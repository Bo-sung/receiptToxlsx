using MEC;

namespace Ragnarok
{
    public class NpcEffectPlayer : UnitEffectPlayer
    {
        protected NpcEntity entity;
        protected HudUnitName hudName;
        protected PoolObject questionMark_Yellow; // 물음표
        protected PoolObject questionMark_Gray; // 물음표
        protected PoolObject exclamationMark; // 느낌표 

        protected override bool IsCharacter => false;

        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            this.entity = entity as NpcEntity;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (hudName)
            {
                hudName.Release();
                hudName = null;
            }

            if (questionMark_Yellow)
            {
                questionMark_Yellow.Release();
                questionMark_Yellow = null;
            }

            if (questionMark_Gray)
            {
                questionMark_Gray.Release();
                questionMark_Gray = null;
            }

            if (exclamationMark)
            {
                exclamationMark.Release();
                exclamationMark = null;
            }
        }

        protected override void OnDie(UnitEntity unit, UnitEntity attacker)
        {
            base.OnDie(unit, attacker);
        }

        public override void ShowName()
        {
            if (hudName == null)
                hudName = hudPool.SpawnUnitName(CachedTransform);

            hudName.Initialize(entity.GetNameId(), entity.type);
            hudName.Show();
        }

        public override void HideName()
        {
            if (hudName)
                hudName.Hide();
        }

        public void ShowQuestionMark_Yellow(UnityEngine.Transform parent)
        {
            if (questionMark_Yellow is null)
                questionMark_Yellow = battlePool.SpawnQuestionMark_Yellow(parent);

            questionMark_Yellow.Show();
        }

        public void HideQuestionMark_Yellow()
        {
            if (questionMark_Yellow)
                questionMark_Yellow.Hide();
        }

        public void ShowQuestionMark_Gray(UnityEngine.Transform parent)
        {
            if (questionMark_Gray is null)
                questionMark_Gray = battlePool.SpawnQuestionMark_Gray(parent);

            questionMark_Gray.Show();
        }

        public void HideQuestionMark_Gray()
        {
            if (questionMark_Gray)
                questionMark_Gray.Hide();
        }

        public void ShowExclamationMark(UnityEngine.Transform parent)
        {
            if (exclamationMark is null)
                exclamationMark = battlePool.SpawnExclamationMark(parent);

            exclamationMark.Show();
        }

        public void HideExclamationMark()
        {
            if (exclamationMark)
                exclamationMark.Hide();
        }
    }
}