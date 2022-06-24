using UnityEngine;

public class DebugObject : MonoBehaviour
{
    [SerializeField] UILabel debuglabel1;
    UISprite sprite;
    // Start is called before the first frame update
    void Start()
    {
        //UIRoot상의 content height값
        int rootsize = GameObject.Find("UI Root").GetComponent<UIRoot>().manualHeight;
        //디스플레이상의 렌더링 높이
        int renderingsize = Display.main.renderingHeight;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            renderingsize / -2,
            transform.localPosition.z
            );
        sprite = GetComponent<UISprite>();
        sprite.height = (renderingsize - rootsize) / 2;
        debuglabel1.text = ((renderingsize - rootsize) / 2).ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
