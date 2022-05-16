using UnityEngine;

namespace Ragnarok.View
{
    public sealed class AdventureView : UIView
    {
        [SerializeField] UIChapterSelector chapterSelector;
        [SerializeField] UIStageElement[] stages;

        public event System.Action<int> OnSelectChapter;
        public event System.Action<int> OnSelectStage;

        protected override void Awake()
        {
            base.Awake();

            chapterSelector.OnSelect += OnSelectChapterSelector;
            foreach (var item in stages)
            {
                item.OnSelectEnter += OnSelectStageEnter;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            chapterSelector.OnSelect -= OnSelectChapterSelector;
            foreach (var item in stages)
            {
                item.OnSelectEnter -= OnSelectStageEnter;
            }
        }

        protected override void OnLocalize()
        {
        }

        void OnSelectChapterSelector(int chapter)
        {
            OnSelectChapter?.Invoke(chapter);
        }

        void OnSelectStageEnter(int stageId)
        {
            OnSelectStage?.Invoke(stageId);
        }

        public void SetChapterData(UIChapterElement.IInput[] inputs)
        {
            chapterSelector.SetData(inputs);
        }

        public void InitChapterProgress()
        {
            chapterSelector.InitProgress();
        }

        public void MoveChapter(int index)
        {
            chapterSelector.Move(index);
        }

        public void SetStageData(UIStageElement.IInput[] inputs)
        {
            int length = inputs == null ? 0 : inputs.Length;
            for (int i = 0; i < stages.Length; i++)
            {
                stages[i].SetData(i < length ? inputs[i] : null);
            }
        }
    }
}