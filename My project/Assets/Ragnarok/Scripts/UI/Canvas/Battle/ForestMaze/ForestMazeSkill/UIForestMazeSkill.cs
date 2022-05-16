using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIForestMazeSkill : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIForestMazeSkillElement[] elements;
        [SerializeField] UILabelHelper labelSkillName, labelSkillDescription;
        [SerializeField] UILabelHelper labelSkillList;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIForestMazeSkillElement element;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UIButtonHelper btnSelect;

        ForestMazeSkillPresenter presenter;
        SuperWrapContent<UIForestMazeSkillElement, UIForestMazeSkillElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;

        private int selectedId;

        protected override void OnInit()
        {
            presenter = new ForestMazeSkillPresenter();

            wrapContent = wrapper.Initialize<UIForestMazeSkillElement, UIForestMazeSkillElement.IInput>(element);

            foreach (var item in elements)
            {
                item.OnSelect += SelectSkill;
            }

            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            foreach (var item in elements)
            {
                item.OnSelect -= SelectSkill;
            }

            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
            selectedId = 0;
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._39605; // 처치 보상
            labelDescription.LocalKey = LocalizeKey._39606; // 보상을 선택하세요.
            labelSkillList.LocalKey = LocalizeKey._39607; // 보유 보상
            labelNoData.LocalKey = LocalizeKey._39608; // 보유한 보상이 없습니다.
            btnSelect.LocalKey = LocalizeKey._39609; // 선택하기

            UpdateSkillText();
        }

        void OnClickedBtnSelect()
        {
            OnSelect?.Invoke(selectedId);
            HideUI();
        }

        public void Show(int[] selectSkills, int[] hasSkills)
        {
            Show();

            int selectSkillCount = selectSkills == null ? 0 : selectSkills.Length;
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].SetData(i < selectSkillCount ? presenter.GetData(selectSkills[i]) : null);
            }

            wrapContent.SetData(presenter.GetData(hasSkills));
            labelNoData.SetActive(wrapContent.DataSize == 0);

            // 첫번째 스킬 선택
            if (selectSkillCount > 0)
            {
                SelectSkill(selectSkills[0]);
            }
        }

        private void SelectSkill(int id)
        {
            selectedId = id;

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].SetSelect(selectedId);
            }

            UpdateSkillText();
        }

        private void UpdateSkillText()
        {
            labelSkillName.Text = presenter.GetName(selectedId);
            labelSkillDescription.Text = presenter.GetDesc(selectedId);
        }

        private void HideUI()
        {
            Hide();
        }

        public override bool Find()
        {
            base.Find();

            elements = GetComponentsInChildren<UIForestMazeSkillElement>();

#if UNITY_EDITOR
            if (element != null)
                UnityEditor.ArrayUtility.Remove(ref elements, element);
#endif
            return true;
        }
    }
}