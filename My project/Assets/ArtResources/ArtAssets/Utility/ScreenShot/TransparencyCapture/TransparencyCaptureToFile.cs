using System.Collections;
using UnityEngine;

public class TransparencyCaptureToFile:MonoBehaviour
{
    public IEnumerator capture()
    {

        yield return new WaitForEndOfFrame();
        //After Unity4,you have to do this function after WaitForEndOfFrame in Coroutine
        //Or you will get the error:"ReadPixels was called to read pixels from system frame buffer, while not inside drawing frame"
        zzTransparencyCapture.captureScreenshot(string.Format("capture{0}{1}.png", System.DateTime.Now.Minute, System.DateTime.Now.Second));
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            StartCoroutine(capture());
    }
}