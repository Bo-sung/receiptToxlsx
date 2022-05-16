using UnityEngine;

namespace Ragnarok
{
    public class UIUnitViewer : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] Camera unitCamera;
        [SerializeField] Transform scale;
        [SerializeField] UITexture unitView;

        CharacterEntity entity;
        RenderTexture renderTexture;

        void Start()
        {
            renderTexture = unitCamera.RenderTexture(1024, 1024);
            unitView.mainTexture = renderTexture;
        }

        void OnDestroy()
        {
            DespawnActor();
            NGUITools.Destroy(renderTexture);
        }

        public void Show(CharacterEntity entity)
        {
            DespawnActor();
            this.entity = entity;
            CreateActor();
        }

        private void DespawnActor()
        {
            if (entity)
                entity.DespawnActor();
        }

        public UnitActor GetActor()
        {
            return entity?.GetActor();
        }

        private void CreateActor()
        {
            if (entity)
            {
                UnitActor unitActor = entity.SpawnActor();
                unitActor.SetParent(scale, worldPositionStays: false); // 배치
                NGUITools.SetLayer(scale.gameObject, Layer.UI_3D);
            }
        }

        void OnDrag(Vector2 delta)
        {
            Debug.Log(delta);
        }

        bool IInspectorFinder.Find()
        {
            // UnitCamera 세팅
            Transform cam = transform.Find("UnitCamera") ?? new GameObject("UnitCamera") { layer = Layer.UI_3D }.transform;
            cam.SetParent(transform);
            cam.localPosition = new Vector3(0f, 2000f, 0f);
            cam.localRotation = Quaternion.identity;
            cam.localScale = Vector3.one;
            unitCamera = cam.gameObject.AddMissingComponent<Camera>();
            unitCamera.clearFlags = CameraClearFlags.SolidColor;
            unitCamera.backgroundColor = Color.clear;
            unitCamera.cullingMask = 1 << Layer.UI_3D;
            unitCamera.orthographic = true;
            unitCamera.orthographicSize = 1f;
            unitCamera.nearClipPlane = -2f;
            unitCamera.farClipPlane = 2f;
            unitCamera.depth = 0f;

            // Scale 세팅
            scale = cam.Find("Scale") ?? new GameObject("Scale") { layer = Layer.UI_3D }.transform;
            scale.SetParent(cam);
            scale.localPosition = new Vector3(0f, -630, 0f);
            scale.localRotation = Quaternion.Euler(0f, 180f, 0f);
            scale.localScale = Vector3.one * 200f;
            return true;
        }

        public void SetViewSize(int size)
        {
            unitView.SetDimensions(size, size);
        }

        public void SetActive(bool isActive)
        {
            unitView.enabled = isActive;
        }
    }
}