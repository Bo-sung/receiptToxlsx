using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTable : MonoBehaviour
{
    [SerializeField] GameObject Origin;
    public Dictionary<string, int> titleDic;
    public List<UIDebugLabel> gameObjects;

    UITable uITable;
    // Start is called before the first frame update
    void Awake()
    {
        uITable = GetComponent<UITable>();
        gameObjects = new List<UIDebugLabel>();
        titleDic = new Dictionary<string, int>();
    }
    public void Add(string Title, string Data)
    {
        if (titleDic.ContainsKey(Title))
        {
            gameObjects[titleDic[Title]].SetData(Data);
        }
        else
        {
            GameObject obj = Instantiate(Origin);
            UIDebugLabel debugLabel = obj.GetComponent<UIDebugLabel>();
            debugLabel.SetTitle(Title);
            debugLabel.SetData(Data);
            obj.SetActive(true);
            obj.transform.SetParent(this.transform);
            obj.transform.localScale = new Vector3(1, 1, 1);
            gameObjects.Add(debugLabel);
            titleDic.Add(Title, gameObjects.Count - 1);
        }
        uITable.enabled = true;
        uITable.repositionNow = true;
    }

    public void UpdateValue(string Title, string Data)
    {
        if (!titleDic.ContainsKey(Title))
            return;

        gameObjects[titleDic[Title]].SetData(Data);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
