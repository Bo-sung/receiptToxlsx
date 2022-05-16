using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CubeMakeEditor : MonoBehaviour
{
    public GameObject CT_FirstPlane;
    public GameObject CT_SecondPlane;
    public GameObject CT_ThirdPlane;
    public GameObject CT_FourthPlane;
    public GameObject CT_FifthPlane;
    public GameObject CT_SixthPlane;

    public float CT_radius;

    float CT_X;
    float CT_Y;
    float CT_Z;

    void OnValidate()
    {
        CT_FirstPlane.transform.position = new Vector3(CT_X, CT_Y + CT_radius, CT_Z);
        CT_SecondPlane.transform.position = new Vector3(CT_X, CT_Y, CT_Z + CT_radius);
        CT_ThirdPlane.transform.position = new Vector3(CT_X + CT_radius, CT_Y, CT_Z);
        CT_FourthPlane.transform.position = new Vector3(CT_X, CT_Y, CT_Z - CT_radius);
        CT_FifthPlane.transform.position = new Vector3(CT_X - CT_radius, CT_Y, CT_Z);
        CT_SixthPlane.transform.position = new Vector3(CT_X, CT_Y - CT_radius, CT_Z);

        CT_SecondPlane.transform.rotation = Quaternion.Euler(-90, 0, 0);
        CT_ThirdPlane.transform.rotation = Quaternion.Euler(0, -90, 90);
        CT_FourthPlane.transform.rotation = Quaternion.Euler(90, 90, -90);
        CT_FifthPlane.transform.rotation = Quaternion.Euler(0, 90, -90);
        CT_SixthPlane.transform.rotation = Quaternion.Euler(0, 90, -180);
    }
}