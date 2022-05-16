using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class SkillListView : UIView<SkillListView.IListener>, IInspectorFinder, TutorialSkillLearn.ISelectActiveSkillImpl
    {
        public interface IInfo
        {
            string GetJobIcon();
            string GetJobName();
            UISkillInfoToggle.IInfo[] GetSkills();
        }

        public interface IListener
        {
            void OnSelect(int skillId);
            void OnUnselect();
        }

        private enum ViewMode
        {
            Full = 1,
            Small,
        }

        [SerializeField] int fullHeight;
        [SerializeField] int smallHeight;
        [SerializeField] UIWidget backgroundWidget;
        [SerializeField] UIEventTrigger eventTrigger;
        [SerializeField] UIScrollView scrollView;
        [SerializeField] UIJobSkillList[] jobSkillLists;

        private ViewMode savedViewMode;
        private bool isSetParent;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(eventTrigger.onClick, OnClickedBtnBackground);

            foreach (var item in jobSkillLists)
            {
                item.OnSelectSkill += OnSelectSkill;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(eventTrigger.onClick, OnClickedBtnBackground);

            foreach (var item in jobSkillLists)
            {
                item.OnSelectSkill -= OnSelectSkill;
            }
        }

        void OnClickedBtnBackground()
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnUnselect();
            }
        }

        void OnSelectSkill(int skillId)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnSelect(skillId);
            }

            isSelectedActiveSkill = true;
        }

        protected override void OnLocalize()
        {
        }

        public void Show(IInfo[] skillInfos)
        {
            for (int i = 0; i < jobSkillLists.Length; i++)
            {
                if (i < skillInfos.Length) // Unlock
                {
                    jobSkillLists[i].ShowUnlock(skillInfos[i].GetJobIcon(), skillInfos[i].GetJobName(), skillInfos[i].GetSkills());
                }
                else
                {
                    jobSkillLists[i].ShowLock(i);
                }
            }
        }

        public void SetSelect(int selectedSkillId)
        {
            SetViewMode(selectedSkillId == 0 ? ViewMode.Full : ViewMode.Small);

            foreach (var item in jobSkillLists)
            {
                item.SetSelect(selectedSkillId);
            }
        }

        private void SetViewMode(ViewMode viewMode)
        {
            if (savedViewMode == viewMode)
                return;

            savedViewMode = viewMode;

            switch (savedViewMode)
            {
                case ViewMode.Full:
                    backgroundWidget.height = fullHeight;
                    scrollView.ResetPosition();
                    break;

                case ViewMode.Small:
                    backgroundWidget.height = smallHeight;
                    break;
            }
        }

        public void SetParent(UIPanel panel)
        {
            if (isSetParent)
                return;

            isSetParent = true;
            foreach (var item in jobSkillLists)
            {
                item.SetParent(panel);
            }
        }

        public void ResetParent()
        {
            if (!isSetParent)
                return;

            isSetParent = false;
            foreach (var item in jobSkillLists)
            {
                item.ResetParent();
            }
        }

        /// <summary>
        /// 스크롤 가능 여부 
        /// </summary>
        public void SetDraggable(bool isDraggable)
        {
            foreach (var skillList in jobSkillLists)
            {
                skillList.SetDraggable(isDraggable);
            }
        }

        bool IInspectorFinder.Find()
        {
            eventTrigger = GetComponentInChildren<UIEventTrigger>();
            jobSkillLists = GetComponentsInChildren<UIJobSkillList>();
            return true;
        }

        #region Tutorial
        bool isSelectedActiveSkill;
        void TutorialSkillLearn.ISelectActiveSkillImpl.SetTutorialMode(bool isTutorialMode)
        {
            scrollView.enabled = !isTutorialMode;
        }

        UIWidget TutorialSkillLearn.ISelectActiveSkillImpl.GetActiveSkill()
        {
            return jobSkillLists[0].GetWidget(0);
        }

        bool TutorialSkillLearn.ISelectActiveSkillImpl.IsSelectedActiveSkill()
        {
            if (isSelectedActiveSkill)
            {
                isSelectedActiveSkill = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}