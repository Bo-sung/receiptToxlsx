using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testForChatWrapper : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] ChatWrapper wrapper;
    List<UILabel> lblist;
    private void Start()
    {
        lblist = new List<UILabel>();
        for(int i = 0; i < 100; ++i)
        {
            lblist.Add(new UILabel()
            { text = $"Test ({i})"});

        }

        wrapper.SetOnLoadCallback(OnUpdateItems);
        wrapper.InitItems(prefab, lblist.Count);
    }
    void OnUpdateItems(GameObject go, int index)
    {
        UILabel lbl = go.GetComponentInChildren<UILabel>();
        lbl.text = lblist[index].text;
    }

}
