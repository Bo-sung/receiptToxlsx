using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testTweener : MonoBehaviour
{
    [SerializeField] TweenScale tweenScale;
    [SerializeField] bool btn1;
    [SerializeField] bool btn2;
    private void Awake()
    {
        EventDelegate.Add(tweenScale.onFinished, aaaa);
    }
    void aaaa()
    {
        Debug.Log("tween finish!!");
        //tweenScale.PlayReverse();
        //tweenScale.Toggle();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(btn1)
        {
            //tweenScale.enabled = true;
            tweenScale.PlayReverse();
            btn1 = false;
        }

        if (btn2)
        {
            //tweenScale.enabled = true;
            tweenScale.PlayForward();
            btn2 = false;
        }

    }
}
