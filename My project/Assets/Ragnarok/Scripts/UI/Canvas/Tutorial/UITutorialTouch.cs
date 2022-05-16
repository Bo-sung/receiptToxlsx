using UnityEngine;

namespace Ragnarok
{
    public class UITutorialTouch : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIWidget mask, fingerAnchor;
        [SerializeField] GameObject colliders;
        [SerializeField] PropertyBinding maskBinding;
        [SerializeField] PropertyBinding fingerBinding;
        [SerializeField] GameObject finger;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void Show(UIWidget maskArea, UIWidget fingerArea, bool isActiveCollider, bool isActiveFinger)
        {
            // mask
            if (maskArea == null)
            {
                mask.cachedGameObject.SetActive(false);
            }
            else
            {
                mask.cachedGameObject.SetActive(true);
                maskArea.ResetAndUpdateAnchors();

                //mask.cachedTransform.position = maskArea.cachedTransform.position;
                maskBinding.source.Set(maskArea.cachedTransform, "position");

                //	Pivot에 따라 좌표 교정
                Vector3 newPos = mask.cachedTransform.localPosition;

                bool isTop = maskArea.pivot == UIWidget.Pivot.Top || maskArea.pivot == UIWidget.Pivot.TopLeft || maskArea.pivot == UIWidget.Pivot.TopRight;
                bool isBottom = maskArea.pivot == UIWidget.Pivot.Bottom || maskArea.pivot == UIWidget.Pivot.BottomLeft || maskArea.pivot == UIWidget.Pivot.BottomRight;
                bool isLeft = maskArea.pivot == UIWidget.Pivot.Left || maskArea.pivot == UIWidget.Pivot.TopLeft || maskArea.pivot == UIWidget.Pivot.BottomLeft;
                bool isRight = maskArea.pivot == UIWidget.Pivot.Right || maskArea.pivot == UIWidget.Pivot.TopRight || maskArea.pivot == UIWidget.Pivot.BottomRight;

                if (isTop) newPos.y -= maskArea.height / 2;
                else if (isBottom) newPos.y += maskArea.height / 2;
                if (isRight) newPos.x -= maskArea.width / 2;
                else if (isLeft) newPos.x += maskArea.width / 2;

                mask.cachedTransform.localPosition = newPos;


                mask.width = maskArea.width;
                mask.height = maskArea.height;
            }

            // finger
            if (fingerArea == null)
            {
                fingerAnchor.cachedGameObject.SetActive(false);
            }
            else
            {
                fingerAnchor.cachedGameObject.SetActive(true);
                fingerAnchor.ResetAndUpdateAnchors();

                //fingerAnchor.cachedTransform.position = fingerArea.cachedTransform.position;
                fingerBinding.source.Set(fingerArea.cachedTransform, "position");

                //	Pivot에 따라 좌표 교정
                Vector3 newPos = fingerAnchor.cachedTransform.localPosition;

                bool isTop = fingerArea.pivot == UIWidget.Pivot.Top || fingerArea.pivot == UIWidget.Pivot.TopLeft || fingerArea.pivot == UIWidget.Pivot.TopRight;
                bool isBottom = fingerArea.pivot == UIWidget.Pivot.Bottom || fingerArea.pivot == UIWidget.Pivot.BottomLeft || fingerArea.pivot == UIWidget.Pivot.BottomRight;
                bool isLeft = fingerArea.pivot == UIWidget.Pivot.Left || fingerArea.pivot == UIWidget.Pivot.TopLeft || fingerArea.pivot == UIWidget.Pivot.BottomLeft;
                bool isRight = fingerArea.pivot == UIWidget.Pivot.Right || fingerArea.pivot == UIWidget.Pivot.TopRight || fingerArea.pivot == UIWidget.Pivot.BottomRight;

                if (isTop) newPos.y -= fingerArea.height / 2;
                else if (isBottom) newPos.y += fingerArea.height / 2;
                if (isRight) newPos.x -= fingerArea.width / 2;
                else if (isLeft) newPos.x += fingerArea.width / 2;

                fingerAnchor.cachedTransform.localPosition = newPos;

                fingerAnchor.width = fingerArea.width;
                fingerAnchor.height = fingerArea.height;
            }

            // colliders
            SetActiveCollider(isActiveCollider);

            NGUITools.SetActive(finger, isActiveFinger);

            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        private void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }

        public void SetActiveCollider(bool isActiveCollider)
        {
            colliders.SetActive(isActiveCollider);
        }
    }
}