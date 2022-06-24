using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testForChatWrapper : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] DragWrapper wrapper;
    [SerializeField] List<string> lblist;
    public bool btn0 = false;
    public bool btn1 = false;
    public bool btn2 = false;
    private void Start()
    {
        lblist = new List<string>();
        for(int i = 0; i < 1; ++i)
        {
            lblist.Add($"Test ({i})");

            //outside of area
        }

        //wrapper1.SpawnNewList(prefab, 0, 0);
        //wrapper1.SetRefreshCallback(OnUpdateItems);
        //Resize(lblist.Count, progress: 1);



        wrapper.SetOnLoadCallback(OnUpdateItems);
        wrapper.Onload = OnUpdateItems;
        wrapper.OnItemsClicked = OnClickedItem;
        wrapper.InitItems(prefab, lblist.Count -1);
    }

    private void Update()
    {
        if(btn0)
        {
            for (int i = 0; i < 1; ++i)
            {
                lblist.Add($"Test ({lblist.Count})");
            }
            wrapper.DataIndexMax = lblist.Count -1;
            //Resize(lblist.Count, progress: 1);
            btn0 = false;
        }

        if (btn1)
        {
            ResetPosition();
            btn1 = false;
        }
    }
    void OnUpdateItems(GameObject go, int index)
    {
        UISprite spr = go.GetComponent<UISprite>();
        if (index == 10)
        {
            spr.height = 50;
        }
        else if(index == 20)
        {
            spr.height = 500;
        }
        else
        {
            spr.height = 200;
        }
        UILabel lbl = go.GetComponentInChildren<UILabel>();
        lbl.text = lblist[index];
    }

    void OnClickedItem(GameObject go, int index)
    {
        Debug.Log($"{go.name} {index}");
    }

    public void ResetPosition()
    {
    }

    public void Resize(int dataSize, int progress)
    {
    }
}
