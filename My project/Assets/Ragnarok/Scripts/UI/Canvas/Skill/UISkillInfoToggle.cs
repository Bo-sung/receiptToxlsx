using System.Collections;
using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class UISkillInfoToggle : UISkillInfoToggleSimple<UISkillInfoToggle.IInfo>, IInspectorFinder
    {
        public interface IInfo : UISkillInfoSelect.IInfo
        {
            int GetSkillLevel();
        }

        [SerializeField] UISprite iconOutline;
        [SerializeField] UILabelHelper labelSkillLv;
        [SerializeField] GameObject lockBase;
        [SerializeField] UIInsideOnChild insideChild;
        [SerializeField] UITweener selectTweener;

        private Transform myTransform;
        private Transform savedParent;
        private bool isDirtyParent;
        private int savedSelectedSkillId;
        private UIDragScrollView uiDragScrollView;

        protected override void Awake()
        {
            base.Awake();

            myTransform = transform;
            savedParent = myTransform.parent;
            uiDragScrollView = GetComponent<UIDragScrollView>();
        }

        void Start()
        {
            StopRotationAni(); // Rotation 애니메이션 종료
        }

        public override bool SetSelect(int selectedSkillId)
        {
            bool isSelect = base.SetSelect(selectedSkillId);

            if (isSelect)
            {
                if (savedSelectedSkillId != selectedSkillId)
                {
                    savedSelectedSkillId = selectedSkillId;
                    StartCoroutine(nameof(YieldInsideChild));
                }
            }
            else
            {
                savedSelectedSkillId = 0;
            }

            return isSelect;
        }

        public void SetParent(UIPanel panel)
        {
            if (info == null)
                return;

            // 장착 불가능한 스킬
            switch (info.SkillType)
            {
                case SkillType.Passive:
                case SkillType.BasicActiveSkill:
                case SkillType.Plagiarism:
                case SkillType.Reproduce:
                case SkillType.SummonBall:
                case SkillType.RuneMastery:
                    return;
            }

            if (info.GetSkillLevel() == 0)
                return;

            myTransform.SetParent(panel.cachedTransform);
            PlayRotationAni(); // Rotation 애니메이션 시작
            NGUITools.MarkParentAsChanged(myGameObject);

            isDirtyParent = true;
        }

        public void ResetParent()
        {
            if (!isDirtyParent)
                return;

            myTransform.SetParent(savedParent);
            StopRotationAni(); // Rotation 애니메이션 종료
            NGUITools.MarkParentAsChanged(myGameObject);
        }

        protected override void Refresh(bool isAsync)
        {
            base.Refresh(isAsync);

            if (info == null)
                return;

            int skillLevel = info.GetSkillLevel();
            bool isLock = skillLevel == 0; // 아직 배우지 않은 스킬
            iconOutline.spriteName = isLock ? "Ui_Common_BG_SkillFrame_Off" : "Ui_Common_BG_SkillFrame_On";
            labelSkillLv.Text = LocalizeKey._39005.ToText() // Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, skillLevel);
            lockBase.SetActive(isLock);
        }

        public void SetDraggable(bool isDraggable)
        {
            uiDragScrollView.enabled = isDraggable;
        }

        private void PlayRotationAni()
        {
            selectTweener.PlayForward();
        }

        private void StopRotationAni()
        {
            selectTweener.Finish();
            myTransform.localRotation = Quaternion.identity;
        }

        IEnumerator YieldInsideChild()
        {
            yield return Awaiters.EndOfFrame;
            insideChild.Run();
        }

        bool IInspectorFinder.Find()
        {
            insideChild = GetComponent<UIInsideOnChild>();
            selectTweener = GetComponent<TweenRotation>();
            return true;
        }
    }
}