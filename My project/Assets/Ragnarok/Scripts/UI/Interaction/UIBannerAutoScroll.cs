using UnityEngine;

namespace Ragnarok
{
    public class UIBannerAutoScroll : MonoBehaviour
    {
        [SerializeField] UIScrollView scrollView;
        [SerializeField] float autoScrollDelay = 4f;


        private Transform tfWrapper;
        private UICenterOnChild centerOnChild;

        private float timer;
        private bool isDragging;

        private void Start()
        {
            centerOnChild = scrollView.GetComponentInChildren<UICenterOnChild>();
            tfWrapper = centerOnChild.transform;
            timer = 0f;
            isDragging = false;

            scrollView.onDragStarted += OnDragStart;
            scrollView.onDragFinished += onDragFinish;
        }

        private void FixedUpdate()
        {
            if (isDragging)
                return;

            timer += Time.deltaTime;

            if (timer >= autoScrollDelay)
            {
                ScrollNext();
                timer = 0f;
            }
        }

        private void OnDragStart()
        {
            timer = 0f;
            isDragging = true;
        }

        private void onDragFinish()
        {
            isDragging = false;
        }

        public void ScrollNext()
        {
            // 배너가 1개 이하 : 스크롤하지 않음.
            if (tfWrapper.childCount <= 1)
                return;

            Vector3[] corners = scrollView.panel.worldCorners;
            Vector3 panelCenter = (corners[2] + corners[0]) * 0.5f;

            // 현재 center를 찾기 
            float min = float.MaxValue;
            Transform center = null;
            for (int i = 0, imax = tfWrapper.childCount; i < imax; ++i)
            {
                Transform t = tfWrapper.GetChild(i);
                if (!t.gameObject.activeInHierarchy) continue;
                float sqrDist = Vector3.SqrMagnitude(t.position - panelCenter);

                if (sqrDist < min)
                {
                    min = sqrDist;
                    center = t;
                }
            }

            // center의 다음 오브젝트 찾기
            min = float.MaxValue;
            Transform next = null;
            for (int i = 0, imax = tfWrapper.childCount ; i < imax; ++i)
            {
                Transform t = tfWrapper.GetChild(i);
                if (!t.gameObject.activeInHierarchy) continue;
                if (t == center) continue;

                float sqrDist = Vector3.SqrMagnitude(t.position - center.position);
                if (sqrDist < min && center.position.x < t.position.x)
                {
                    min = sqrDist;
                    next = t;
                }
            }

            if (next == null)
            {
                next = tfWrapper.GetChild(0);
            }

            centerOnChild.CenterOn(next);
        }
    }
}