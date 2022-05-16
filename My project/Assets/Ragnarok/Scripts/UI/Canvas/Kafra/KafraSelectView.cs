using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class KafraSelectView : UIView
    {
        [SerializeField] private UILabelHelper labelQuest;
        [SerializeField] private KafraSelectElement selectRoPoint;
        [SerializeField] private KafraSelectElement selectZeny;

        public event Action<KafraType> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            selectRoPoint.OnSelect += OnSelectKafra;
            selectZeny.OnSelect += OnSelectKafra;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            selectRoPoint.OnSelect -= OnSelectKafra;
            selectZeny.OnSelect -= OnSelectKafra;
        }

        protected override void OnLocalize()
        {
            labelQuest.LocalKey = LocalizeKey._19101; // 퀘스트
        }

        public override void Show()
        {
            base.Show();
            selectRoPoint.Show();
            selectZeny.Show();
        }

        void OnSelectKafra(KafraType kafraType)
        {
            OnSelect?.Invoke(kafraType);
        }
    }
}