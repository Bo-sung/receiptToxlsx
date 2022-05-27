using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatWrapper : MonoBehaviour
{
    public struct Item
    {
        public GameObject go;
        public int poolIndex;
        public System.Action<GameObject, int> updateAction;
    }

    private List<Item> objectList;    //전체 오브젝트 리스트    
    private Queue<Item> recycleQueue;
    private Bounds area;
    [SerializeField]private int scrollHeadindex = 0, scrollTailindex = 0;
    [SerializeField]private int currentHeadDataIndex = 0, currentTailDataIndex = 0;

    [SerializeField] int MaxSize;
    [SerializeField] Collider2D DragArea;
    [SerializeField] Arrangement scrollArrangement;
    [SerializeField] FlowDirection flow_direction = FlowDirection.UpBottom;
    public System.Action<GameObject, int> Onload;

    public int DataIndexMax { get; set; }

    public void Awake()
    {
        objectList = new List<Item>();
        recycleQueue = new Queue<Item>();
    }

    private void FixedUpdate()
    {
    }
    private void LateUpdate()
    {
        CheckBounds();
    }

    //최초 1회 소팅
    private void Sort()
    {
        objectList.Add(recycleQueue.Dequeue());
        //단일 오브젝트 리프레시.
        Onload?.Invoke(objectList[0].go, 0);
        objectList[0].go.SetActive(true);
        for (int i = 1; i < MaxSize; ++i)
        {
            //큐가 비어있으면 새로 생성.
            if (recycleQueue.Count <= 0)
            {
                GameObject obj = transform.AddChild(objectList[0].go);
                obj.name = objectList[0].go.name + i.ToString();
                obj.SetActive(false);
                recycleQueue.Enqueue(new Item() { poolIndex = i, go = obj});
            }

            objectList.Add(recycleQueue.Dequeue());
            //단일 오브젝트 리프레시.
            Onload?.Invoke(objectList[objectList.Count - 1].go, i);


            objectList[objectList.Count - 1].go.SetActive(true);
            switch (flow_direction)
            {
                case FlowDirection.BottomUP:    { MoveToTargetPosition(objectList.Count - 1, objectList.Count - 2, Pivot.Bottom); } break;
                case FlowDirection.UpBottom:    { MoveToTargetPosition(objectList.Count - 1, objectList.Count - 2, Pivot.Top); } break;
                case FlowDirection.LeftToRight: { MoveToTargetPosition(objectList.Count - 1, objectList.Count - 2, Pivot.Left); } break;
                case FlowDirection.RightToLeft: { MoveToTargetPosition(objectList.Count - 1, objectList.Count - 2, Pivot.Right); } break;
            }
        }
        scrollHeadindex = 0;
        currentHeadDataIndex = 0;
        scrollTailindex = MaxSize;
        currentTailDataIndex = MaxSize;
    }


    public void SetOnLoadCallback(System.Action<GameObject, int> action)
    {
        this.Onload = action;
    }

    public enum FlowDirection
    {
        BottomUP,
        UpBottom,
        LeftToRight,
        RightToLeft
    }

    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }

    public enum Pivot
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

    public enum InsideEdges   //사각형 기준
    {
        None = 0,
        Left = 3,
        Right = 12,
        Bottom = 6,
        Up = 9,
        LeftUP = 1,
        LeftBottom = 2,
        RightUp = 8,
        RightBottom = 4,
        All = 15, //전체포함
        Error1 = 5, //대각선 == 에러 (좌상 + 우하)
        Error3 = 10, //대각선 == 에러 (좌하 + 우상)
        Error2 = 7, //우상단 제외 전체 == 에러
        Error4 = 11, //우하단 제외 전체 == 에러
        Error5 = 13, //좌하단 제외 전체 == 에러
        Error6 = 14, //좌상단 제외 전체 == 에러
    }

    /// <summary>
    /// 데이터 인덱스 불러오기
    /// </summary>
    private int GetDataindex(bool IsHead)
    {
        //데이터가 범위를 벗어날 경우 오류 발생
        if (currentHeadDataIndex >= DataIndexMax || currentTailDataIndex <= 0 || currentTailDataIndex < currentHeadDataIndex)
            return -1000000;


        if (IsHead)
        {
            //헤드 데이터가 변경되는 경우 테일도 같이 줄어들음
            currentTailDataIndex--;

            //다만 헤드 값이 0 이하가 될수는 없지만 테일값은 줄어들수 있음
            if (currentHeadDataIndex <= 0)
                return 0;
            else
            {
                //헤드 데이터 인덱스 감쇠 후 리턴
                currentHeadDataIndex--;
                return currentHeadDataIndex;
            }
        }
        else
        {
            //테일 데이터가 변경되는 경우 헤드도 같이 늘어남
            currentHeadDataIndex++;

            //다만 데이터 인덱스 맥스 수치보다 커질수는 없지만 헤드 값이 늘어나는것은 가능.
            if (currentTailDataIndex >= DataIndexMax)
                return 0;
            else
            {
                //테일 값 증가 후 리턴
                currentTailDataIndex++;
                return currentTailDataIndex;
            }
        }
    }


    //바운더리 체크 후 그에 맞는 로직 실행.
    private void CheckBounds()
    {
        for (int i = 0; i < objectList.Count; ++i)
        {
            Bounds bounds = objectList[i].go.GetComponent<Collider>().bounds;
            if (area.Intersects(bounds))
            {
                //in the area;
                //no needs to refresh
                continue;
            }
            else
            {
                //outside of area
                if (recycleQueue.Count == 0)
                {
                    for (int j = 0; j < objectList.Count; ++j)
                    {
                        if (!area.Intersects(objectList[j].go.GetComponent<Collider>().bounds))
                        {
                            recycleQueue.Enqueue(objectList[j]);
                            objectList.Remove(objectList[j]);
                            j = 0;
                        }
                    }
                    scrollTailindex = objectList.Count - 1;
                    currentTailDataIndex = objectList.Count - 1;
                }
                else
                {
                    objectList[i].go.SetActive(false);
                    recycleQueue.Enqueue(objectList[i]);
                    objectList.RemoveAt(i);
                }

                //serch ClosestPoint for getting direction
                Vector3 nearpoint = area.ClosestPoint(bounds.center);
                //!!!주의점 MoveToTargetPosition은 타겟과 자신의 크기와 중심점 기준으로 이동하기 떄문에 OnLoad가 실행 된 이후 실행해줘야함. 안그러면 서로 겹치는 문제 발생 가능.

                //리사이클 큐에서 오브젝트 리스트로 등록
                objectList.Add(recycleQueue.Dequeue());

                switch (scrollArrangement)
                {
                    case Arrangement.Horizontal:
                        {
                            if (flow_direction == FlowDirection.LeftToRight)
                            {
                                if (area.center.x > nearpoint.x)
                                {
                                    //it's on Left

                                    //단일 오브젝트 리프레시.
                                    Onload?.Invoke(objectList[objectList.Count - 1].go, GetDataindex(true));
                                    //타겟 포지션으로 요소 꺼내와서 이동
                                    MoveToTargetPosition(objectList.Count - 1, scrollHeadindex, Pivot.Right);
                                    //스크롤 헤드의 인덱스 저장.
                                    scrollHeadindex = objectList.Count - 1;
                                }
                                else
                                {
                                    //it's On Rights

                                    //단일 오브젝트 리프레시.
                                    Onload?.Invoke(objectList[objectList.Count - 1].go, GetDataindex(false));
                                    //타겟 포지션으로 요소 꺼내와서 이동
                                    MoveToTargetPosition(objectList.Count - 1, scrollTailindex++, Pivot.Left);
                                    //스크롤 테일의 인덱스 저장
                                    scrollTailindex = objectList.Count - 1;
                                }
                            }
                            else if (flow_direction == FlowDirection.LeftToRight)
                            {
                                if (area.center.x > nearpoint.x)
                                {
                                    //it's on Left

                                    //단일 오브젝트 리프레시.
                                    Onload?.Invoke(objectList[objectList.Count - 1].go, GetDataindex(false));
                                    //타겟 포지션으로 요소 꺼내와서 이동
                                    MoveToTargetPosition(objectList.Count - 1, scrollTailindex++, Pivot.Right);
                                    //스크롤 테일의 인덱스 저장
                                    scrollTailindex = objectList.Count - 1;
                                }
                                else
                                {
                                    //it's On Rights

                                    //단일 오브젝트 리프레시.
                                    Onload?.Invoke(objectList[objectList.Count - 1].go, GetDataindex(true));
                                    //타겟 포지션으로 요소 꺼내와서 이동
                                    MoveToTargetPosition(objectList.Count - 1, scrollHeadindex, Pivot.Left);
                                    //스크롤 헤드의 인덱스 저장.
                                    scrollHeadindex = objectList.Count - 1;
                                }
                            }
                            else
                            {
                                //it's on Top or Bottom
                            }
                        }
                        break;
                    case Arrangement.Vertical:
                        {
                            if (flow_direction == FlowDirection.BottomUP)
                            {
                                
                                if (area.center.y > nearpoint.y)
                                {
                                    //it's on Bottom

                                    //단일 오브젝트 리프레시.
                                    Onload?.Invoke(objectList[objectList.Count - 1].go, GetDataindex(true));
                                    //타겟 포지션으로 요소 꺼내와서 이동
                                    MoveToTargetPosition(objectList.Count - 1, scrollHeadindex, Pivot.Top);
                                    //스크롤 헤드의 인덱스 저장.
                                    scrollHeadindex = objectList.Count - 1;
                                }
                                else
                                {
                                    //it's On Top

                                    //단일 오브젝트 리프레시 및 초기화..
                                    Onload?.Invoke(objectList[objectList.Count - 1].go, GetDataindex(false));
                                    //타겟 포지션으로 요소 이동.
                                    MoveToTargetPosition(objectList.Count - 1, scrollTailindex, Pivot.Bottom);
                                    //스크롤 테일의 인덱스 저장
                                    scrollTailindex = objectList.Count - 1;
                                }
                            }
                            else if (flow_direction == FlowDirection.UpBottom)
                            {
                                if (area.center.y > nearpoint.y)
                                {
                                    //it's on Bottom

                                    //단일 오브젝트 리프레시.
                                    Onload?.Invoke(objectList[objectList.Count - 1].go, GetDataindex(false));
                                    //타겟 포지션으로 요소 꺼내와서 이동
                                    MoveToTargetPosition(objectList.Count - 1, scrollTailindex, Pivot.Top);
                                    //스크롤 테일의 인덱스 저장
                                    scrollTailindex = objectList.Count - 1;
                                }
                                else
                                {
                                    //it's On Top

                                    //단일 오브젝트 리프레시.
                                    Onload?.Invoke(objectList[objectList.Count - 1].go, GetDataindex(true));
                                    //타겟 포지션으로 요소 이동.
                                    MoveToTargetPosition(objectList.Count - 1, scrollHeadindex, Pivot.Bottom);
                                    //스크롤 헤드의 인덱스 저장.
                                    scrollHeadindex = objectList.Count - 1;
                                }
                            }
                            else
                            {
                                //it's on Left or Right
                            }

                        }
                        break;
                }

                Debug.Log($"HeadIndex = {scrollHeadindex}");
                Debug.Log($"TailIndex = {scrollTailindex}");
            }
        }
    }
    
    /// <summary>
    /// 타겟의 특정 방향에 요소 이동
    /// </summary>
    public void MoveToTargetPosition(int index, int targetindex, Pivot direction)
    {
        GameObject obj = objectList[index].go;
        GameObject targetobj = objectList[targetindex].go;

        Bounds objbounds = obj.GetComponent<Collider>().bounds;
        Bounds target = targetobj.GetComponent<Collider>().bounds;
        switch (direction)
        {
            case Pivot.Top:      { obj.transform.position = targetobj.transform.position + (target.extents.y  +    objbounds.extents.y)  * Vector3.up;       } break;
            case Pivot.Bottom:   { obj.transform.position = targetobj.transform.position + (target.extents.y  +    objbounds.extents.y)  * Vector3.down;     } break;
            case Pivot.Left:     { obj.transform.position = targetobj.transform.position + (target.extents.x  +    objbounds.extents.x)  * Vector3.left;     } break;
            case Pivot.Right:    { obj.transform.position = targetobj.transform.position + (target.extents.x  +    objbounds.extents.x)  * Vector3.right;    } break;
        }
    }

    /// <summary>
    /// 리사이클 큐에서 아이템 받아와 해당하는 인덱스의 요소의 지정위치로 이동 후 object리스트에 추가.
    /// </summary>
    private void MoveToTargetPosition(Item recycled, int targetindex, Pivot direction)
    {
        GameObject obj = recycled.go;
        obj.SetActive(true);
        GameObject targetobj = objectList[targetindex].go;        

        Bounds objbounds = obj.GetComponent<Collider>().bounds;
        Bounds target = targetobj.GetComponent<Collider>().bounds;
        switch (direction)
        {
            case Pivot.Top: { obj.transform.localPosition = target.center + (target.extents.y * Vector3.up) + (objbounds.max.y * Vector3.down); } break;
            case Pivot.Bottom: { obj.transform.localPosition = target.center + (target.extents.y * Vector3.down) + (objbounds.min.y * Vector3.up); } break;
            case Pivot.Left: { obj.transform.localPosition = target.center + (target.extents.x * Vector3.left) + (objbounds.max.x * Vector3.right); } break;
            case Pivot.Right: { obj.transform.localPosition = target.center + (target.extents.x * Vector3.right) + (objbounds.min.x * Vector3.left); } break;
        }
        objectList.Add(recycled);
    }
    void RefreshTarget(int index, int flag, Arrangement dir)
    {
        switch (dir)
        {
            case Arrangement.Vertical:
                {
                    switch ((InsideEdges)flag)
                    {
                        case InsideEdges.None:
                            {
                                //totally outside
                            }
                            break;
                        case InsideEdges.Up:
                        case InsideEdges.LeftUP:
                        case InsideEdges.RightUp:
                            {
                                //bottom of area
                            }
                            break;
                        case InsideEdges.Bottom:
                        case InsideEdges.LeftBottom:
                        case InsideEdges.RightBottom:
                            {
                                //top of area
                            }
                            break;
                        default:
                            {
                                //Error;
                            }
                            break;
                    }
                }
                break;
            case Arrangement.Horizontal:
                {
                    switch ((InsideEdges)flag)
                    {
                        case InsideEdges.None:
                            {
                                //totally outside
                            }
                            break;
                        case InsideEdges.Left:
                        case InsideEdges.LeftUP:
                        case InsideEdges.LeftBottom:
                            {
                                //Left of area
                            }
                            break;
                        case InsideEdges.Right:
                        case InsideEdges.RightUp:
                        case InsideEdges.RightBottom:
                            {
                                //Right of area
                            }
                            break;

                        default:
                            {
                                //Error;
                            }
                            break;
                    }
                }
                break;
        }
    }

    public void InitItems(GameObject prefab, int dataSize)
    {
        DataIndexMax = dataSize;
        InitItems(prefab);
        Sort();
    }

    public void InitItems(GameObject prefab, int dataSize , int MaxCount)
    {
        InitItems(prefab, dataSize);
        MaxSize = MaxCount;
        Sort();
    }

    public void InitItems(GameObject prefab)
    {
        area = DragArea.bounds;
        int i = 0;
        prefab.SetActive(true);
        MaxSize = CalculateFitInArea(prefab.GetComponent<Collider>().bounds, flow_direction);
        prefab.SetActive(false);
        for (; i < MaxSize; ++i)
        {
            GameObject obj = transform.AddChild(prefab);
            obj.name = prefab.name + i.ToString();
            obj.SetActive(false);
            recycleQueue.Enqueue(new Item() { poolIndex = i, go = obj});
        }
    }

    public void SetUpdateAction(System.Action<GameObject, int> _updateAction)
    {
        Onload = _updateAction;
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
                }break;
            case FlowDirection.RightToLeft:
            case FlowDirection.LeftToRight:
                {
                    count = (int)(area.extents.x / bounds.extents.x);
                }
                break;
        }
        return count + 3;
    }

    private void AddItems()
    {
        Item item = recycleQueue.Dequeue();
        objectList.Add(item);
    }
}

