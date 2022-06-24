using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestforUIRootContents : MonoBehaviour
{
    UISprite sprite;
    // Start is called before the first frame update
    void Start()
    {
        UIRoot uiroot = GameObject.Find("UI Root").GetComponent<UIRoot>();
        sprite = this.gameObject.GetComponent<UISprite>();
        sprite.height = uiroot.manualHeight;
        sprite.width = uiroot.manualWidth;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
