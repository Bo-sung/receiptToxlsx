using UnityEngine;

public class CamsizeTogle : MonoBehaviour
{
    [SerializeField] int Camsize = 3;
    [SerializeField] bool _toggle = false;
    UIButton btn; 
    Camera camera;
    private void Awake()
    {
        camera = GameObject.Find("UI Root").GetComponentInChildren<Camera>();
        btn = GetComponent<UIButton>();
        EventDelegate.Add(btn.onClick, Toggle);
    }

    private void OnDestroy()
    {
        EventDelegate.Remove(btn.onClick, Toggle);
    }
    // Update is called once per frame
    void Update()
    {
        if(_toggle)
            camera.orthographicSize = Camsize;
        else
            camera.orthographicSize = 1;
    }
    void Toggle()
    {
        _toggle = !_toggle;
    }
}
