using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
//using UnityEngine.iOS;

public class CameraManager : MonoBehaviour
{
    WebCamTexture camTexture;

    public RawImage cameraViewImage;
    public UIButton uiButton;
    //public DebugTable debugTable;

    /// <summary>
    /// 카메라 작동시 호출
    /// </summary>
    public System.Action CamOn;
    /// <summary>
    /// 카메라 꺼질시 호출
    /// </summary>
    public System.Action CamOFF;
    /// <summary>
    /// 카메라 캡쳐시 호출
    /// </summary>
    public System.Action CamSnapshoted;



    public bool IsCamOn { get; private set; }

    public void Awake()
    {
        EventDelegate.Add(uiButton.onClick, OnToggleChange);
        IsCamOn = false;
    }

    public void OnDestroy()
    {
        
    }
    private void OnToggleChange()
    {
        IsCamOn = !IsCamOn;
        if(IsCamOn)
            CameraOn();
        else
            CameraOff();
    }

    public void CameraOn()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("NoCamera");
            //debugTable.Add("Camera", "False");
            return;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        int selectedCameraIndex = -1;

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                selectedCameraIndex = i;
                break;
            }
        }

        if (selectedCameraIndex >= 0)
        {
            camTexture = new WebCamTexture(devices[selectedCameraIndex].name);

            camTexture.requestedFPS = 30;

            cameraViewImage.texture = camTexture;

            camTexture.Play();
            CamOn?.Invoke();
        }
    }

    public void CameraOff()
    {
        if(camTexture != null)
        {
            camTexture.Stop();
            CamOFF?.Invoke();
            WebCamTexture.Destroy(camTexture);
            camTexture = null;
        }
    }

    public Texture2D CameraTakeSnapShot()
    {
        if (!IsCamOn)
        {
            //debugTable.Add("CameraTakeSnapshot", "camTexture => null");
            Debug.Log("camTexture == null");
            return new Texture2D(camTexture.width, camTexture.height);
        }
        Texture2D snapShot = new Texture2D(camTexture.width, camTexture.height);
        snapShot.SetPixels(camTexture.GetPixels());
        snapShot.Apply();
        CamSnapshoted?.Invoke();
        return snapShot;
    }
}
