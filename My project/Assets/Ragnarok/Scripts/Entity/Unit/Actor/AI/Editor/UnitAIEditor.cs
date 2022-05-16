using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomEditor(typeof(UnitAI), editorForChildClasses: true)]
    public class UnitAIEditor : Editor
    {
        UnitAI ai;
        UnitActor actor;

        int atk;
        CrowdControlType crowdControlType;
        int crowdControlRate;
        int autoGuardRate;
        int criRate, criDmgRate;
        int flee;
        int atkSpd = 20000;
        int moveSpd = 20000;
        int maxHp, regenHP;

        void OnEnable()
        {
            ai = target as UnitAI;
            actor = ai.GetComponent<UnitActor>();

            Initialize();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            atk = EditorGUILayout.IntField(nameof(atk), atk);
            crowdControlType = (CrowdControlType)EditorGUILayout.EnumPopup(nameof(crowdControlType), crowdControlType);
            crowdControlRate = EditorGUILayout.IntField(nameof(crowdControlRate), crowdControlRate);
            autoGuardRate = EditorGUILayout.IntField(nameof(autoGuardRate), autoGuardRate);
            criRate = EditorGUILayout.IntField(nameof(criRate), criRate);
            criDmgRate = EditorGUILayout.IntField(nameof(criDmgRate), criDmgRate);
            flee = EditorGUILayout.IntField(nameof(flee), flee);
            atkSpd = EditorGUILayout.IntField(nameof(atkSpd), atkSpd);
            moveSpd = EditorGUILayout.IntField(nameof(moveSpd), moveSpd);
            maxHp = EditorGUILayout.IntField(nameof(maxHp), maxHp);
            regenHP = EditorGUILayout.IntField(nameof(regenHP), regenHP);

            if (GUILayout.Button(nameof(ApplyAttack)))
                ApplyAttack();

            if (GUILayout.Button(nameof(ApplyCrowdControl)))
                ApplyCrowdControl();

            if (GUILayout.Button(nameof(ApplyAutoGuardRate)))
                ApplyAutoGuardRate();

            if (GUILayout.Button(nameof(ApplyCriticalRate)))
                ApplyCriticalRate();

            if (GUILayout.Button(nameof(ApplyFlee)))
                ApplyFlee();

            if (GUILayout.Button(nameof(ApplySpeed)))
                ApplySpeed();

            if (GUILayout.Button(nameof(ApplyHp)))
                ApplyHp();
        }

        private void ApplyAttack()
        {
            if (IsInvalid())
                return;

            actor.Entity.battleStatusInfo.SetTestAtk(atk);
        }

        private void ApplyCrowdControl()
        {
            if (IsInvalid())
                return;

            actor.Entity.battleStatusInfo.SetTestCrowdContorlRate(crowdControlType, crowdControlRate);
        }

        private void ApplyAutoGuardRate()
        {
            if (IsInvalid())
                return;

            actor.Entity.battleStatusInfo.SetTestAutoGuard(autoGuardRate);
        }

        private void ApplyCriticalRate()
        {
            if (IsInvalid())
                return;

            actor.Entity.battleStatusInfo.SetCriticalRate(criRate, criDmgRate);
        }

        private void ApplyFlee()
        {
            if (IsInvalid())
                return;

            actor.Entity.battleStatusInfo.SetFlee(flee);
        }

        private void ApplySpeed()
        {
            if (IsInvalid())
                return;

            actor.Entity.battleStatusInfo.SetSpeed(atkSpd, moveSpd);
        }

        private void ApplyHp()
        {
            if (IsInvalid())
                return;

            actor.Entity.battleStatusInfo.SetMaxHp(maxHp);
            actor.Entity.battleStatusInfo.SetRegenHp(regenHP);
            actor.Entity.SetHp(maxHp);
        }

        private bool IsInvalid()
        {
            return (actor == null) || (actor.Entity == null);
        }

        private void Initialize()
        {
            if (IsInvalid())
                return;

            var status = actor.Entity.battleStatusInfo;
            atk = status.MeleeAtk;
            autoGuardRate = status.AutoGuardRate;
            criRate = status.CriRate;
            criDmgRate = status.CriDmgRate;
            flee = status.Flee;
            atkSpd = status.AtkSpd;
            moveSpd = status.MoveSpd;
            maxHp = actor.Entity.CurHP;
            regenHP = status.RegenHp;
        }
    }
}