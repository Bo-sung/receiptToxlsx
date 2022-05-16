using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UITweener))]
public class UITweenerSynchronizer : MonoBehaviour
{
    private static Dictionary<int, List<UITweenerSynchronizer>> synchronizers = new Dictionary<int, List<UITweenerSynchronizer>>();

    [SerializeField] int synchGroup;

    List<UITweenerSynchronizer> list;
    UITweener tweener;

    private void Awake()
    {
        tweener = GetComponent<UITweener>();
    }

    private void OnEnable()
    {
        if (!synchronizers.TryGetValue(synchGroup, out list))
        {
            list = new List<UITweenerSynchronizer>();
            synchronizers.Add(synchGroup, list);
        }

        list.Add(this);
        if (list.Count > 1)
            tweener.tweenFactor = list[0].tweener.tweenFactor;
    }

    private void Update()
    {
        if (list.Count > 1)
            tweener.tweenFactor = list[0].tweener.tweenFactor;
    }

    private void OnDisable()
    {
        list.Remove(this);
    }
}
