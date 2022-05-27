using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDebugLabel : MonoBehaviour
{
    [SerializeField] UILabel Title;
    [SerializeField] UILabel Data;    
    public void SetData(string data)
    {
        Data.text = data;
    }
    public void SetTitle(string data)
    {
        Title.text = data;
    }
}
