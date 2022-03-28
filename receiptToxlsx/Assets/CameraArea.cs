using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraArea : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] CanvasRenderer canvasRenderer;
    [SerializeField] RawImage rawImage;
    [SerializeField] CameraManager camManager;
    [SerializeField] Rect original;
    [SerializeField] Rect CamArea;

    private void Awake()
    {
        camManager.CamOn += ShowCamArea;
        camManager.CamOFF += CloseCamArea;
    }
    private void OnDestroy()
    {
        camManager.CamOn -= ShowCamArea;
        camManager.CamOFF -= CloseCamArea;
    }
    void ShowCamArea()
    {
        gameObject.active = true;
    }

    void CloseCamArea()
    {
        gameObject.active = false;
    }

}
