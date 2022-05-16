using Cinemachine;
using Cinemachine.Editor;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomEditor(typeof(CinemachineZoom))]
    public class CinemachineZoomEditor : BaseEditor<CinemachineZoom>
    {
        private const float DEFAULT_ZOOM_IN_PLUS_VALUE = 10f;
        private const float DEFAULT_ZOOM_OUT_PLUS_VALUE = 20f;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty zoomInDistance = serializedObject.FindProperty(nameof(zoomInDistance));
            SerializedProperty zoomOutDistance = serializedObject.FindProperty(nameof(zoomOutDistance));

            float? cameraDistance = null;
            if (Target.VirtualCamera is CinemachineVirtualCamera vcam)
            {
                CinemachineFramingTransposer transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

                if (transposer == null)
                {
                    EditorGUILayout.HelpBox("CinemachineFramingTransposer 가 존재하지 않음", MessageType.Warning);
                }
                else
                {
                    cameraDistance = transposer.m_CameraDistance;
                }
            }

            EditorGUILayout.PropertyField(zoomInDistance);
            EditorGUILayout.PropertyField(zoomOutDistance);

            // 초기 세팅 값의 경우
            if (zoomInDistance.floatValue == 0f && zoomOutDistance.floatValue == 0f)
            {
                if (cameraDistance.HasValue)
                {
                    zoomInDistance.floatValue = cameraDistance.Value - DEFAULT_ZOOM_IN_PLUS_VALUE;
                    zoomOutDistance.floatValue = cameraDistance.Value + DEFAULT_ZOOM_OUT_PLUS_VALUE;
                }
            }

            zoomInDistance.floatValue = Mathf.Max(0f, zoomInDistance.floatValue); // 0보다 작은 값 제외

            serializedObject.ApplyModifiedProperties();
        }
    }
}