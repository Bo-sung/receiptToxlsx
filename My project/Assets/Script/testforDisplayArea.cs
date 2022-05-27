using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testforDisplayArea : MonoBehaviour
{
    UISprite sprite;
    // Start is called before the first frame update
    void Start()
    {
        sprite = this.gameObject.GetComponent<UISprite>();
        sprite.height = Display.main.renderingHeight;
        sprite.width = Display.main.renderingWidth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
