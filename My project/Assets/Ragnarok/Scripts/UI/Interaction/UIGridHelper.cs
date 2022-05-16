using UnityEngine;

namespace Ragnarok
{
    public class UIGridHelper : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UIGrid grid;
        [SerializeField] GameObject[] children;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void SetValue(int value)
        {
            for (int i = 0; i < children.Length; i++)
            {
                children[i].SetActive(i < value);
            }

            grid.Reposition();
        }

        public void Reposition()
        {
            grid.Reposition();
        }

        bool IInspectorFinder.Find()
        {
            grid = GetComponentInChildren<UIGrid>();

            Transform tf = grid.transform;
            children = new GameObject[tf.childCount];
            for (int i = 0; i < tf.childCount; ++i)
            {
                children[i] = tf.GetChild(i).gameObject;
            }
            return true;
        }

        public void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }
    }
}