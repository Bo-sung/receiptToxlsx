using UnityEngine;

namespace Ragnarok
{
    public class UIMoveNotice : MonoBehaviour, IAutoInspectorFinder
    {
        GameObject myGameObject;
        Transform myTransform;

        [SerializeField] UIPanel panel;
        [SerializeField] UILabel label;
        [SerializeField] TweenPosition tween;
        [SerializeField] float durationRate = 0.01f;

        public event System.Action OnFinish;

        void Awake()
        {
            myGameObject = gameObject;
            myTransform = transform;
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void ShowNotice(string notice)
        {
            if (!FindPanel())
                return;

            label.text = notice;

            Vector2 viewExtents = panel.GetViewSize() * 0.5f;
            Vector2 clipSoftness = panel.clipSoftness;
            Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(myTransform);
            Vector2 extents = bounds.extents;

            viewExtents.y = 0f;
            clipSoftness.y = 0f;
            extents.y = 0f;

            tween.from = viewExtents + clipSoftness + extents;
            tween.to = -viewExtents - clipSoftness - extents;

            float dist = tween.to.x - tween.from.x;
            tween.duration = dist * durationRate;

            tween.ResetToBeginning();
            tween.PlayForward();
            tween.SetOnFinished(OnFinishedTween);
        }

        private bool FindPanel()
        {
            if (panel == null)
                panel = NGUITools.FindInParents<UIPanel>(myGameObject);

            return panel != null;
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        void OnFinishedTween()
        {
            OnFinish?.Invoke();
        }
    }
}