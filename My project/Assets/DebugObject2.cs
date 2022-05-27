using UnityEngine;

public class DebugObject2 : MonoBehaviour
{
    [SerializeField] UILabel debuglabel1;
    UISprite sprite;
    // Start is called before the first frame update
    void Start()
    {
        //디스플레이상의 렌더링 높이
        int renderingsize = Display.main.renderingHeight;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            renderingsize / -2,
            transform.localPosition.z
            );
        sprite = GetComponent<UISprite>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
