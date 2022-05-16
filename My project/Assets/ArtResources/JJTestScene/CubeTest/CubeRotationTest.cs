using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRotationTest : MonoBehaviour
{
    public GameObject CT_Body;
    public float CT_Spd = 10f;
    public Quaternion CT_rot;

    void Start()
    {
        StartCoroutine("Rotate");
    }

    IEnumerator Rotate()
    {
        CT_rot = Quaternion.identity;
        CT_rot.eulerAngles = Vector3.zero; // 일단 0, 0, 0 에서 시작한다고 가정 

        while (true)
        {
            CT_Body.transform.rotation = Quaternion.Lerp(CT_Body.transform.rotation, CT_rot, CT_Spd * Time.deltaTime);
            yield return null;

        }
    }

    public void RotationTo(float angleX, float angleY, float angleZ)
    {
        CT_rot = Quaternion.identity;
        CT_rot.eulerAngles = new Vector3(angleX, angleY, angleZ);
    }

    private void Update()
    {
        switch (Input.inputString)
        {
            case "1":
                RotationTo(0,0,0);
                break;
            case "2":
                RotationTo(-90,0,0);
                break;
            case "3":
                RotationTo(0,-90,90);
                break;
            case "4":
                RotationTo(90,90,-90);
                break;
            case "5":
                RotationTo(0,90,-90);
                break;
            case "6":
                RotationTo(0,90,-180);
                break;
        }
    }

}

