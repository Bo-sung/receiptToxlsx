using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CombinerSettings", order = 1)]
public class CombinerSettings : ScriptableObject
{
    [Tooltip("가시성 체크를 위한 레이캐스트를 어느 정도 각도로 쏘아서 체크할 것인가")]
    [SerializeField]
    [Range(30.0f, 90.0f)]
    private float raycastAngle = 60.0f;
    [Tooltip("그리드의 사이즈를 어느정도로 할 것인가")]
    [SerializeField]
    private float gridSize = 36.0f;
    [Tooltip("매터리얼의 색상을 매시정보에 넣을 것인가")]
    [SerializeField]
    private bool materialColor2VertexColor = true;
    [Tooltip("윗면을 바라보는 삼각형만을 이용해 AABBTree 를 구성할 것인가 (체크하면 빠르지만, 일부 폴리곤에 대응하지 못할 수 있음.)")]
    [SerializeField]
    private bool genAABBTreeOnlyWithTopFace = true;

    public float RaycastAngle { get { return raycastAngle; } }
    public float GridSize { get { return gridSize; } }
    public bool MaterialColor2VertexColor { get { return materialColor2VertexColor; } }
    public bool GenAABBTreeOnlyWithTopFace { get { return genAABBTreeOnlyWithTopFace; } }
}