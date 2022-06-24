using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// NGUI를 위한 스크롤 요소를 위한 오브젝트 풀 래퍼
/// Head에서 Tail까지의 방향을 따라 정렬됨.
/// flow_direction : Head 가 향하는 방향.
/// </summary>
[RequireComponent(typeof(UIScrollView))]
public class DragWrapper : MonoBehaviour
{
    public class Item
    {
        public GameObject go;
        public int dataindex;
        public int curSize;
    }

    private enum FlowDirection
    {
        BottomUP,
        UpBottom,
        LeftToRight,
        RightToLeft
    }

    private enum Pivot
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,
    }

    private List<Item> objectList;    //전체 오브젝트 리스트    
    private Queue<Item> intrimQueue;    //임시 오브젝트 큐.. 기존 풀에서 부족한 경우 끌어다 사용
    private Bounds area;
    private UIScrollView scrollView;

    [SerializeField] private GameObject prefab;
    [SerializeField] private int scrollIndexHead = 0;
    [SerializeField] private int scrollIndexTail = 0;
    [SerializeField] private int dataIndexHead = 0;
    [SerializeField] private int dataIndexTail = 0;

    [SerializeField] int maxSize;
    [SerializeField] Collider2D DragArea;
    [SerializeField] FlowDirection flow_direction = FlowDirection.UpBottom;
    [SerializeField] Pivot pivot;

    public delegate void OnItemCallback(GameObject go, int index);
    public OnItemCallback Onload;
    public OnItemCallback OnItemsClicked;

    private int dataIndexMax;
    public int DataIndexMax
    {
        get
        {
            return dataIndexMax;
        }
        set
        {
            dataIndexMax = value;
            if (dataIndexMax < maxSize)
                Sort();
        }
    }

    private bool isInit;
    private bool is2D;

    void Awake()
    {
        objectList = new List<Item>();
        intrimQueue = new Queue<Item>();
        scrollView = GetComponent<UIScrollView>();
    }

    private void LateUpdate()
    {
        if (isInit)
            CheckBounds();
    }

    Vector2 GetStartPositionFromPivot(Pivot pivot)
    {
        Vector2 result;
        Vector2 viewArea = scrollView.panel.GetViewSize();
        Bounds ViewBound = new Bounds(scrollView.transform.position, viewArea);
        switch (pivot)
        {
            case Pivot.Top: { result = new Vector2(ViewBound.center.x, ViewBound.max.y); } break;
            case Pivot.TopLeft: { result = new Vector2(ViewBound.min.x, ViewBound.max.y); } break;
            case Pivot.TopRight: { result = new Vector2(ViewBound.max.x, ViewBound.max.y); } break;
            case Pivot.Center: { result = new Vector2(ViewBound.center.x, ViewBound.center.y); } break;
            case Pivot.Left: { result = new Vector2(ViewBound.min.x, ViewBound.center.y); } break;
            case Pivot.Right: { result = new Vector2(ViewBound.max.x, ViewBound.center.y); } break;
            case Pivot.Bottom: { result = new Vector2(ViewBound.center.x, ViewBound.min.y); } break;
            case Pivot.BottomLeft: { result = new Vector2(ViewBound.min.x, ViewBound.min.y); } break;
            case Pivot.BottomRight: { result = new Vector2(ViewBound.max.x, ViewBound.min.y); } break;
            default: { result = Vector2.zero; } break;
        }
        return result;
    }

    //최초 1회 소팅
    private void Sort()
    {
        Onload(objectList[0].go, 0);
        objectList[0].go.SetActive(true);
        MoveToTargetPosition(0, GetStartPositionFromPivot(pivot), pivot, isReverse: true);
        scrollIndexTail = 0;

        if (dataIndexMax == 0)
            objectList[0].go.SetActive(false);

        for (int i = 1; i <= Mathf.Min(maxSize, dataIndexMax); ++i)
        {
            Onload(objectList[i].go, i);

            objectList[i].go.SetActive(true);
            switch (flow_direction)
            {
                case FlowDirection.BottomUP: { MoveToTargetPosition(i, scrollIndexTail++, Pivot.Top); } break;
                case FlowDirection.UpBottom: { MoveToTargetPosition(i, scrollIndexTail++, Pivot.Bottom); } break;
                case FlowDirection.LeftToRight: { MoveToTargetPosition(i, scrollIndexTail++, Pivot.Right); } break;
                case FlowDirection.RightToLeft: { MoveToTargetPosition(i, scrollIndexTail++, Pivot.Left); } break;
            }
        }

        dataIndexHead = scrollIndexHead;
        dataIndexTail = scrollIndexTail;

        isInit = true;
    }

    /// <summary>
    /// 데이터 인덱스 불러오기
    /// </summary>
    private int GetDataindex(bool IsHead)
    {
        //isHead == true 일떄 Head => Tail로 감
        if (IsHead)
        {
            dataIndexHead--;
            dataIndexTail--;

            if (dataIndexTail >= dataIndexMax)
                dataIndexTail = dataIndexMax;

            if (dataIndexHead <= 0)
                dataIndexHead = 0;

            if (dataIndexTail <= 0)
                dataIndexTail = 0;

            if (dataIndexTail <= dataIndexHead)
                dataIndexTail = dataIndexHead;

            return dataIndexHead;
        }
        else
        {
            dataIndexHead++;
            dataIndexTail++;

            if (dataIndexTail >= dataIndexMax)
                dataIndexTail = dataIndexMax;

            if (dataIndexHead >= dataIndexMax)
                dataIndexHead = dataIndexMax;

            if (dataIndexHead >= dataIndexTail)
                dataIndexHead = dataIndexTail;

            return dataIndexTail;
        }
    }
    [ContextMenu("Reset To Start")]
    public void ResetToStart()
    {
        // scrollView.SetDragAmount(0, 0, true);
        SetScrollPosition(0, 0);
    }

    [ContextMenu("Reset To Head")]
    public void ResetToHead()
    {
        SetScrollPosition(objectList[scrollIndexHead].go.transform.localPosition + (Vector3)GetStartPositionFromPivot(pivot));
    }

    [ContextMenu("Reset To Tail")]
    public void ResetToTail()
    {
        SetScrollPosition(objectList[scrollIndexTail].go.transform.localPosition + (Vector3)GetStartPositionFromPivot(pivot));
    }

    public void SetScrollPosition(int x, int y)
    {
        SetScrollPosition(new Vector2(x, y));
    }

    public void SetScrollPosition(Vector2 Position)
    {
        Position -= (Vector2)scrollView.transform.localPosition;
        scrollView.MoveRelative(Position);
    }

    //바운더리 체크 후 그에 맞는 로직 실행.
    private void CheckBounds()
    {
        if (dataIndexHead == 0 && scrollIndexHead == 0)
        {
            if (dataIndexHead == dataIndexTail)
                return;
            objectList[0].go.SetActive(true);
        }

        area = DragArea.bounds;
        for (int i = 0; i < objectList.Count; ++i)
        {
            Vector3 position = objectList[i].go.transform.position;
            Bounds bounds;
            if (is2D)
                bounds = objectList[i].go.GetComponent<Collider2D>().bounds;
            else
                bounds = objectList[i].go.GetComponent<Collider>().bounds;
            if (area.Contains(position))
            {
                //in the area;
                //no needs to refresh
                continue;
            }
            else if (area.Intersects(bounds))
            {
                //in the area;
                //no needs to refresh
                continue;
            }
            else
            {
                //outside of area

                //serch ClosestPoint for getting direction
                Vector3 nearpoint = area.ClosestPoint(position);
                //!!!주의점 MoveToTargetPosition은 타겟과 자신의 크기와 중심점 기준으로 이동하기 떄문에 OnLoad가 실행 된 이후 실행해줘야함. 안그러면 서로 겹치는 문제 발생 가능.

                switch (flow_direction)
                {
                    case FlowDirection.LeftToRight:
                        {
                            if (area.center.x > nearpoint.x)
                            {
                                //it's on Left

                                //만약 현재값이 최대값이거나 최소값인경우 스킵
                                if (dataIndexTail >= dataIndexMax)
                                    continue;
                                //단일 오브젝트 리프레시 및 초기화.
                                objectList[scrollIndexHead].dataindex = GetDataindex(false);
                                Onload(objectList[scrollIndexHead].go, objectList[scrollIndexHead].dataindex);
                                //타겟 포지션으로 요소 이동.
                                MoveToTargetPosition(scrollIndexHead, scrollIndexTail, Pivot.Right);
                                if (!area.Contains(objectList[scrollIndexHead].go.transform.position))
                                {
                                    //이동한 헤드가 범위 밖임
                                    objectList[scrollIndexHead].go.SetActive(false);
                                }
                                scrollIndexTail = scrollIndexHead;
                                if (scrollIndexHead == objectList.Count - 1)
                                    scrollIndexHead = -1;
                                scrollIndexHead++;
                            }
                            else
                            {
                                //it's On Right

                                //만약 현재값이 최대값이거나 최소값인경우 스킵
                                if (dataIndexHead <= 0)
                                    continue;
                                //단일 오브젝트 리프레시.
                                objectList[scrollIndexTail].dataindex = GetDataindex(true);
                                Onload(objectList[scrollIndexTail].go, objectList[scrollIndexTail].dataindex);
                                //타겟 포지션으로 요소 꺼내와서 이동
                                MoveToTargetPosition(scrollIndexTail, scrollIndexHead, Pivot.Left);
                                if (area.Contains(objectList[scrollIndexHead].go.transform.position))
                                {
                                    //이동한 테일이 범위 밖임
                                    objectList[scrollIndexTail].go.SetActive(false);
                                }
                                scrollIndexHead = scrollIndexTail;
                                if (scrollIndexTail == 0)
                                    scrollIndexTail = objectList.Count;
                                scrollIndexTail--;
                            }
                        }
                        break;
                    case FlowDirection.RightToLeft:
                        {
                            if (area.center.x > nearpoint.x)
                            {
                                //it's on Left

                                //만약 현재값이 최대값이거나 최소값인경우 스킵
                                if (dataIndexHead <= 0)
                                    continue;
                                //단일 오브젝트 리프레시 및 초기화.
                                objectList[scrollIndexTail].dataindex = GetDataindex(true);
                                Onload(objectList[scrollIndexTail].go, objectList[scrollIndexTail].dataindex);
                                //타겟 포지션으로 요소 꺼내와서 이동
                                MoveToTargetPosition(scrollIndexHead, scrollIndexTail, Pivot.Right);
                                if (area.Contains(objectList[scrollIndexHead].go.transform.position))
                                {
                                    //이동한 헤드가 범위 밖임
                                    objectList[scrollIndexHead].go.SetActive(false);
                                }
                                scrollIndexHead = scrollIndexTail;
                                if (scrollIndexTail == 0)
                                    scrollIndexTail = objectList.Count;
                                scrollIndexTail--;

                            }
                            else
                            {
                                //it's On Right

                                //만약 현재값이 최대값이거나 최소값인경우 스킵
                                if (dataIndexTail >= dataIndexMax)
                                    continue;
                                //단일 오브젝트 리프레시.
                                objectList[scrollIndexHead].dataindex = GetDataindex(false);
                                Onload(objectList[scrollIndexHead].go, objectList[scrollIndexHead].dataindex);
                                //타겟 포지션으로 요소 이동.
                                MoveToTargetPosition(scrollIndexTail, scrollIndexHead, Pivot.Left);
                                if (area.Contains(objectList[scrollIndexHead].go.transform.position))
                                {
                                    //이동한 테일이 범위 밖임
                                    objectList[scrollIndexTail].go.SetActive(false);
                                }
                                scrollIndexTail = scrollIndexHead;
                                if (scrollIndexHead == objectList.Count - 1)
                                    scrollIndexHead = -1;
                                scrollIndexHead++;
                            }
                        }
                        break;
                    case FlowDirection.BottomUP:
                        {
                            if (area.center.y > nearpoint.y)
                            {
                                //it's on Bottom

                                //만약 현재값이 최대값이거나 최소값인경우 스킵
                                if (dataIndexTail >= dataIndexMax)
                                    continue;
                                //단일 오브젝트 리프레시 및 초기화.
                                objectList[scrollIndexHead].dataindex = GetDataindex(false);
                                Onload(objectList[scrollIndexHead].go, objectList[scrollIndexHead].dataindex);
                                //타겟 포지션으로 요소 이동.
                                MoveToTargetPosition(scrollIndexHead, scrollIndexTail, Pivot.Top);
                                if (area.Contains(objectList[scrollIndexHead].go.transform.position))
                                {
                                    //이동한 헤드가 범위 밖임
                                    objectList[scrollIndexHead].go.SetActive(false);
                                }
                                scrollIndexTail = scrollIndexHead;
                                if (scrollIndexHead == objectList.Count - 1)
                                    scrollIndexHead = -1;
                                scrollIndexHead++;
                            }
                            else
                            {
                                //it's On Top

                                //만약 현재값이 최대값이거나 최소값인경우 스킵
                                if (dataIndexHead <= 0)
                                    continue;
                                //단일 오브젝트 리프레시.
                                objectList[scrollIndexTail].dataindex = GetDataindex(true);
                                Onload(objectList[scrollIndexTail].go, objectList[scrollIndexTail].dataindex);
                                //타겟 포지션으로 요소 꺼내와서 이동
                                MoveToTargetPosition(scrollIndexTail, scrollIndexHead, Pivot.Bottom);
                                if (area.Contains(objectList[scrollIndexHead].go.transform.position))
                                {
                                    //이동한 테일이 범위 밖임
                                    objectList[scrollIndexTail].go.SetActive(false);
                                }
                                scrollIndexHead = scrollIndexTail;
                                if (scrollIndexTail == 0)
                                    scrollIndexTail = objectList.Count;
                                scrollIndexTail--;
                            }
                        }
                        break;
                    case FlowDirection.UpBottom:
                        {
                            if (area.center.y > nearpoint.y)
                            {
                                //it's on Bottom

                                //만약 현재값이 최대값이거나 최소값인경우 스킵
                                if (dataIndexHead <= 0)
                                    continue;
                                //단일 오브젝트 리프레시 및 초기화..
                                objectList[scrollIndexTail].dataindex = GetDataindex(true);
                                Onload(objectList[scrollIndexTail].go, objectList[scrollIndexTail].dataindex);
                                //타겟 포지션으로 요소 이동.
                                MoveToTargetPosition(scrollIndexTail, scrollIndexHead, Pivot.Top);
                                if (area.Contains(objectList[scrollIndexHead].go.transform.position))
                                {
                                    //이동한 테일이 범위 밖임
                                    objectList[scrollIndexTail].go.SetActive(false);
                                }
                                scrollIndexHead = scrollIndexTail;
                                if (scrollIndexTail == 0)
                                    scrollIndexTail = objectList.Count;
                                scrollIndexTail--;
                            }
                            else
                            {
                                //it's On Top

                                //만약 현재값이 최대값이거나 최소값인경우 스킵
                                if (dataIndexTail >= dataIndexMax)
                                    continue;
                                //단일 오브젝트 리프레시.
                                objectList[scrollIndexHead].dataindex = GetDataindex(false);
                                Onload(objectList[scrollIndexHead].go, objectList[scrollIndexHead].dataindex);
                                //타겟 포지션으로 요소 꺼내와서 이동
                                MoveToTargetPosition(scrollIndexHead, scrollIndexTail, Pivot.Bottom);
                                if (area.Contains(objectList[scrollIndexHead].go.transform.position))
                                {
                                    //이동한 헤드가 범위 밖임
                                    objectList[scrollIndexHead].go.SetActive(false);
                                }
                                scrollIndexTail = scrollIndexHead;
                                if (scrollIndexHead == objectList.Count - 1)
                                    scrollIndexHead = -1;
                                scrollIndexHead++;
                            }
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 타겟의 특정 방향에 요소 이동
    /// </summary>
    private void MoveToTargetPosition(int index, int targetindex, Pivot direction)
    {
        if (index <= -1 || index >= objectList.Count)
            return;

        if (targetindex <= -1 || targetindex >= objectList.Count)
            return;

        GameObject obj = objectList[index].go;
        GameObject targetobj = objectList[targetindex].go;

        targetobj.SetActive(true);
        obj.SetActive(true);
        Bounds objbounds;
        Bounds target;
        if (is2D)
        {
            objbounds = obj.GetComponent<Collider2D>().bounds;
            target = targetobj.GetComponent<Collider2D>().bounds;
        }
        else
        {
            objbounds = obj.GetComponent<Collider>().bounds;
            target = targetobj.GetComponent<Collider>().bounds;
        }

        Vector3 targetPosition;
        switch (direction)
        {
            case Pivot.Top: { targetPosition = targetobj.transform.position + (target.extents.y + objbounds.extents.y) * Vector3.up; } break;
            case Pivot.Bottom: { targetPosition = targetobj.transform.position + (target.extents.y + objbounds.extents.y) * Vector3.down; } break;
            case Pivot.Left: { targetPosition = targetobj.transform.position + (target.extents.x + objbounds.extents.x) * Vector3.left; } break;
            case Pivot.Right: { targetPosition = targetobj.transform.position + (target.extents.x + objbounds.extents.x) * Vector3.right; } break;
            default: { targetPosition = obj.transform.position; } break;
        }

        obj.transform.position = targetPosition;
    }

    /// <summary>
    /// 아이템을 해당 인덱스요소의 지정위치로 이동 후 object리스트에 추가.
    /// </summary>
    private void MoveToTargetPosition(Item recycled, int targetindex, Pivot direction)
    {
        GameObject obj = recycled.go;
        obj.SetActive(true);
        GameObject targetobj = objectList[targetindex].go;

        Bounds objbounds;
        Bounds target;
        if (is2D)
        {
            objbounds = obj.GetComponent<Collider2D>().bounds;
            target = targetobj.GetComponent<Collider2D>().bounds;
        }
        else
        {
            objbounds = obj.GetComponent<Collider>().bounds;
            target = targetobj.GetComponent<Collider>().bounds;
        }
        switch (direction)
        {
            case Pivot.Top: { obj.transform.localPosition = target.center + (target.extents.y * Vector3.up) + (objbounds.max.y * Vector3.down); } break;
            case Pivot.Bottom: { obj.transform.localPosition = target.center + (target.extents.y * Vector3.down) + (objbounds.min.y * Vector3.up); } break;
            case Pivot.Left: { obj.transform.localPosition = target.center + (target.extents.x * Vector3.left) + (objbounds.max.x * Vector3.right); } break;
            case Pivot.Right: { obj.transform.localPosition = target.center + (target.extents.x * Vector3.right) + (objbounds.min.x * Vector3.left); } break;
        }
        objectList.Add(recycled);
    }

    private void MoveToTargetPosition(int index, Vector2 targetPos, Pivot direction, bool isReverse = false)
    {

        if (index <= -1 || index >= objectList.Count)
            return;
        if (targetPos == Vector2.zero)
            return;

        GameObject obj = objectList[index].go;

        obj.SetActive(true);

        Bounds objbounds;
        if (is2D)
            objbounds = obj.GetComponent<Collider2D>().bounds;
        else
            objbounds = obj.GetComponent<Collider>().bounds;

        Vector3 targetPosition;

        if (isReverse)
        {
            switch (direction)
            {
                case Pivot.Top: { direction = Pivot.Bottom; } break;
                case Pivot.Bottom: { direction = Pivot.Top; } break;
                case Pivot.Left: { direction = Pivot.Right; } break;
                case Pivot.Right: { direction = Pivot.Left; } break;
                case Pivot.TopLeft: { direction = Pivot.BottomRight; } break;
                case Pivot.TopRight: { direction = Pivot.BottomLeft; } break;
                case Pivot.BottomLeft: { direction = Pivot.TopRight; } break;
                case Pivot.BottomRight: { direction = Pivot.TopLeft; } break;
            }
        }


        switch (direction)
        {
            case Pivot.Bottom: { targetPosition = (objbounds.extents.y * Vector3.down); } break;
            case Pivot.Top: { targetPosition = (objbounds.extents.y * Vector3.up); } break;
            case Pivot.Right: { targetPosition = (objbounds.extents.x * Vector3.right); } break;
            case Pivot.Left: { targetPosition = (objbounds.extents.x * Vector3.left); } break;
            case Pivot.TopLeft: { targetPosition = (objbounds.extents.y * Vector3.up) + (objbounds.extents.x * Vector3.left); } break;
            case Pivot.TopRight: { targetPosition = (objbounds.extents.y * Vector3.up) + (objbounds.extents.x * Vector3.right); } break;
            case Pivot.BottomLeft: { targetPosition = (objbounds.extents.y * Vector3.down) + (objbounds.extents.x * Vector3.left); } break;
            case Pivot.BottomRight: { targetPosition = (objbounds.extents.y * Vector3.down) + (objbounds.extents.x * Vector3.right); } break;
            case Pivot.Center: { targetPosition = targetPos; } break;
            default: { targetPosition = obj.transform.position; } break;
        }

        obj.transform.localPosition = (Vector3)targetPos;
        obj.transform.position += targetPosition;
    }
    private void OnClickItem(GameObject go)
    {
        if (OnItemsClicked == null)
            return;

        for (int i = 0; i < objectList.Count; ++i)
        {
            if (objectList[i].go == go)
            {
                OnItemsClicked(go, objectList[i].dataindex);
            }
        }
    }

    private void OnItemLoaded(GameObject go)
    {
        if (Onload == null)
            return;

        for (int i = 0; i < objectList.Count; ++i)
        {
            if (objectList[i].go == go)
            {
                Onload(go, objectList[i].dataindex);

                switch (flow_direction)
                {
                    case FlowDirection.BottomUP:
                    case FlowDirection.UpBottom:
                        {
                            objectList[i].curSize = go.GetComponent<UIWidget>().height;
                        }
                        break;
                    case FlowDirection.RightToLeft:
                    case FlowDirection.LeftToRight:
                        {
                            objectList[i].curSize = go.GetComponent<UIWidget>().width;
                        }
                        break;
                }
            }
        }
    }
    public void SetOnLoadCallback(OnItemCallback action)
    {
        this.Onload = action;
    }

    public void InitItems(GameObject prefab, int dataSize)
    {
        dataIndexMax = dataSize;
        InitItems(prefab);
        Sort();
    }

    public void InitItems(GameObject prefab, int dataSize, int MaxCount)
    {
        InitItems(prefab, dataSize);
        maxSize = MaxCount;
        Sort();
    }

    private void InitItems(GameObject prefab)
    {
        this.prefab = prefab;
        area = DragArea.bounds;

        prefab.SetActive(true);
        if (prefab.GetComponent<Collider>() == null)
            is2D = true;

        if (is2D)
            maxSize = CalculateFitInArea(prefab.GetComponent<Collider2D>().bounds, flow_direction);
        else
            maxSize = CalculateFitInArea(prefab.GetComponent<Collider>().bounds, flow_direction);

        prefab.SetActive(false);
        maxSize = (int)(maxSize * 1.5f);

        //크기 변화에 따른 표시 가능 갯수 변화 감안하여 1.5배수로 미리 생성
        for (int i = 0; i < maxSize; ++i)
        {
            GameObject obj = transform.AddChild(prefab);
            obj.AddComponent<UIDragScrollView>().scrollView = GetComponent<UIScrollView>();

            obj.name = prefab.name + (i).ToString();
            obj.SetActive(false);
            if (obj.GetComponent<Collider>() != null || obj.GetComponent<Collider2D>() != null)
            {
                UIEventListener.Get(obj).onClick = OnClickItem;
            }

            objectList.Add(new Item() { go = obj, dataindex = i });
        }
    }

    private int CalculateFitInArea(Bounds bounds, FlowDirection direction)
    {
        int count = 0;
        switch (direction)
        {
            case FlowDirection.BottomUP:
            case FlowDirection.UpBottom:
                {
                    count = (int)(area.extents.y / bounds.extents.y);
                }
                break;
            case FlowDirection.RightToLeft:
            case FlowDirection.LeftToRight:
                {
                    count = (int)(area.extents.x / bounds.extents.x);
                }
                break;
        }
        return count + 2;
    }
    private int CalculateFitInArea()
    {
        int size = 0;
        foreach (var i in objectList)
        {
            size += i.curSize;
        }
        switch (flow_direction)
        {
            case FlowDirection.BottomUP:
            case FlowDirection.UpBottom:
                {
                    size = (int)(area.extents.y / size);
                }
                break;
            case FlowDirection.RightToLeft:
            case FlowDirection.LeftToRight:
                {
                    size = (int)(area.extents.x / size);
                }
                break;
        }
        return size;
    }
}

