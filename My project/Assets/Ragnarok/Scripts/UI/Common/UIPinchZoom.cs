using UnityEngine;

public class UIPinchZoom : MonoBehaviour
{
    [SerializeField] float minZoom = 0.7f;
    [SerializeField] float maxZoom = 1.5f;
    [SerializeField] float zoomSpeed = 0.1f;
    [SerializeField] float zoomLerpSpeed = 10f;
    float currentZoom = 1;
    public float CurrentZoom => currentZoom;
    bool isPinching = false;
    float mouseWheelSensitivity = 1;

    float savedZoom = 0;

#if UNITY_EDITOR
    Vector2 savedLastClickPos;
#endif

    [SerializeField] Transform target;
    [SerializeField] UIWidget pivot;
    [SerializeField] UIScrollView scrollView;

    public delegate void PinchZoomEvent(float deltaZoom);

    public event PinchZoomEvent OnPinchZoom;


    private void OnEnable()
    {
        Input.multiTouchEnabled = true;
    }

    private void OnDisable()
    {
        Input.multiTouchEnabled = false;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(1))
#else
        if (Input.touchCount == 2)
#endif
        {
            if (!isPinching)
            {
                isPinching = true;
                savedZoom = 0f;
            }
            OnPinch();
        }
        else
        {
            if (isPinching) // 핀치줌을 막 끝낸 상태면 얼마나 줌했는지 이벤트 발생
            {
                OnPinchZoom?.Invoke(savedZoom);
#if UNITY_EDITOR
                savedLastClickPos = default;
#endif
            }
            isPinching = false;
        }

        #region PC Input

        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollWheelInput) > float.Epsilon)
        {
            currentZoom *= 1 + scrollWheelInput * mouseWheelSensitivity;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }

        #endregion

        if (scrollView != null)
        {
            scrollView.customMovement = isPinching ? Vector2.zero : Vector2.one;
        }

        if (target)
        {
            if (Mathf.Abs(target.localScale.x - currentZoom) > 0.001f)
            {
                var scale = Vector3.Lerp(target.localScale, Vector3.one * currentZoom, zoomLerpSpeed * Time.deltaTime);
                ScaleAround(target, pivot.transform, scale);

                if (scrollView != null)
                {
                    scrollView.InvalidateBounds();
                    scrollView.MoveAbsolute(Vector3.zero);
                    scrollView.RestrictWithinBounds(true);
                }
            }
        }
    }

    void OnPinch()
    {
#if UNITY_EDITOR
        float prevTouchDeltaMag = ((savedLastClickPos == default ? (Vector2)Input.mousePosition : savedLastClickPos)).magnitude;
        float touchDeltaMag = (Input.mousePosition).magnitude;

        savedLastClickPos = Input.mousePosition;
#else
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
#endif

        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        currentZoom -= deltaMagnitudeDiff * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        savedZoom -= deltaMagnitudeDiff;
    }

    void ScaleAround(Transform target, Transform pivot, Vector3 scale)
    {
        Transform pivotParent = pivot.parent;
        Vector3 pivotPos = pivot.position;
        pivot.parent = target;
        target.localScale = scale;
        target.position += pivotPos - pivot.position;
        pivot.parent = pivotParent;
    }

    public void SetZoom(float zoom, bool isImmediately = false)
    {
        currentZoom = zoom;
        if (isImmediately)
        {
            ScaleAround(target, pivot.transform, Vector3.one * currentZoom);

            if (scrollView != null)
            {
                scrollView.InvalidateBounds();
                scrollView.MoveAbsolute(Vector3.zero);
                scrollView.RestrictWithinBounds(true);
            }
        }
    }

    public float ClampMinMaxZoom(float zoom)
    {
        return Mathf.Clamp(zoom, minZoom, maxZoom);
    }
}
